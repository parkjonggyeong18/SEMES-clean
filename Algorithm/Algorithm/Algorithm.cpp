#define _CRT_SECURE_NO_WARNINGS
#include <iostream>
#include <fstream>
#include <vector>
#include <string>
#include <unordered_map>
#include <atomic>
#include <chrono>
#include <omp.h>              // OpenMP 병렬 처리
#include <immintrin.h>        // SIMD (AVX 명령어)
#include <windows.h>          // Windows mmap 파일 매핑
#include <charconv>           // 고속 문자열 숫자 변환 from_chars

using namespace std;

// ✅ Union-Find 자료구조 (경로 압축 + rank 병합)
class UnionFind {
public:
    vector<int> parent, rank;
    UnionFind(int size) : parent(size), rank(size, 0) {
#pragma omp parallel for
        for (int i = 0; i < size; i++) {
            parent[i] = i;
        }
    }

    int find(int x) {
        while (parent[x] != x) {
            parent[x] = parent[parent[x]];
            x = parent[x];
        }
        return x;
    }

    void unionSets(int x, int y) {
        int rootX = find(x), rootY = find(y);
        if (rootX == rootY) return;
        if (rank[rootX] < rank[rootY]) {
            parent[rootX] = rootY;
        }
        else {
            parent[rootY] = rootX;
            if (rank[rootX] == rank[rootY]) rank[rootX]++;
        }
    }
};

// ✅ 숫자를 Excel 열 주소로 변환 (예: 1 -> A, 28 -> AB)
string toExcelColumn(int num) {
    string col;
    while (num > 0) {
        num--;
        col = char('A' + (num % 26)) + col;
        num /= 26;
    }
    return col;
}

