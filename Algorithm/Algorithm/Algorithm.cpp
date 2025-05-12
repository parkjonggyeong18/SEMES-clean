#define _CRT_SECURE_NO_WARNINGS
#include <iostream>
#include <fstream>
#include <vector>
#include <string>
#include <unordered_map>
#include <chrono>
#include <omp.h>
#include <immintrin.h>
#include <windows.h>
#include <charconv>
#include <climits>
#include <iomanip>
#include <algorithm>
#include <numeric>
using namespace std;

// 이물질 검출을 위한 임계값 및 PCB 크기 상수
constexpr float THRESHOLD = 2.5f;
constexpr float PCB_WIDTH_UM = 240000.0f;
constexpr float PCB_HEIGHT_UM = 77500.0f;
constexpr float PCB_LENGTH_UM = 250000.0f;

/**
 * @brief 이물질(Blob) 정보를 저장하는 구조체
 */
struct BlobInfo {
    int minX = INT_MAX, minY = INT_MAX, maxX = 0, maxY = 0;
    int area = 0; // 이물질 면적(픽셀 수)
};

/**
 * @brief 평면 피팅을 위한 구조체
 * @details z = ax + by + c 형태의 평면 방정식 계수를 저장
 */
struct PlaneCoeffs {
    float a, b, c; // z = ax + by + c
};

/**
 * @brief 숫자를 Excel 열 형식으로 변환
 * @param num 변환할 숫자
 * @return Excel 열 문자열 (예: 1 -> A, 27 -> AA)
 */
static string toExcelColumn(int num) {
    string col;
    while (num > 0) {
        num--;
        col = char('A' + (num % 26)) + col;
        num /= 26;
    }
    return col;
}

/**
 * @brief 파일을 메모리에 매핑
 * @param path 파일 경로
 * @param filedata 매핑된 파일 데이터를 저장할 포인터
 * @param filesize 파일 크기를 저장할 변수
 * @return 파일 매핑 핸들, 실패 시 nullptr
 */
