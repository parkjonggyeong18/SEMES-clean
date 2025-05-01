#include <iostream>
#include <fstream>
#include <sstream>
#include <vector>
#include <unordered_map>
#include <atomic>
#include <omp.h>
#include <chrono>
#include <string> // Added for string operations
#include <map>    // Added for std::map
#include <algorithm> // Added for std::min and std::max

using namespace std;

// Union-Find 자료구조 (경로 압축 포함)
class UnionFind {
public:
    vector<int> parent;
    UnionFind(int size) {
        parent.resize(size);
#pragma omp parallel for // 병렬 초기화 유지
        for (int i = 0; i < size; i++) {
            parent[i] = i;
        }
    }

    // 루트 노드 찾기 (반복문 기반 경로 압축)
    int find(int x) {
        int root = x;
        // 루트 찾기
        while (parent[root] != root) {
            root = parent[root];
        }
        // 경로 압축 (찾는 동안 거쳐간 노드들의 부모를 루트로 직접 연결)
        while (parent[x] != root) {
            int next = parent[x];
            parent[x] = root;
            x = next;
        }
        return root;
    }


    // 두 집합 병합 (더 작은 루트를 부모로 삼도록 개선 가능하나, 여기서는 유지)
    void unionSets(int x, int y) {
        int rootX = find(x);
        int rootY = find(y);
        if (rootX != rootY) {
            // 원자적 연산 또는 critical 섹션 필요 시 고려 (여기서는 각 스레드가 독립적으로 동작 가정)
            parent[rootY] = rootX;
        }
    }
};

