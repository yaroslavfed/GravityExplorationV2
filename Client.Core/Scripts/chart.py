import sys
import json
import matplotlib.pyplot as plt

json_file = sys.argv[1]
output_image = sys.argv[2]

with open(json_file, "r") as f:
    data = json.load(f)

domain = data["domain"]
strata = data["strata"]

fig, ax = plt.subplots(figsize=(8, 6))

# Оси и сетка
ax.axhline(y=0, color='black', linewidth=1)
ax.axvline(x=0, color='black', linewidth=1)

ax.set_xticks([domain["StartX"] + i * (domain["EndX"] - domain["StartX"]) / domain["SplitsXCount"] for i in
               range(domain["SplitsXCount"] + 1)])
ax.set_yticks([domain["StartY"] + i * (domain["EndY"] - domain["StartY"]) / domain["SplitsYCount"] for i in
               range(domain["SplitsYCount"] + 1)])
ax.grid(True, linestyle="--", linewidth=0.5, alpha=0.7)

# Границы области
rect = plt.Rectangle((domain["StartX"], domain["StartY"]),
                     domain["EndX"] - domain["StartX"],
                     domain["EndY"] - domain["StartY"],
                     linewidth=2, edgecolor='r', facecolor='none')
ax.add_patch(rect)

print("Domain:", domain)

# Отрисовка объектов
for stratum in strata:
    x_min = stratum["StartX"]
    y_min = stratum["StartY"]
    x_max = stratum["EndX"]
    y_max = stratum["EndY"]
    is_active = stratum["IsActive"]

    alpha = 1.0 if is_active else 0.5

    # Обрезаем прямоугольник по границам области
    x_min = max(x_min, domain["StartX"])
    y_min = max(y_min, domain["StartY"])
    x_max = min(x_max, domain["EndX"])
    y_max = min(y_max, domain["EndY"])

    width = x_max - x_min
    height = y_max - y_min

    print(
        f"Stratum: Start=({x_min},{y_min}), End=({x_max},{y_max}), Width={width}, Height={height}, Active={is_active}")

    # Рисуем обрезанный прямоугольник
    obj_rect = plt.Rectangle((x_min, y_min), width, height, linewidth=1.5, edgecolor='blue', facecolor='lightblue',
                             alpha=alpha)
    ax.add_patch(obj_rect)

    ax.text((x_min + x_max) / 2, (y_min + y_max) / 2, f"{stratum['Density']:.1f}", fontsize=8, ha='center', va='center', color='black')

ax.set_xlim(domain["StartX"], domain["EndX"])
ax.set_ylim(domain["StartY"], domain["EndY"])
ax.set_xlabel("X")
ax.set_ylabel("Y")

plt.savefig(output_image, dpi=300, bbox_inches='tight')
print(f"Сохранено изображение: {output_image}")
