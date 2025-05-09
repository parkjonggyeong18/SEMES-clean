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
#include <ctime>  // rand와 srand를 위한 헤더
#include <cstdlib>
using namespace std;

// === 상수 정의 ===
constexpr float THRESHOLD = 2.5f;
constexpr float PCB_WIDTH_UM = 240000.0f;
constexpr float PCB_HEIGHT_UM = 77500.0f;
constexpr float PCB_LENGTH_UM = 250000.0f; // 전체 스캔 길이 (여유 포함)

struct BlobInfo {
    int minX = INT_MAX, minY = INT_MAX, maxX = 0, maxY = 0;
};

// RANSAC을 위한 3D 포인트 구조체
struct Point3D {
    float x, y, z;
    Point3D(float _x, float _y, float _z) : x(_x), y(_y), z(_z) {}
};

// 평면 방정식: ax + by + c = z
struct PlaneModel {
    float a, b, c;

    // 평면에서 점까지의 거리 계산
    float distanceToPoint(const Point3D& p) const {
        return abs(a * p.x + b * p.y + c - p.z);
    }

    // 3개의 점으로 평면 방정식 계산
    static PlaneModel fromPoints(const Point3D& p1, const Point3D& p2, const Point3D& p3) {
        // 평면상의 두 벡터 계산
        float v1x = p2.x - p1.x, v1y = p2.y - p1.y, v1z = p2.z - p1.z;
        float v2x = p3.x - p1.x, v2y = p3.y - p1.y, v2z = p3.z - p1.z;

        // 법선 벡터 계산 (벡터의 외적)
        float nx = v1y * v2z - v1z * v2y;
        float ny = v1z * v2x - v1x * v2z;
        float nz = v1x * v2y - v1y * v2x;

        // 법선 벡터가 유효한지 확인 (0이면 세 점이 일직선 상에 있음)
        if (abs(nz) < 1e-6) {
            throw std::runtime_error("세 점이 일직선 상에 있어 평면을 정의할 수 없습니다");
        }

        // 평면 방정식 계수 계산
        float a = nx / nz;
        float b = ny / nz;
        float c = (p1.z - a * p1.x - b * p1.y);

        return { a, b, c };
    }
};

// RANSAC 알고리즘으로 평면 찾기
PlaneModel findPCBSurfaceRANSAC(const vector<Point3D>& points, float distance_threshold, int max_iterations) {
    int best_inliers = 0;
    PlaneModel best_model = { 0, 0, 0 };

    // 난수 생성기 초기화
    srand(static_cast<unsigned>(time(nullptr)));

    // 최대 반복 횟수만큼 시도
    for (int iter = 0; iter < max_iterations; iter++) {
        // 1. 무작위로 3개 점 선택
        int idx1 = rand() % points.size();
        int idx2 = rand() % points.size();
        int idx3 = rand() % points.size();

        // 같은 점이 선택되지 않도록
        if (idx1 == idx2 || idx1 == idx3 || idx2 == idx3) continue;

        // 2. 선택한 3개 점으로 평면 모델 생성
        try {
            PlaneModel model = PlaneModel::fromPoints(
                points[idx1], points[idx2], points[idx3]);

            // 3. 모델에 잘 맞는 점 개수 세기
            int inliers = 0;
            for (const auto& p : points) {
                if (model.distanceToPoint(p) < distance_threshold) {
                    inliers++;
                }
            }

            // 4. 가장 많은 점이 맞는 모델 저장
            if (inliers > best_inliers) {
                best_inliers = inliers;
                best_model = model;
            }
        }
        catch (...) {
            // 평면 계산 중 오류 발생 시 무시하고 계속
            continue;
        }
    }

    cout << "RANSAC 결과: 전체 " << points.size() << "개 점 중 "
        << best_inliers << "개 점이 평면에 맞음 ("
        << (best_inliers * 100.0f / points.size()) << "%)" << endl;

    return best_model;
}

