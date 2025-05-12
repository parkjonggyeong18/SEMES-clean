import numpy as np
import cv2
import pandas as pd

# ==== 기본 설정 ====
pcb_x_um = 250000  # PCB X 길이 (250mm)
pcb_y_um = 77500   # PCB Y 길이 (77.5mm)
cols, rows = 12000, 4000
um_per_pixel_x = pcb_x_um / cols
um_per_pixel_y = pcb_y_um / rows

# Die 설정
die_x_um = 10000
die_y_um = 11200
gap_die_x_um = 12000
gap_die_y_um = 13200
num_die_x = 19
num_die_y = 5

# ==== 평탄하지 않은 ZMap 생성 (기울기 + 곡면 + 노이즈) ====
x = np.linspace(0, 1, cols)
y = np.linspace(0, 1, rows)
xx, yy = np.meshgrid(x, y)

# 기울기 성분
a, b = 1.0, -0.5
slope = a * xx + b * yy

# 곡면 성분
curve = 0.2 * ((xx - 0.5) ** 2 + (yy - 0.5) ** 2)

# 노이즈
noise = np.random.normal(0, 0.01, (rows, cols))

# 최종 ZMap
zmap = (2.1 + slope + curve + noise).astype(np.float32)

# 픽셀 단위 계산
die_w_px = int(die_x_um / um_per_pixel_x)
die_h_px = int(die_y_um / um_per_pixel_y)
gap_x_px = int(gap_die_x_um / um_per_pixel_x)
gap_y_px = int(gap_die_y_um / um_per_pixel_y)

# Die 마스킹 및 제거
total_w_um = (num_die_x - 1) * gap_die_x_um
total_h_um = (num_die_y - 1) * gap_die_y_um
start_x_px = int(((pcb_x_um - total_w_um) / 2) / um_per_pixel_x)
start_y_px = int(((pcb_y_um - total_h_um) / 2) / um_per_pixel_y)

mask_die = np.zeros((rows, cols), dtype=np.uint8)
for iy in range(num_die_y):
    for ix in range(num_die_x):
        cx = start_x_px + ix * gap_x_px
        cy = start_y_px + iy * gap_y_px
        tlx = cx - die_w_px // 2
        tly = cy - die_h_px // 2
        brx = tlx + die_w_px
        bry = tly + die_h_px
        if tlx < 0 or tly < 0 or brx > cols or bry > rows:
            continue
        zmap[tly:bry, tlx:brx] = 0
        mask_die[tly:bry, tlx:brx] = 1

# ==== 불량 생성 함수 (높이 보장) ====
def add_rectangular_defects(zmap, mask_die, num_defects=20,
                            min_size_px=10, max_size_px=1000,
                            min_height_um=0.6, max_height_um=1.0):
    rows, cols = zmap.shape
    defects = []
    tries = 0
    while len(defects) < num_defects and tries < 20000:
        tries += 1
        w = np.random.randint(min_size_px, max_size_px + 1)
        h = np.random.randint(min_size_px, max_size_px + 1)
        cx = np.random.randint(w // 2, cols - w // 2)
        cy = np.random.randint(h // 2, rows - h // 2)
        tlx, tly = cx - w // 2, cy - h // 2
        brx, bry = tlx + w, tly + h
        if tlx < 0 or tly < 0 or brx > cols or bry > rows:
            continue
        if np.any(mask_die[tly:bry, tlx:brx] != 0):
            continue
        base_max = np.max(zmap[tly:bry, tlx:brx])
        rnd_h = base_max + np.random.uniform(min_height_um, max_height_um)
        zmap[tly:bry, tlx:brx] = rnd_h
        defects.append((tlx, tly, brx, bry))
    return defects

# ==== 불량 생성 ====
defect_boxes = add_rectangular_defects(zmap, mask_die, num_defects=1000)

# ==== TIF 저장 ====
zmap_min = np.min(zmap)
zmap_max = np.max(zmap)
zmap_normalized = ((zmap - zmap_min) / (zmap_max - zmap_min) * 255.0).astype(np.uint8)
tif_path = "C:\\Users\\SSAFY\\Desktop\\final_pcb_with_dies_and_defects_skewed.tif"
cv2.imwrite(tif_path, zmap_normalized)

# ==== CSV 저장 ====
csv_path = "C:\\Users\\SSAFY\\Desktop\\final_pcb_with_dies_and_defects15.csv"
with open(csv_path, "w") as f:
    for row in zmap:
        line = ",".join(map(str, row))
        f.write(line + "\n")

print(f"TIF 저장 완료: {tif_path}")
print(f"CSV 저장 완료: {csv_path}")
