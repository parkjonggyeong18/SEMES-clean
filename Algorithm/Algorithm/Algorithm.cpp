#define _CRT_SECURE_NO_WARNINGS

#include <iostream>  // 입출력
#include <cstdio>    // C 스타일 파일 입출력
#include <vector>
#include <string>
#include <cstring>
#include <sstream>
#include <fstream>
#include <unordered_map>
#include <unordered_set>
#include <atomic>    // 원자 연산
#include <omp.h>     // OpenMP 병렬 처리
#include <chrono>    // 시간 측정
#include <algorithm>
#include <immintrin.h>  // AVX2 SIMD 명령어 사용
#include <malloc.h>  // _mm_malloc 사용

using namespace std;

#if defined(__GNUC__) || defined(__clang__)
#define PREFETCH(addr) __builtin_prefetch(addr, 0, 1)
#else
#define PREFETCH(addr) ((void)0)
#endif

static string toExcelColumn(int num) {
    string col;
    while (num > 0) {
        num--;
        col = char('A' + (num % 26)) + col;
        num /= 26;
    }
    return col;
}

class UnionFind {
public:
    vector<int> parent;
    UnionFind(int size) {
        parent.resize(size);
        for (int i = 0; i < size; i++) parent[i] = i;
    }
    int find(int x) {
        while (parent[x] != x) {
            parent[x] = parent[parent[x]];
            x = parent[x];
        }
        return x;
    }
    void unite(int x, int y) {
        int rx = find(x), ry = find(y);
        if (rx != ry) parent[ry] = rx;
    }
};

int main() {
#ifdef _OPENMP
    cout << "OpenMP 활성화됨. 최대 스레드 수: " << omp_get_max_threads() << endl;
#else
    cerr << "OpenMP가 활성화되지 않았습니다." << endl;
    return -1;
#endif

    auto start = chrono::high_resolution_clock::now();

    FILE* fp = fopen("C:/Users/SSAFY/Desktop/final_pcb_with_dies_and_defects6.csv", "r");
    if (!fp) {
        cerr << "파일 열기 실패" << endl;
        return -1;
    }

    vector<float> data_flat;
    int W = -1;
    char buffer[200000];
    while (fgets(buffer, sizeof(buffer), fp)) {
        int count = 0;
        char* token = strtok(buffer, ",");
        while (token) {
            data_flat.push_back(atof(token));
            token = strtok(nullptr, ",");
            count++;
        }
        if (W == -1) W = count;
    }
    fclose(fp);

    int H = static_cast<int>(data_flat.size() / W);
    if (H == 0 || W == 0) {
        cerr << "CSV 파싱 실패 또는 빈 파일입니다." << endl;
        return -1;
    }

    int* binary = (int*)_mm_malloc(H * W * sizeof(int), 32);
    float threshold = 2.5f;

#pragma omp parallel for
    for (int i = 0; i < H * W; i += 8) {
        __m256 vals = _mm256_loadu_ps(&data_flat[i]);
        __m256 thresh = _mm256_set1_ps(threshold);
        __m256 mask = _mm256_cmp_ps(vals, thresh, _CMP_GE_OQ);
        __m256 ones = _mm256_set1_ps(1.0f);
        __m256 zeros = _mm256_setzero_ps();
        __m256 result = _mm256_blendv_ps(zeros, ones, mask);
        _mm256_store_si256((__m256i*) & binary[i], _mm256_cvtps_epi32(result));
    }

    int* labels = (int*)_mm_malloc(H * W * sizeof(int), 32);
    memset(labels, 0, H * W * sizeof(int));
    int maxLabels = H * W;
    UnionFind uf(maxLabels);
    atomic<int> nextLabel(1);

#pragma omp parallel for schedule(static)
    for (int y = 0; y < H; ++y) {
        for (int x = 0; x < W; ++x) {
            int idx = y * W + x;
            if (binary[idx] == 0) continue;
            int left = (x > 0) ? labels[y * W + x - 1] : 0;
            int up = (y > 0) ? labels[(y - 1) * W + x] : 0;

            if (left == 0 && up == 0) {
                labels[idx] = nextLabel.fetch_add(1);
            }
            else if (left != 0 && up == 0) {
                labels[idx] = left;
            }
            else if (left == 0 && up != 0) {
                labels[idx] = up;
            }
            else {
                labels[idx] = min(left, up);
                uf.unite(left, up);
            }
        }
    }

    vector<atomic<int>> labelMap(maxLabels);
    for (auto& x : labelMap) x.store(0);
    atomic<int> newLabel(1);

#pragma omp parallel for schedule(static)
    for (int i = 0; i < H * W; ++i) {
        if (labels[i] != 0) {
            int root = uf.find(labels[i]);
            int assigned = labelMap[root].load();
            if (assigned == 0) {
                int expected = 0;
                int val = newLabel.fetch_add(1);
                if (!labelMap[root].compare_exchange_strong(expected, val)) {
                    val = labelMap[root].load();
                }
                labels[i] = val;
            }
            else {
                labels[i] = assigned;
            }
        }
    }

    FILE* fout = fopen("C:/Users/SSAFY/Desktop/defect_coordinates_excel.csv", "w");
    fprintf(fout, "Label,ExcelAddress\n");
    for (int y = 0; y < H; y++) {
        for (int x = 0; x < W; x++) {
            int idx = y * W + x;
            if (labels[idx] != 0) {
                string excelAddress = toExcelColumn(x + 1) + to_string(y + 1);
                fprintf(fout, "%d,%s\n", labels[idx], excelAddress.c_str());
            }
        }
    }
    fclose(fout);

    _mm_free(binary);
    _mm_free(labels);

    auto end = chrono::high_resolution_clock::now();
    double elapsed = chrono::duration<double>(end - start).count();
    cout << "\n실행 시간: " << elapsed << "초" << endl;
    cout << "총 라벨 수: " << (newLabel - 1) << endl;
    cout << "불량 좌표 저장 완료: defect_coordinates_excel.csv" << endl;
    return 0;
}