// 평탄화 함수 수정: 통계 정보 추가 및 임계값 조정
void flattenPCBSurface(float* flat, int rows, int cols, float& new_threshold) {
    // 1. Die 영역(높이=0)을 제외한 유효한 데이터 포인트 수집
    vector<Point3D> valid_points;
    valid_points.reserve(rows * cols / 2);

    for (int y = 0; y < rows; y++) {
        for (int x = 0; x < cols; x++) {
            float z = flat[y * cols + x];
            if (z > 0.0f) {  // Die 영역 제외(높이=0)
                valid_points.emplace_back(x, y, z);
            }
        }
    }

    // 유효한 점이 너무 적으면 평탄화 작업 중단
    if (valid_points.size() < 100) {
        cout << "유효한 데이터 포인트가 너무 적습니다." << endl;
        return;
    }

    // 2. RANSAC으로 PCB 표면의 평면 방정식 찾기
    cout << "RANSAC 알고리즘으로 PCB 표면 찾는 중..." << endl;
    float distance_threshold = 0.5f;  // 평면에서 점까지 허용 거리
    int max_iterations = 1000;        // RANSAC 최대 반복 횟수
    PlaneModel plane = findPCBSurfaceRANSAC(valid_points, distance_threshold, max_iterations);

    cout << "평면 방정식: z = " << plane.a << "x + " << plane.b << "y + " << plane.c << endl;

    // 3. 모든 점에서 기울기 제거하여 평탄화
    float min_val = FLT_MAX, max_val = -FLT_MAX, sum = 0;
    int count = 0;
    vector<float> deviations;  // 기울기 제거 후 편차를 저장할 벡터

#pragma omp parallel for
    for (int y = 0; y < rows; y++) {
        for (int x = 0; x < cols; x++) {
            float& z = flat[y * cols + x];
            if (z > 0.0f) {  // Die 영역은 그대로 유지
                // 예상 기본 높이 계산
                float expected_z = plane.a * x + plane.b * y + plane.c;
                // 평탄화: 실제 높이 - 예상 기본 높이
                float deviation = z - expected_z;
                z = deviation;

                // 통계 계산
#pragma omp critical
                {
                    min_val = min(min_val, deviation);
                    max_val = max(max_val, deviation);
                    sum += deviation;
                    count++;
                    deviations.push_back(deviation);
                }
            }
        }
    }

    // 평탄화 후 데이터 분포 확인
    float mean = sum / count;

    // 표준편차 계산
    float variance_sum = 0;
    for (const float& dev : deviations) {
        variance_sum += (dev - mean) * (dev - mean);
    }
    float std_dev = sqrt(variance_sum / deviations.size());

    cout << "평탄화 후 데이터 통계:" << endl;
    cout << "  최소값: " << min_val << endl;
    cout << "  최대값: " << max_val << endl;
    cout << "  평균: " << mean << endl;
    cout << "  표준편차: " << std_dev << endl;

    // 이물질 검출을 위한 새로운 임계값 설정
    // 통계 기반으로 설정 - 일반적으로 평균 + 3*표준편차는 정상적인 범위를 벗어난 것으로 봄
    new_threshold = mean + 3 * std_dev;
    cout << "새로운 임계값: " << new_threshold << endl;

    cout << "PCB 표면 평탄화 완료!" << endl;
}

static string toExcelColumn(int num) {
    string col;
    while (num > 0) {
        num--;
        col = char('A' + (num % 26)) + col;
        num /= 26;
    }
    return col;
}

