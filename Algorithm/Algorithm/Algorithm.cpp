#define _CRT_SECURE_NO_WARNINGS
#include <iostream>
#include <fstream>
#include <vector>
#include <string>
#include <unordered_map>
#include <atomic>
#include <chrono>
#include <omp.h>
#include <immintrin.h>
#include <windows.h>
#include <functional>
#include <climits>
#include <iomanip>
#include <charconv>  // ✅ from_chars 사용
using namespace std;

// ✅ Excel 주소 변환 함수 (1 → A, 27 → AA 등)
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

	// ✅ 파일 mmap 열기
	string path = "C:/Users/SSAFY/Desktop/final_pcb_with_dies_and_defects3.csv";
	HANDLE hFile = CreateFileA(path.c_str(), GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
	if (hFile == INVALID_HANDLE_VALUE) return -1;
	HANDLE hMap = CreateFileMapping(hFile, NULL, PAGE_READONLY, 0, 0, NULL);
	if (!hMap) return -1;
	char* filedata = (char*)MapViewOfFile(hMap, FILE_MAP_READ, 0, 0, 0);
	DWORD filesize = GetFileSize(hFile, NULL);

	// ✅ 줄 단위 시작 오프셋 수집
	vector<size_t> line_offsets;
	line_offsets.push_back(0);
	for (DWORD i = 0; i < filesize; i++) {
		if (filedata[i] == '\n') line_offsets.push_back(i + 1);
	}
	int rows = line_offsets.size() - 1;

	// ✅ 열 개수 계산 (첫 줄의 ',' 개수 + 1)
	int cols = 0;
	for (DWORD i = line_offsets[0]; i < filesize && filedata[i] != '\n'; i++) {
		if (filedata[i] == ',') cols++;
	}
	cols++;

	// ✅ flat 배열 (row x col float 데이터)
	float* flat = new float[rows * cols];

	// ✅ 병렬 파싱 (from_chars 사용, locale 영향 없음)
#pragma omp parallel for schedule(dynamic)
	for (int y = 0; y < rows; y++) {
		char* start = filedata + line_offsets[y];
		char* end = filedata + (y + 1 < line_offsets.size() ? line_offsets[y + 1] : filesize);
		int x = 0;
		while (start < end && x < cols) {
			float val = 0;
			auto [ptr, ec] = std::from_chars(start, end, val);
			if (ec == std::errc()) {
				flat[y * cols + x++] = val;
				start = (*ptr == ',' || *ptr == '\n') ? const_cast<char*>(ptr + 1) : const_cast<char*>(ptr);

			}
			else {
				flat[y * cols + x++] = 0.0f;  // 오류 시 0.0으로 대체
				while (start < end && *start != ',' && *start != '\n') ++start;
				start++;
			}
		}
	}

	UnmapViewOfFile(filedata);
	CloseHandle(hMap);
	CloseHandle(hFile);
	
	// ✅ AVX2 이진화 처리
	float threshold = 2.5;
	uint8_t* binary = new uint8_t[rows * cols];
#pragma omp parallel for 
	for (int y = 0; y < rows; y++) {
		int x = 0;
		for (; x + 7 < cols; x += 8) {
			__m256 vals = _mm256_loadu_ps(&flat[y * cols + x]);
			__m256 thresh = _mm256_set1_ps(threshold);
			__m256 result = _mm256_cmp_ps(vals, thresh, _CMP_GE_OS);
			int mask = _mm256_movemask_ps(result);
			for (int i = 0; i < 8; i++) binary[y * cols + x + i] = (mask >> i) & 1;
		}
		for (; x < cols; x++) {
			binary[y * cols + x] = flat[y * cols + x] >= threshold;
		}
	}
	delete[] flat;

	// ✅ DFS 기반 blob 추출
	struct BlobInfo {
		int minX = INT_MAX, minY = INT_MAX, maxX = 0, maxY = 0;
	};
	vector<BlobInfo> blobs;
	uint8_t* visited = new uint8_t[rows * cols]();
	int dx[] = { 1, -1, 0, 0 }, dy[] = { 0, 0, 1, -1 };

	struct Coord { int y, x; };
	for (int y = 0; y < rows; y++) {
		for (int x = 0; x < cols; x++) {
			if (binary[y * cols + x] && !visited[y * cols + x]) {
				blobs.emplace_back();
				vector<Coord> stack;
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
	delete[] binary;
	delete[] visited;

	// ✅ 엑셀 주소로 결과 출력
	ofstream fout("C:/Users/SSAFY/Desktop/defect_coordinates_excel.csv");
	fout << "Blob,ExcelRange\n";
	int idx = 1;
	for (auto& b : blobs) {
		string start = toExcelColumn(b.minX + 1) + to_string(b.minY + 1);
		string end = toExcelColumn(b.maxX + 1) + to_string(b.maxY + 1);
		fout << idx++ << "," << start << "~" << end << "\n";
		cout << "Blob: " << idx - 1 << ", 위치: " << start << "~" << end << endl;
	}
	fout.close();

	// ✅ µm 단위 이물질 중심 출력
	float um_per_pixel_x = 240000.0f / cols;
	float um_per_pixel_y = 77500.0f / rows;

	cout << "[이물질 위치]" << endl;
	idx = 1;
	for (auto& b : blobs) {
		int minY = b.minY, minX = b.minX, maxY = b.maxY, maxX = b.maxX;

		float x1_mm = (minX * um_per_pixel_x) / 1000.0f;
		float y1_mm = (minY * um_per_pixel_y) / 1000.0f;
		float x2_mm = (maxX * um_per_pixel_x) / 1000.0f;
		float y2_mm = (maxY * um_per_pixel_y) / 1000.0f;

		float avg_x = (x1_mm + x2_mm) / 2.0f;
		float avg_y = (y1_mm + y2_mm) / 2.0f;

		cout << fixed << setprecision(3);
		cout << "Blob: " << idx << endl;
		cout << y1_mm << "mm," << x1_mm << "mm," << y2_mm << "mm," << x2_mm << "mm" << endl;
		cout << "이물질 위치: " << "X: " << avg_x << "mm, "<<"Y: " << avg_y << "mm" << endl;
		idx++;
	}

	auto end = chrono::high_resolution_clock::now();
	cout << "총 실행 시간: " << chrono::duration<double>(end - start).count() << "초" << endl;
	cout << "이물질 개수: " << blobs.size() << endl;
	cout << "엑셀 파일 경로: C:/Users/SSAFY/Desktop/defect_coordinates_excel.csv" << endl;
	return 0;
}
