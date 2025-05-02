// ✅ 초고속 PCB 이물질 감지 코드 (1~2초 목표)

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
#include <charconv>
#include <functional>
using namespace std;

// ✅ Excel 주소 변환 함수
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


	// ✅ 파일 mmap
	string path = "C:/Users/SSAFY/Desktop/final_pcb_with_dies_and_defects6.csv";
	HANDLE hFile = CreateFileA(path.c_str(), GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
	if (hFile == INVALID_HANDLE_VALUE) return -1;
	HANDLE hMap = CreateFileMapping(hFile, NULL, PAGE_READONLY, 0, 0, NULL);
	if (!hMap) return -1;
	char* filedata = (char*)MapViewOfFile(hMap, FILE_MAP_READ, 0, 0, 0);
	DWORD filesize = GetFileSize(hFile, NULL);

	// ✅ 1차: 줄 수 세기
	int rows = 0, cols = 0;
	for (DWORD i = 0; i < filesize; i++) {
		if (filedata[i] == '\n') rows++;
		if (rows == 0 && filedata[i] == ',') cols++;
	}
	cols++;  // 마지막 열 포함
	float* flat = new float[rows * cols];

	// ✅ 병렬 파싱 (줄 단위 분할)
	vector<size_t> line_offsets;
	line_offsets.push_back(0);
	for (DWORD i = 0; i < filesize; i++) {
		if (filedata[i] == '\n') line_offsets.push_back(i + 1);
	}

#pragma omp parallel for schedule(dynamic)
	for (int y = 0; y < rows; y++) {
		char* start = filedata + line_offsets[y];
		char* end = filedata + (y + 1 < line_offsets.size() ? line_offsets[y + 1] : filesize);
		int x = 0;
		while (start < end && x < cols) {
			char* next = start;
			while (next < end && *next != ',' && *next != '\n') next++;
			float val;
			from_chars(start, next, val);
			flat[y * cols + x++] = val;
			start = (*next == ',' || *next == '\n') ? next + 1 : next;
		}
	}
	UnmapViewOfFile(filedata);
	CloseHandle(hMap);
	CloseHandle(hFile);

	// ✅ AVX2 이진화
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

	// ✅ 이물질 blob 좌표만 추출 (dfs)
	struct BlobInfo {
		int minX = INT_MAX, minY = INT_MAX, maxX = 0, maxY = 0;
	};
	vector<BlobInfo> blobs;
	vector<vector<bool>> visited(rows, vector<bool>(cols, false));
	int dx[] = { 1, -1, 0, 0 }, dy[] = { 0, 0, 1, -1 };

	function<void(int, int)> dfs = [&](int y, int x) {
		visited[y][x] = true;
		blobs.back().minX = min(blobs.back().minX, x);
		blobs.back().minY = min(blobs.back().minY, y);
		blobs.back().maxX = max(blobs.back().maxX, x);
		blobs.back().maxY = max(blobs.back().maxY, y);
		for (int d = 0; d < 4; d++) {
			int ny = y + dy[d], nx = x + dx[d];
			if (ny >= 0 && ny < rows && nx >= 0 && nx < cols && !visited[ny][nx] && binary[ny * cols + nx]) {
				dfs(ny, nx);
			}
		}
		};
	for (int y = 0; y < rows; y++) {
		for (int x = 0; x < cols; x++) {
			if (binary[y * cols + x] && !visited[y][x]) {
				blobs.emplace_back();
				dfs(y, x);
			}
		}
	}
	delete[] binary;

	// ✅ 결과 출력
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

	auto end = chrono::high_resolution_clock::now();
	cout << "총 실행 시간: " << chrono::duration<double>(end - start).count() << "초" << endl;
	cout << "이물질 개수: " << blobs.size() << endl;
	cout << "엑셀 파일 경로: C:/Users/SSAFY/Desktop/defect_coordinates_excel.csv" << endl;
	return 0;
}
