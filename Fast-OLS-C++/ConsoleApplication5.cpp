
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
constexpr float PCB_WIDTH_UM = 240000.0f;  // PCB X 방향 길이 (µm)
constexpr float PCB_HEIGHT_UM = 77500.0f;   // PCB Y 방향 높이 (µm)
constexpr float PCB_LENGTH_UM = 250000.0f;  // 스캔 전체 길이 (µm)

// 블롭 정보 구조체
struct BlobInfo {
    int minX = INT_MAX, minY = INT_MAX;
    int maxX = 0, maxY = 0;
};

// 엑셀 열 변환 (1→A, 27→AA)
static string toExcelColumn(int num) {
    string col;
    while (num > 0) {
        num--;
        col = char('A' + (num % 26)) + col;
        num /= 26;
    }
    return col;
}

// mmap 파일 열기
static HANDLE mapFile(const string& path, char*& data, DWORD& size) {
    HANDLE hf = CreateFileA(path.c_str(), GENERIC_READ, FILE_SHARE_READ,
        NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
    if (hf == INVALID_HANDLE_VALUE) return nullptr;
    HANDLE hm = CreateFileMapping(hf, NULL, PAGE_READONLY, 0, 0, NULL);
    if (!hm) { CloseHandle(hf); return nullptr; }
    data = (char*)MapViewOfFile(hm, FILE_MAP_READ, 0, 0, 0);
    size = GetFileSize(hf, NULL);
    return hm;
}

// 줄 오프셋 계산
static void computeOffsets(char* data, DWORD size,
    vector<size_t>& offs, int& H, int& W) {
    offs.push_back(0);
    for (DWORD i = 0; i < size; ++i)
        if (data[i] == '\n') offs.push_back(i + 1);
    H = (int)offs.size() - 1;
    W = 0;
    for (DWORD i = offs[0]; i < size && data[i] != '\n'; ++i)
        if (data[i] == ',') ++W;
    ++W;
}

// CSV 파싱 (병렬)
static void parseCSV(float* flat, char* data,
    const vector<size_t>& offs,
    int H, int W, DWORD size) {
#pragma omp parallel for schedule(dynamic)
    for (int r = 0; r < H; ++r) {
        const char* s = data + offs[r];
        const char* e = data + ((r + 1 < H) ? offs[r + 1] : size);
        int c = 0;
        while (s < e && c < W) {
            float v = 0;
            auto [p, ec] = from_chars(s, e, v);
            if (ec == errc()) {
                flat[r * W + c++] = v;
                s = (*p == ',' || *p == '\n') ? p + 1 : p;
            }
            else {
                flat[r * W + c++] = 0.0f;
                while (s < e && *s != ',' && *s != '\n') ++s;
                ++s;
            }
        }
    }
}

// 이진화 (AVX2 + 테두리 제거)
static void binarize(const float* flat, uint8_t* bin, int H, int W) {
#pragma omp parallel for
    for (int r = 0; r < H; ++r) {
        int c = 0;
        for (; c + 7 < W; c += 8) {
            __m256 vals = _mm256_loadu_ps(&flat[r * W + c]);
            __m256 thr = _mm256_set1_ps(THRESHOLD);
            __m256 cmp = _mm256_cmp_ps(vals, thr, _CMP_GE_OS);
            int mask = _mm256_movemask_ps(cmp);
            for (int i = 0; i < 8; ++i)
                bin[r * W + c + i] = (mask >> i) & 1;
        }
        for (; c < W; ++c)
            bin[r * W + c] = (flat[r * W + c] >= THRESHOLD);
    }
#pragma omp parallel for
    for (int r = 0; r < H; ++r)
        for (int x = 0; x < 240; ++x)
            bin[r * W + x] = bin[r * W + (W - 1 - x)] = 0;
}

// Union-Find 레이블링
class UF {
    vector<int> p, rankv;
public:
    UF() { p.push_back(0); rankv.push_back(0); }
    int add() { int i = p.size(); p.push_back(i); rankv.push_back(0); return i; }
    int find(int x) { return p[x] == x ? x : (p[x] = find(p[x])); }
    void unite(int a, int b) {
        a = find(a); b = find(b);
        if (a == b) return;
        if (rankv[a] < rankv[b]) p[a] = b;
        else if (rankv[a] > rankv[b]) p[b] = a;
        else { p[b] = a; rankv[a]++; }
    }
};

int main() {
    cout << "[PCB 이물질 감지 시작]\n";
    auto t0 = chrono::high_resolution_clock::now();

    char* data = nullptr; DWORD size = 0;
    HANDLE hm = mapFile("C:/Users/SSAFY/Desktop/final_pcb_with_dies_and_defects1.csv", data, size);
    if (!hm || !data) return -1;

    vector<size_t> offs;
    int H = 0, W = 0;
    computeOffsets(data, size, offs, H, W);

    float* flat = new float[H * W];
    uint8_t* bin = new uint8_t[H * W];
    parseCSV(flat, data, offs, H, W, size);
    binarize(flat, bin, H, W);
    delete[] flat;

    // 2-Pass CCL (벡터 대신 고정 배열)
    UF uf;
    vector<int> label(H * W, 0);
    int dr[4] = { -1,-1,0,-1 }, dc[4] = { 0,-1,-1,1 };
    for (int r = 0; r < H; ++r) {
        for (int c = 0; c < W; ++c) {
            int idx = r * W + c;
            if (!bin[idx]) continue;
            int nbrs[4], ncnt = 0;
            for (int k = 0; k < 4; ++k) {
                int nr = r + dr[k], nc = c + dc[k];
                if (nr >= 0 && nc >= 0 && nr < H && nc < W) {
                    int l = label[nr * W + nc];
                    if (l) nbrs[ncnt++] = l;
                }
            }
            if (ncnt == 0) {
                label[idx] = uf.add();
            }
            else {
                int m = nbrs[0];
                for (int i = 1; i < ncnt; ++i) if (nbrs[i] < m) m = nbrs[i];
                label[idx] = m;
                for (int i = 0; i < ncnt; ++i) if (nbrs[i] != m) uf.unite(m, nbrs[i]);
            }
        }
    }

    // 바운딩 박스 계산
    unordered_map<int, int> mp;
    vector<BlobInfo> blobs;
    for (int r = 0; r < H; ++r) for (int c = 0; c < W; ++c) {
        int l = label[r * W + c];
        if (!l) continue;
        int root = uf.find(l);
        auto it = mp.find(root);
        if (it == mp.end()) {
            mp[root] = blobs.size();
            blobs.emplace_back();
        }
        auto& b = blobs[mp[root]];
        b.minX = min(b.minX, c);
        b.minY = min(b.minY, r);
        b.maxX = max(b.maxX, c);
        b.maxY = max(b.maxY, r);
    }

    // 결과 출력
    for (int i = 0; i < blobs.size(); ++i) {
        auto& b = blobs[i];
        string s = toExcelColumn(b.minX + 1) + to_string(b.minY + 1);
        string e = toExcelColumn(b.maxX + 1) + to_string(b.maxY + 1);
        cout << "[Blob " << i + 1 << "] Excel 위치: " << s << "-" << e << "\n";
    }
    cout << "[이물질 위치]\n";
    float umx = PCB_LENGTH_UM / (float)W;
    float umy = PCB_HEIGHT_UM / (float)H;
    for (int i = 0; i < blobs.size(); ++i) {
        auto& b = blobs[i];
        float x1 = b.minX * umx / 1000, y1 = b.minY * umy / 1000;
        float x2 = b.maxX * umx / 1000, y2 = b.maxY * umy / 1000;
        float xm = (x1 + x2) / 2, ym = (y1 + y2) / 2;
        cout << "Blob: " << i + 1 << "\n"
            << fixed << setprecision(3)
            << "시작 끝: " << y1 << "mm " << x1 << "mm ~ "
            << y2 << "mm " << x2 << "mm\n"
            << "이물질 위치: X: " << xm << "mm, Y: " << ym << "mm\n";
    }
    auto t1 = chrono::high_resolution_clock::now();
    cout << "총 실행 시간: " << fixed << setprecision(3)
        << chrono::duration<double>(t1 - t0).count() << "초\n";
    cout << "이물질 개수: " << blobs.size() << "\n";



    UnmapViewOfFile(data);
    CloseHandle(hm);
    delete[] bin;
    return 0;
}
