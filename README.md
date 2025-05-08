🔧 주요 기술 및 알고리즘

1. 초고속 파싱

Memory-mapped file (mmap): 대용량 CSV 파일을 파일 I/O 없이 메모리에 직접 매핑 → 수백 MB 이상도 빠르게 처리

줄 오프셋 인덱싱: 개행 문자의 위치만 미리 파악하여 병렬 파싱 가능

2. 병렬 이진화 (Thresholding)

기준 값 threshold = 2.5 이상인 픽셀을 이물질로 판단

SIMD AVX2 명령어 사용: _mm256_cmp_ps, _mm256_movemask_ps

OpenMP와 결합해 이진화 병렬화 → 실시간 처리 지원

3. 이물질 영역 검출 (Blob Detection)

DFS 기반 Connected Component Labeling

4방향(상하좌우) 탐색으로 연결된 이물질 blob 추출

각 blob에 대해 minX, minY ~ maxX, maxY 좌표 계산

4. 병렬 처리 최적화

모든 row 단위 처리에 #pragma omp parallel for 적용

멀티코어(CPU 12코어 기준) 환경에서 1~2초 내 전체 완료 가능

📊 출력 및 시각화

콘솔: [DEFECTS] ~ [END] 구간으로 blob 좌표 출력

각 이물질에 대해:

픽셀 좌표 (minY, minX, maxY, maxX)

mm 환산 좌표 (예: 2.345mm, 3.672mm)

결과 파일(defect_coordinates_excel.csv)에는 Excel 좌표(C130~D150 등) 형식 저장

🧩 확장 가능성

JSON 또는 binary 포맷으로 출력 형태 변경 가능

WPF UI에서 mm 단위 위치를 그래픽으로 시각화 가능

소수점 제한, 필터링, blob 면적 기반 정렬 등 기능 추가 용이

⏱ 성능 요약

전체 CSV 처리 시간: 1~2초 (OpenMP + AVX2 최적화)

실시간 분석 및 자동화 파이프라인 적용 가능

다양한 산업용 PCB 검사, AI 비전 시스템에 연동 가능