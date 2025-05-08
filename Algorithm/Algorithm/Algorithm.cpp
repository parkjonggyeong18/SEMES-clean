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
using namespace std;

// === 상수 정의 ===
constexpr float THRESHOLD = 2.5f;
constexpr float PCB_WIDTH_UM = 240000.0f;
constexpr float PCB_HEIGHT_UM = 77500.0f;
constexpr float PCB_LENGTH_UM = 250000.0f; // 전체 스캔 길이 (여유 포함)

struct BlobInfo {
    int minX = INT_MAX, minY = INT_MAX, maxX = 0, maxY = 0;
};

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

static void outputBlobs(const vector<BlobInfo>& blobs, int cols,int rows, const string& path) {
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
    if (!hMap || !filedata) return -1;

    vector<size_t> line_offsets;
    int rows = 0, cols = 0;
    computeOffsets(filedata, filesize, line_offsets, rows, cols);

    float* flat = new float[rows * cols];
    parseCSV(flat, filedata, line_offsets, rows, cols);

    uint8_t* binary = new uint8_t[rows * cols];
    binarize(flat, binary, rows, cols);
    delete[] flat;

    vector<BlobInfo> blobs = detectBlobs(binary, rows, cols);
    auto end = chrono::high_resolution_clock::now();
    delete[] binary;

    outputBlobs(blobs, cols,rows, "C:/Users/SSAFY/Desktop/defect_coordinates_excel.csv");
    cout << "총 실행 시간: " << chrono::duration<double>(end - start).count() << "초" << endl;
    cout << "이물질 개수: " << blobs.size() << endl;
    UnmapViewOfFile(filedata);
    CloseHandle(hMap);
    return 0;
}