int main() {
#ifdef _OPENMP
    cout << "OpenMP 활성화됨. 최대 스레드 수: " << omp_get_max_threads() << endl;
#else
    cerr << "OpenMP가 활성화되지 않았습니다." << endl;
    return -1;
#endif


    auto start = chrono::high_resolution_clock::now();

    // ✅ 파일 열기 및 mmap 설정
    string path = "C:/Users/SSAFY/Desktop/final_pcb_with_dies_and_defects3.csv";
    HANDLE hFile = CreateFileA(path.c_str(), GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
    if (hFile == INVALID_HANDLE_VALUE) return -1;

    HANDLE hMapping = CreateFileMapping(hFile, NULL, PAGE_READONLY, 0, 0, NULL);
    if (!hMapping) return -1;

    char* filedata = (char*)MapViewOfFile(hMapping, FILE_MAP_READ, 0, 0, 0);
    if (!filedata) return -1;

    DWORD filesize = GetFileSize(hFile, NULL);
    if (filesize == INVALID_FILE_SIZE) return -1;

    // ✅ from_chars 기반 직접 파싱 (빈 셀/지수 대응)
    vector<vector<float>> data;
    data.emplace_back();
    char* cur = filedata;
    char* end_time = filedata + filesize;

    while (cur < end_time) {
        if (*cur == ',' || *cur == '\n') {
            if (cur != filedata) {
                float val;
                auto [ptr, ec] = from_chars(filedata, cur, val);
                if (ec == errc()) {
                    data.back().push_back(val);
                }
            }
            filedata = cur + 1;
            if (*cur == '\n') data.emplace_back();
        }
        cur++;
    }
    if (data.back().empty()) data.pop_back();
    UnmapViewOfFile(filedata);
    CloseHandle(hMapping);
    CloseHandle(hFile);

    // ✅ 이진화 (SIMD AVX2 병렬처리 포함)
    int H = data.size(), W = data[0].size();
    float threshold = 2.5;
    vector<vector<int>> binary(H, vector<int>(W));
#pragma omp parallel for
    for (int y = 0; y < H; y++) {
        int x = 0;
        for (; x + 7 < W; x += 8) {
            __m256 vals = _mm256_loadu_ps(&data[y][x]);
            __m256 thresh = _mm256_set1_ps(threshold);
            __m256 result = _mm256_cmp_ps(vals, thresh, _CMP_GE_OS);
            int mask = _mm256_movemask_ps(result);
            for (int i = 0; i < 8; i++) {
                binary[y][x + i] = (mask >> i) & 1;
            }
        }
        for (; x < W; x++) {
            binary[y][x] = data[y][x] >= threshold ? 1 : 0;
        }
    }

    // ✅ 라벨링 1차 패스 (순차 Union-Find 병합)
    vector<vector<int>> labels(H, vector<int>(W));
    UnionFind uf(H * W);
    atomic<int> label(1);
    for (int y = 0; y < H; y++) {
        for (int x = 0; x < W; x++) {
            if (!binary[y][x]) continue;
            int left = (x > 0) ? labels[y][x - 1] : 0;
            int up = (y > 0) ? labels[y - 1][x] : 0;
            if (left == 0 && up == 0) {
                labels[y][x] = label++;
            }
            else if (left != 0 && up == 0) {
                labels[y][x] = left;
            }
            else if (left == 0 && up != 0) {
                labels[y][x] = up;
            }
            else {
                labels[y][x] = min(left, up);
                uf.unionSets(left, up);
            }
        }
    }

    // ✅ 라벨 정규화 (2차 패스)
    vector<int> root_to_new(label);
    atomic<int> newLabel(1);
#pragma omp parallel for collapse(2)
    for (int y = 0; y < H; y++) {
        for (int x = 0; x < W; x++) {
            if (labels[y][x] != 0) {
                int root = uf.find(labels[y][x]);
                labels[y][x] = root;
                if (root_to_new[root] == 0) {
#pragma omp critical
                    {
                        if (root_to_new[root] == 0) {
                            root_to_new[root] = newLabel++;
                        }
                    }
                }
            }
        }
    }

    // ✅ 최종 라벨로 재할당
#pragma omp parallel for collapse(2)
    for (int y = 0; y < H; y++) {
        for (int x = 0; x < W; x++) {
            if (labels[y][x] != 0) {
                labels[y][x] = root_to_new[labels[y][x]];
            }
        }
    }

    // ✅ 불량 좌표 기록 (스레드 분리 + 병합 방식)
    vector<unordered_map<int, pair<string, string>>> localMaps(omp_get_max_threads());
#pragma omp parallel for
    for (int y = 0; y < H; y++) {
        int tid = omp_get_thread_num();
        for (int x = 0; x < W; x++) {
            int lbl = labels[y][x];
            if (lbl != 0) {
                string addr = toExcelColumn(x + 1) + to_string(y + 1);
                auto& m = localMaps[tid];
                if (m.find(lbl) == m.end()) m[lbl] = { addr, addr };
                else m[lbl].second = addr;
            }
        }
    }

    unordered_map<int, pair<string, string>> labelRange;
    for (auto& m : localMaps) {
        for (auto& [lbl, range] : m) {
            if (labelRange.find(lbl) == labelRange.end()) labelRange[lbl] = range;
            else labelRange[lbl].second = range.second;
        }
    }

    // ✅ 결과 저장
    ofstream fout("C:/Users/SSAFY/Desktop/defect_coordinates_excel.csv");
    fout << "Label,ExcelAddress\n";
    for (auto& [lbl, range] : labelRange) {
        fout << lbl << "," << range.first << "~" << range.second << "\n";
		cout << "Label: " << lbl << ", 이물질 위치: " << range.first << "~" << range.second << endl;
    }
    fout.close();

    // ✅ 성능 출력
    auto end = chrono::high_resolution_clock::now();
    double elapsed = chrono::duration<double>(end - start).count();
    cout << "\n실행 시간: " << elapsed << "초" << endl;
    cout << "총 라벨 수: " << (newLabel - 1) << endl;
    cout << "불량 좌표 저장 완료: defect_coordinates_excel.csv" << endl;
    return 0;
}