static HANDLE mapFile(const string& path, char*& filedata, DWORD& filesize) {
    HANDLE hFile = CreateFileA(path.c_str(), GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
    if (hFile == INVALID_HANDLE_VALUE) return nullptr;
    HANDLE hMap = CreateFileMapping(hFile, NULL, PAGE_READONLY, 0, 0, NULL);
    if (!hMap) return nullptr;
    filedata = (char*)MapViewOfFile(hMap, FILE_MAP_READ, 0, 0, 0);
    filesize = GetFileSize(hFile, NULL);
    return hMap;
}

/**
 * @brief CSV 파일의 행과 열 수를 계산하고 각 행의 시작 오프셋을 저장
 * @param filedata 파일 데이터
 * @param filesize 파일 크기
 * @param line_offsets 각 행의 시작 오프셋을 저장할 벡터
 * @param rows 행 수를 저장할 변수
 * @param cols 열 수를 저장할 변수
 */
static void computeOffsets(char* filedata, DWORD filesize, vector<size_t>& line_offsets, int& rows, int& cols) {
    line_offsets.push_back(0);
    for (DWORD i = 0; i < filesize; i++) {
        if (filedata[i] == '\n') line_offsets.push_back(i + 1);
    }
    rows = line_offsets.size() - 1;
    cols = 0;
    for (DWORD i = line_offsets[0]; i < filesize && filedata[i] != '\n'; i++) {
        if (filedata[i] == ',') cols++;
    }
    cols++;
}

/**
 * @brief CSV 파일 데이터를 파싱하여 2D 배열에 저장
 * @param flat 데이터를 저장할 2D 배열
 * @param filedata 파일 데이터
 * @param line_offsets 각 행의 시작 오프셋
 * @param rows 행 수
 * @param cols 열 수
 */
static void parseCSV(float* flat, char* filedata, const vector<size_t>& line_offsets, int rows, int cols) {
#pragma omp parallel for schedule(dynamic)
    for (int y = 0; y < rows; y++) {
        const char* start = filedata + line_offsets[y];
        const char* end = filedata + (y + 1 < line_offsets.size() ? line_offsets[y + 1] : 0);
        int x = 0;
        while (start < end && x < cols) {
            float val = 0;
            auto [ptr, ec] = from_chars(start, end, val);
            if (ec == errc()) {
                flat[y * cols + x++] = val;
                start = (*ptr == ',' || *ptr == '\n') ? ptr + 1 : ptr;
            }
            else {
                flat[y * cols + x++] = 0.0f;
                while (start < end && *start != ',' && *start != '\n') ++start;
                start++;
            }
        }
    }
}

/**
 * @brief 최소 제곱법을 사용하여 데이터에 평면을 피팅
 * @param data 높이 데이터
 * @param rows 행 수
 * @param cols 열 수
 * @return 피팅된 평면의 계수 (z = ax + by + c)
 */
static PlaneCoeffs fitPlane(const float* data, int rows, int cols) {
    cout << "[평면 피팅 시작]" << endl;

    // DI 영역을 제외한 유효 데이터 포인트 수집
    vector<float> x_vals, y_vals, z_vals;
    x_vals.reserve(rows * (cols - 480));
    y_vals.reserve(rows * (cols - 480));
    z_vals.reserve(rows * (cols - 480));

    for (int y = 0; y < rows; y++) {
        for (int x = 240; x < cols - 240; x++) {
            float z = data[y * cols + x];
            if (z > 0.1f) { // 유효한 데이터만 사용
                x_vals.push_back(static_cast<float>(x));
                y_vals.push_back(static_cast<float>(y));
                z_vals.push_back(z);
            }
        }
    }

    // 데이터 포인트가 충분하지 않으면 기본값 반환
    if (x_vals.size() < 10) {
        cout << "[경고] 유효 데이터 포인트가 부족합니다." << endl;
        return { 0.0f, 0.0f, 0.0f };
    }

    // 평균 계산
    float mean_x = accumulate(x_vals.begin(), x_vals.end(), 0.0f) / x_vals.size();
    float mean_y = accumulate(y_vals.begin(), y_vals.end(), 0.0f) / y_vals.size();
    float mean_z = accumulate(z_vals.begin(), z_vals.end(), 0.0f) / z_vals.size();

    // 최소 제곱법을 사용한 평면 피팅 (z = ax + by + c)
    float sum_xx = 0.0f, sum_xy = 0.0f, sum_xz = 0.0f;
    float sum_yy = 0.0f, sum_yz = 0.0f;

    for (size_t i = 0; i < x_vals.size(); i++) {
        float x_centered = x_vals[i] - mean_x;
        float y_centered = y_vals[i] - mean_y;
        float z_centered = z_vals[i] - mean_z;

        sum_xx += x_centered * x_centered;
        sum_xy += x_centered * y_centered;
        sum_xz += x_centered * z_centered;
        sum_yy += y_centered * y_centered;
        sum_yz += y_centered * z_centered;
    }

    // 연립방정식 해결
    float det = sum_xx * sum_yy - sum_xy * sum_xy;
    if (fabs(det) < 1e-6f) {
        cout << "[경고] 행렬식이 0에 가깝습니다. 평면 피팅 실패." << endl;
        return { 0.0f, 0.0f, 0.0f };
    }

    float a = (sum_xz * sum_yy - sum_xy * sum_yz) / det;
    float b = (sum_xx * sum_yz - sum_xy * sum_xz) / det;
    float c = mean_z - a * mean_x - b * mean_y;

    cout << "[평면 피팅 결과] z = " << a << "x + " << b << "y + " << c << endl;
    return { a, b, c };
}

/**
 * @brief 기울어진 데이터를 평탄화
 * @param data 높이 데이터
 * @param rows 행 수
 * @param cols 열 수
 */
static void flattenData(float* data, int rows, int cols) {
    cout << "[데이터 평탄화 시작]" << endl;

    // 평면 피팅
    PlaneCoeffs plane = fitPlane(data, rows, cols);

    // 기울기가 거의 없으면 평탄화 생략
    if (fabs(plane.a) < 1e-5f && fabs(plane.b) < 1e-5f) {
        cout << "[정보] 데이터가 이미 평탄합니다. 평탄화 생략." << endl;
        return;
    }

    // 데이터 평탄화 (기울기 보정)
#pragma omp parallel for
    for (int y = 0; y < rows; y++) {
        for (int x = 0; x < cols; x++) {
            int idx = y * cols + x;
            if (data[idx] > 0.1f) { // 유효한 데이터만 보정
                // 평면 방정식에 따른 예상 높이 계산
                float expected_z = plane.a * x + plane.b * y + plane.c;
                // 기울기 보정
                data[idx] -= expected_z - plane.c;
            }
        }
    }

    cout << "[데이터 평탄화 완료]" << endl;
}

/**
 * @brief 높이 데이터를 임계값에 따라 이진화
 * @param flat 높이 데이터
 * @param binary 이진화 결과를 저장할 배열
 * @param rows 행 수
 * @param cols 열 수
 */
static void binarize(const float* flat, uint8_t* binary, int rows, int cols) {
#pragma omp parallel for
    for (int y = 0; y < rows; y++) {
        int x = 0;
        for (; x + 7 < cols; x += 8) {
            __m256 vals = _mm256_loadu_ps(&flat[y * cols + x]);
            __m256 thresh = _mm256_set1_ps(THRESHOLD);
            __m256 result = _mm256_cmp_ps(vals, thresh, _CMP_GE_OS);
            int mask = _mm256_movemask_ps(result);
            for (int i = 0; i < 8; i++) binary[y * cols + x + i] = (mask >> i) & 1;
        }
        for (; x < cols; x++) {
            binary[y * cols + x] = flat[y * cols + x] >= THRESHOLD;
        }
    }
#pragma omp parallel for
    for (int y = 0; y < rows; y++) {
        for (int x = 0; x < 240; x++) {
            binary[y * cols + x] = 0;
            binary[y * cols + (cols - 1 - x)] = 0;
        }
    }
}

/**
 * @brief 이진화된 이미지에서 활성화된 픽셀 수 계산
 * @param bin 이진화된 이미지
 * @param rows 행 수
 * @param cols 열 수
 * @return 활성화된 픽셀 수
 */
static int estimateDefectCount(const uint8_t* bin, int rows, int cols) {
    int total = 0;
#pragma omp parallel for reduction(+:total)
    for (int i = 0; i < rows * cols; ++i) {
        total += bin[i];
    }
    return total; // 실제로 활성화된 픽셀 수 반환
}

/**
 * @brief DFS 알고리즘을 사용하여 이물질(Blob) 검출
 * @param binary 이진화된 이미지
 * @param rows 행 수
 * @param cols 열 수
 * @return 검출된 이물질 정보 벡터
 */
static vector<BlobInfo> detectBlobsDFS(const uint8_t* binary, int rows, int cols) {
    cout << "[DFS 탐색 시작]" << endl;
    vector<BlobInfo> blobs;
    const int MAX_STACK = 32768;
    struct Coord { int y, x; };
    Coord* stack = new Coord[MAX_STACK];
    uint8_t* visited = new uint8_t[rows * cols]();
    int dx[4] = { 1, -1, 0, 0 }, dy[4] = { 0, 0, 1, -1 };

    for (int y = 0; y < rows; y++) {
        for (int x = 0; x < cols; x++) {
            int idx = y * cols + x;
            if (binary[idx] && !visited[idx]) {
                blobs.emplace_back();
                auto& blob = blobs.back();
                int top = 0;
                stack[top++] = { y, x };
                visited[idx] = 1;

                while (top > 0) {
                    Coord cur = stack[--top];
                    blob.minY = min(blob.minY, cur.y); blob.maxY = max(blob.maxY, cur.y);
                    blob.minX = min(blob.minX, cur.x); blob.maxX = max(blob.maxX, cur.x);
                    blob.area++; // 면적 계산

                    for (int d = 0; d < 4; d++) {
                        int ny = cur.y + dy[d], nx = cur.x + dx[d];
                        if (ny >= 0 && ny < rows && nx >= 0 && nx < cols) {
                            int nidx = ny * cols + nx;
                            if (binary[nidx] && !visited[nidx]) {
                                visited[nidx] = 1;
                                if (top < MAX_STACK)
                                    stack[top++] = { ny, nx };
                            }
                        }
                    }
                }
            }
        }
    }
    delete[] stack;
    delete[] visited;
    return blobs;
}

/**
 * @brief BFS 알고리즘을 사용하여 이물질(Blob) 검출
 * @param bin 이진화된 이미지
 * @param rows 행 수
 * @param cols 열 수
 * @return 검출된 이물질 정보 벡터
 */
static vector<BlobInfo> detectBlobsBFS(const uint8_t* bin, int rows, int cols) {
    cout << "[BFS 탐색 시작]" << endl;
    vector<BlobInfo> blobs;
    vector<uint8_t> vis(rows * cols, 0);
    int dx[4] = { 1, -1, 0, 0 }, dy[4] = { 0, 0, 1, -1 };
    struct P { int y, x; };

    const size_t QUEUE_CAPACITY = static_cast<size_t>(rows) * cols;
    P* queue = new P[QUEUE_CAPACITY];
    size_t qsize = 0, head = 0;

    for (int y = 0; y < rows; ++y) {
        for (int x = 0; x < cols; ++x) {
            int idx = y * cols + x;
            if (bin[idx] && !vis[idx]) {
                blobs.emplace_back();
                auto& B = blobs.back();
                qsize = 0; head = 0;
                queue[qsize++] = { y, x };
                vis[idx] = 1;

                while (head < qsize) {
                    P cur = queue[head++];
                    int cy = cur.y, cx = cur.x;
                    B.minY = min(B.minY, cy); B.maxY = max(B.maxY, cy);
                    B.minX = min(B.minX, cx); B.maxX = max(B.maxX, cx);
                    B.area++; // 면적 계산

                    for (int d = 0; d < 4; ++d) {
                        int ny = cy + dy[d], nx = cx + dx[d];
                        if (ny >= 0 && ny < rows && nx >= 0 && nx < cols) {
                            int nidx = ny * cols + nx;
                            if (bin[nidx] && !vis[nidx]) {
                                vis[nidx] = 1;
                                if (qsize < QUEUE_CAPACITY)
                                    queue[qsize++] = { ny, nx };
                                else
                                    cerr << "[오류] BFS 큐 초과: (" << ny << ", " << nx << ")\n";
                            }
                        }
                    }
                }
            }
        }
    }

    delete[] queue;
    return blobs;
}

/**
 * @brief 이물질 수에 따라 적응적으로 DFS 또는 BFS 알고리즘 선택
 * @param bin 이진화된 이미지
 * @param rows 행 수
 * @param cols 열 수
 * @return 검출된 이물질 정보 벡터
 */
static vector<BlobInfo> detectBlobsAdaptive(const uint8_t* bin, int rows, int cols) {
    int estimate = estimateDefectCount(bin, rows, cols);
    cout << "[DEBUG] 추정 blob 수: " << estimate << endl;

    if (estimate < 70000)
        return detectBlobsDFS(bin, rows, cols);
    else
        return detectBlobsBFS(bin, rows, cols);
}

/**
 * @brief 검출된 이물질 정보를 파일로 출력하고 콘솔에 표시
 * @param blobs 이물질 정보 벡터
 * @param cols 열 수
 * @param rows 행 수
 * @param path 출력 파일 경로
 */
static void outputBlobs(const vector<BlobInfo>& blobs, int cols, int rows, const string& path) {
    float um_per_pixel_x = PCB_LENGTH_UM / static_cast<float>(cols);
    float um_per_pixel_y = PCB_HEIGHT_UM / static_cast<float>(rows);
    float um2_per_pixel = um_per_pixel_x * um_per_pixel_y; // 픽셀당 면적(μm²)

    ofstream fout(path);
    fout << "Blob,ExcelRange,Area(pixels),Area(mm²)\n";
    int idx = 1;
    for (const auto& b : blobs) {
        string start = toExcelColumn(b.minX + 1) + to_string(b.minY + 1);
        string end = toExcelColumn(b.maxX + 1) + to_string(b.maxY + 1);
        float area_mm2 = (b.area * um2_per_pixel) / 1e6f; // μm²에서 mm²로 변환
        fout << idx << "," << start << "-" << end << "," << b.area << "," << fixed << setprecision(4) << area_mm2 << "\n";
        cout << "[Blob " << idx << "] Excel 위치: " << start << "-" << end << endl;
        idx++;
    }
    fout.close();

    cout << "[이물질 위치]" << endl;
    idx = 1;
    for (const auto& b : blobs) {
        float x1_mm = (b.minX * um_per_pixel_x) / 1000.0f;
        float y1_mm = (b.minY * um_per_pixel_y) / 1000.0f;
        float x2_mm = (b.maxX * um_per_pixel_x) / 1000.0f;
        float y2_mm = (b.maxY * um_per_pixel_y) / 1000.0f;
        float avg_x = (x1_mm + x2_mm) / 2.0f;
        float avg_y = (y1_mm + y2_mm) / 2.0f;
        float area_mm2 = (b.area * um2_per_pixel) / 1e6f;

        cout << fixed << setprecision(3);
        cout << "Blob: " << idx++ << endl;
        cout << "시작 끝: " << y1_mm << "mm " << x1_mm << "mm ~ " << y2_mm << "mm " << x2_mm << "mm" << endl;
        cout << "이물질 위치: X: " << avg_x << "mm, Y: " << avg_y << "mm" << endl;
        cout << "이물질 면적: " << b.area << " 픽셀 (" << area_mm2 << " mm²)" << endl;
    }
}

/**
 * @brief 메인 함수
 * @return 프로그램 종료 코드
 */
int main() {
    cout << "[PCB 이물질 감지 시작]\n";
    auto start = chrono::high_resolution_clock::now();

    char* filedata = nullptr;
    DWORD filesize = 0;
    HANDLE hMap = mapFile("C:/Users/SSAFY/Desktop/final_pcb_with_dies_and_defects14.csv", filedata, filesize);
    if (!hMap || !filedata) return -1;

    vector<size_t> line_offsets;
    int rows = 0, cols = 0;
    computeOffsets(filedata, filesize, line_offsets, rows, cols);

    float* flat = new float[rows * cols];
    parseCSV(flat, filedata, line_offsets, rows, cols);

    // 데이터 평탄화 적용
    flattenData(flat, rows, cols);

    uint8_t* binary = new uint8_t[rows * cols];
    binarize(flat, binary, rows, cols);
    delete[] flat;

    auto start2 = chrono::high_resolution_clock::now();
    vector<BlobInfo> blobs = detectBlobsAdaptive(binary, rows, cols);
    auto end = chrono::high_resolution_clock::now();
    delete[] binary;

    outputBlobs(blobs, cols, rows, "C:/Users/SSAFY/Desktop/defect_coordinates_excel.csv");

    cout << "총 실행 시간: " << chrono::duration<double>(end - start).count() << "초" << endl;
    cout << "Blob 탐색 시간: " << chrono::duration<double>(end - start2).count() << "초" << endl;
    cout << "이물질 개수: " << blobs.size() << endl;

    UnmapViewOfFile(filedata);
    CloseHandle(hMap);
    return 0;
}