// 숫자를 엑셀 열 알파벳으로 변환 (예: 1 -> A, 27 -> AA)
string toExcelColumn(int num) {
    string col;
    if (num <= 0) return ""; // 0 이하 입력 처리
    while (num > 0) {
        int rem = (num - 1) % 26; // 0-based index
        col = char('A' + rem) + col;
        num = (num - 1) / 26;
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

    // 전체 실행 시간 측정 시작
    auto start = chrono::high_resolution_clock::now();

    // CSV 파일 경로 및 열기
    string path = "C:/Users/SSAFY/Desktop/final_pcb_with_dies_and_defects6.csv";
    ifstream file(path);
    if (!file.is_open()) {
        cerr << "CSV 파일 열기 실패: " << path << endl;
        return -1;
    }

    // CSV 파일 파싱
    vector<vector<float>> data;
    string line;
    bool first_line = true;
    int W = 0; // 너비 초기화
    while (getline(file, line)) {
        stringstream ss(line);
        string val;
        vector<float> row;
        while (getline(ss, val, ',')) {
            try {
                row.push_back(stof(val));
            }
            catch (const std::invalid_argument& ia) {
                cerr << "경고: 유효하지 않은 숫자 발견, 0으로 대체: " << val << endl;
                row.push_back(0.0f);
            }
            catch (const std::out_of_range& oor) {
                cerr << "경고: 범위를 벗어난 숫자 발견, 0으로 대체: " << val << endl;
                row.push_back(0.0f);
            }
        }
        if (!row.empty()) {
            if (first_line) {
                W = row.size(); // 첫 줄에서 너비 결정
                first_line = false;
            }
            else if (row.size() != W) {
                cerr << "경고: 행의 길이가 일정하지 않습니다. 이전 행 길이: " << W << ", 현재 행 길이: " << row.size() << endl;
                // 필요 시 에러 처리 또는 패딩 추가
                // 여기서는 일단 진행 (오류 가능성 있음)
            }
            data.push_back(row);
        }
    }
    file.close(); // 파싱 후 파일 닫기

    if (data.empty() || W == 0) {
        cerr << "오류: CSV 파일에서 데이터를 읽지 못했거나 비어 있습니다." << endl;
        return -1;
    }
    int H = data.size();
    cout << "이미지 크기: " << W << " x " << H << endl;


    // 이진화 수행 (threshold: 2.5um)
    vector<vector<int>> binary(H, vector<int>(W, 0));
    float threshold = 2.5;
#pragma omp parallel for collapse(2) // 2중 루프 병렬화
    for (int i = 0; i < H; i++) {
        for (int j = 0; j < W; j++) {
            if (i < data.size() && j < data[i].size()) { // 범위 확인 추가
                binary[i][j] = (data[i][j] >= threshold) ? 1 : 0;
            }
        }
    }

    // 라벨 배열 및 Union-Find 초기화
    vector<vector<int>> labels(H, vector<int>(W, 0));
    // UnionFind 크기를 이미지 크기 정도로 설정하는 것이 안전함
    // 대략적인 추정보다 최대 가능한 라벨 수(H*W)를 사용하는 것이 안전하지만 메모리 고려
    // 또는 동적 크기 조절이 가능한 UnionFind 구현 고려
    UnionFind uf(H * W + 1); // 최대 라벨 수는 H*W일 수 있음. 1부터 시작 고려.
    atomic<int> label_counter(1); // 원자적 라벨 카운터

    // 1차 패스: 위/왼쪽 이웃을 기반으로 라벨링 및 병합 (순차적 실행이 안전)
    // 이 부분은 데이터 종속성 때문에 병렬화가 복잡함. 순차 실행 권장.
    int current_max_label = 0; // 임시 라벨의 최대값 추적
    for (int y = 0; y < H; y++) {
        for (int x = 0; x < W; x++) {
            if (binary[y][x] == 0) continue;

            int left_label = (x > 0 && binary[y][x - 1] == 1) ? labels[y][x - 1] : 0;
            int up_label = (y > 0 && binary[y - 1][x] == 1) ? labels[y - 1][x] : 0;

            if (left_label == 0 && up_label == 0) {
                // 새로운 라벨 할당
                labels[y][x] = label_counter.fetch_add(1, memory_order_relaxed);
                current_max_label = labels[y][x];
            }
            else if (left_label != 0 && up_label == 0) {
                // 왼쪽 라벨 사용
                labels[y][x] = left_label;
            }
            else if (left_label == 0 && up_label != 0) {
                // 위쪽 라벨 사용
                labels[y][x] = up_label;
            }
            else {
                // 두 라벨 모두 존재: 작은 라벨 사용하고, 두 라벨 병합
                labels[y][x] = min(left_label, up_label);
                if (left_label != up_label) {
                    uf.unionSets(left_label, up_label);
                }
            }
        }
    }
    int initial_label_count = label_counter.load() - 1; // 초기 할당된 라벨 수
    cout << "1차 패스 완료. 초기 라벨 수: " << initial_label_count << endl;

    // Union-Find 크기 재조정 (필요 시)
    // if (initial_label_count >= uf.parent.size()) {
    //     uf.parent.resize(initial_label_count + 1); // 크기 조정 로직 필요
    //     // 기존 데이터 유지 및 새 요소 초기화 필요
    // }


    // 2차 패스: 라벨을 루트 라벨로 업데이트 (병렬 처리 가능)
#pragma omp parallel for collapse(2)
    for (int y = 0; y < H; y++) {
        for (int x = 0; x < W; x++) {
            if (labels[y][x] != 0) {
                labels[y][x] = uf.find(labels[y][x]);
            }
        }
    }
    cout << "2차 패스 (루트 라벨 업데이트) 완료." << endl;


    // 루트 라벨을 연속적인 새 라벨로 재매핑
    map<int, int> root_to_new_label_map;
    int final_label_counter = 1;
    // 순차적으로 고유 루트 라벨에 새 ID 부여 (map 사용으로 자동 정렬 가능)
    for (int y = 0; y < H; y++) {
        for (int x = 0; x < W; x++) {
            if (labels[y][x] != 0) {
                int root = labels[y][x];
                if (root_to_new_label_map.find(root) == root_to_new_label_map.end()) {
                    root_to_new_label_map[root] = final_label_counter++;
                }
            }
        }
    }
    int final_label_count = final_label_counter - 1;
    cout << "루트 라벨 재매핑 완료. 최종 라벨 수: " << final_label_count << endl;

    // 최종 라벨 적용 (병렬 처리 가능)
#pragma omp parallel for collapse(2)
    for (int y = 0; y < H; y++) {
        for (int x = 0; x < W; x++) {
            if (labels[y][x] != 0) {
                labels[y][x] = root_to_new_label_map[labels[y][x]];
            }
        }
    }
    cout << "최종 라벨 적용 완료." << endl;


    // --- 각 라벨의 경계(시작/끝 좌표) 찾기 ---
    // map<라벨 ID, pair<pair<minX, minY>, pair<maxX, maxY>>>
    map<int, pair<pair<int, int>, pair<int, int>>> labelBounds;

    for (int y = 0; y < H; ++y) {
        for (int x = 0; x < W; ++x) {
            int currentLabel = labels[y][x];
            if (currentLabel != 0) { // 라벨이 있는 픽셀만 처리
                // 해당 라벨이 맵에 처음 나타나는 경우
                if (labelBounds.find(currentLabel) == labelBounds.end()) {
                    // 현재 좌표(x, y)로 minX, minY, maxX, maxY 초기화
                    labelBounds[currentLabel] = { {x, y}, {x, y} };
                }
                else {
                    // 이미 존재하는 라벨이면 min/max 좌표 업데이트
                    labelBounds[currentLabel].first.first = min(labelBounds[currentLabel].first.first, x);     // minX 업데이트
                    labelBounds[currentLabel].first.second = min(labelBounds[currentLabel].first.second, y);    // minY 업데이트
                    labelBounds[currentLabel].second.first = max(labelBounds[currentLabel].second.first, x);   // maxX 업데이트
                    labelBounds[currentLabel].second.second = max(labelBounds[currentLabel].second.second, y);  // maxY 업데이트
                }
            }
        }
    }
    cout << "각 라벨의 경계 좌표 계산 완료." << endl;


    // --- 결과 좌표 출력 및 저장 (각 라벨의 시작/끝 좌표) ---
    ofstream fout("C:/Users/SSAFY/Desktop/defect_coordinates_excel.csv");
    if (!fout.is_open()) {
        cerr << "오류: 출력 CSV 파일 열기 실패!" << endl;
        // 파일 쓰기 실패 시 처리가 필요할 수 있음
    }
    else {
        // CSV 헤더 작성 (시작 주소, 끝 주소)
        fout << "Label,StartExcelAddress,EndExcelAddress\n";
        cout << "\n[불량 위치 목록 (Excel 좌표 - 시작/끝)]\n";

        // labelBounds 맵을 순회하며 결과 출력 및 저장
        for (const auto& pair : labelBounds) {
            int label = pair.first;
            const auto& bounds = pair.second;
            int minX = bounds.first.first;
            int minY = bounds.first.second;
            int maxX = bounds.second.first;
            int maxY = bounds.second.second;

            // 엑셀 주소 형식으로 변환 (좌표는 0부터 시작하므로 +1 필요)
            string startExcelAddress = toExcelColumn(minX + 1) + to_string(minY + 1);
            string endExcelAddress = toExcelColumn(maxX + 1) + to_string(maxY + 1);

            // 파일에 쓰기
            fout << label << "," << startExcelAddress << "," << endExcelAddress << "\n";
            // 콘솔에 출력
            cout << "Label: " << label << ", 시작: " << startExcelAddress << ", 끝: " << endExcelAddress << endl;
        }
        fout.close(); // 파일 닫기
        cout << "불량 좌표 (시작/끝) 저장 완료: defect_coordinates_excel.csv" << endl;
    }


    // 전체 실행 시간 출력
    auto end = chrono::high_resolution_clock::now();
    chrono::duration<double> elapsed = end - start;
    cout << "\n총 실행 시간: " << elapsed.count() << "초" << endl;
    cout << "최종 라벨 수 (불량 영역 수): " << final_label_count << endl;


    return 0;
}