static HANDLE mapFile(const string& path, char*& filedata, DWORD& filesize) {
    HANDLE hFile = CreateFileA(path.c_str(), GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
    if (hFile == INVALID_HANDLE_VALUE) return nullptr;
    HANDLE hMap = CreateFileMapping(hFile, NULL, PAGE_READONLY, 0, 0, NULL);
    if (!hMap) return nullptr;
    filedata = (char*)MapViewOfFile(hMap, FILE_MAP_READ, 0, 0, 0);
    filesize = GetFileSize(hFile, NULL);
    return hMap;
}

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
static void binarize(const float* flat, uint8_t* binary, int rows, int cols, float new_threshold) {
#pragma omp parallel for
    for (int y = 0; y < rows; y++) {
        int x = 0;
        for (; x + 7 < cols; x += 8) {
            __m256 vals = _mm256_loadu_ps(&flat[y * cols + x]);
            __m256 thresh = _mm256_set1_ps(new_threshold);
            __m256 result = _mm256_cmp_ps(vals, thresh, _CMP_GE_OS);
            int mask = _mm256_movemask_ps(result);
            for (int i = 0; i < 8; i++) binary[y * cols + x + i] = (mask >> i) & 1;
        }
        for (; x < cols; x++) {
            binary[y * cols + x] = flat[y * cols + x] >= new_threshold;
        }
    }

    // ✅ 가감속 구간 제거 (X축 기준: 좌우 240픽셀 제거)
#pragma omp parallel for
    for (int y = 0; y < rows; y++) {
        for (int x = 0; x < 240; x++) {
            binary[y * cols + x] = 0;                      // 좌측
            binary[y * cols + (cols - 1 - x)] = 0;         // 우측
        }
    }
}

static vector<BlobInfo> detectBlobs(const uint8_t* binary, int rows, int cols) {
    vector<BlobInfo> blobs;
    uint8_t* visited = new uint8_t[rows * cols]();
    int dx[] = { 1, -1, 0, 0 }, dy[] = { 0, 0, 1, -1 };
    struct Coord { int y, x; };

    for (int y = 0; y < rows; y++) {
        for (int x = 0; x < cols; x++) {
            if (binary[y * cols + x] && !visited[y * cols + x]) {
                blobs.emplace_back();
                vector<Coord> stack;
                stack.reserve(8192);
                stack.push_back({ y, x });
                visited[y * cols + x] = 1;
                while (!stack.empty()) {
                    Coord cur = stack.back(); stack.pop_back();
                    auto& blob = blobs.back();
                    blob.minY = min(blob.minY, cur.y); blob.maxY = max(blob.maxY, cur.y);
                    blob.minX = min(blob.minX, cur.x); blob.maxX = max(blob.maxX, cur.x);
                    for (int d = 0; d < 4; d++) {
                        int ny = cur.y + dy[d], nx = cur.x + dx[d];
                        if (ny >= 0 && ny < rows && nx >= 0 && nx < cols) {
                            int idx = ny * cols + nx;
                            if (binary[idx] && !visited[idx]) {
                                visited[idx] = 1;
                                stack.push_back({ ny, nx });
                            }
                        }
                    }
                }
            }
        }
    }
    delete[] visited;
    return blobs;
}

static void outputBlobs(const vector<BlobInfo>& blobs, int cols, int rows, const string& path) {
    float um_per_pixel_x = PCB_LENGTH_UM / static_cast<float>(cols);
    float um_per_pixel_y = PCB_HEIGHT_UM / static_cast<float>(rows);
    ofstream fout(path);
    fout << "Blob,ExcelRange\n";
    int idx = 1;
    for (const auto& b : blobs) {
        string start = toExcelColumn(b.minX + 1) + to_string(b.minY + 1);
        string end = toExcelColumn(b.maxX + 1) + to_string(b.maxY + 1);
        fout << idx << "," << start << "-" << end << "\n";
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

        cout << fixed << setprecision(3);
        cout << "Blob: " << idx++ << endl;
        cout << "시작 끝: " << y1_mm << "mm " << x1_mm << "mm ~ " << y2_mm << "mm " << x2_mm << "mm" << endl;
        cout << "이물질 위치: X: " << avg_x << "mm, Y: " << avg_y << "mm" << endl;
    }
}

int main() {
    cout << "[PCB 이물질 감지 시작]\n";
    auto start = chrono::high_resolution_clock::now();

    char* filedata = nullptr;
    DWORD filesize = 0;
    HANDLE hMap = mapFile("C:/Users/SSAFY/Desktop/final_pcb_with_dies_and_defects3.csv", filedata, filesize);
    if (!hMap || !filedata) {
        cout << "파일을 열 수 없습니다." << endl;
        return -1;
    }

    vector<size_t> line_offsets;
    int rows = 0, cols = 0;
    computeOffsets(filedata, filesize, line_offsets, rows, cols);
    cout << "CSV 크기: " << rows << "행 x " << cols << "열" << endl;

    float* flat = new float[rows * cols];
    parseCSV(flat, filedata, line_offsets, rows, cols);
    cout << "CSV 파싱 완료" << endl;

    // 새로운 임계값 변수
    float new_threshold = THRESHOLD;

    // 평탄화 함수 호출 - 새로운 임계값 계산
    flattenPCBSurface(flat, rows, cols, new_threshold);

    uint8_t* binary = new uint8_t[rows * cols];
    binarize(flat, binary, rows, cols, new_threshold);
    delete[] flat;

    cout << "이진화 완료" << endl;

    vector<BlobInfo> blobs = detectBlobs(binary, rows, cols);
    auto end = chrono::high_resolution_clock::now();
    delete[] binary;
    cout << "Blob 감지 완료" << endl;

    outputBlobs(blobs, cols, rows, "C:/Users/SSAFY/Desktop/defect_coordinates_excel.csv");
    cout << "총 실행 시간: " << chrono::duration<double>(end - start).count() << "초" << endl;
    cout << "이물질 개수: " << blobs.size() << endl;
    UnmapViewOfFile(filedata);
    CloseHandle(hMap);
    return 0;
}
