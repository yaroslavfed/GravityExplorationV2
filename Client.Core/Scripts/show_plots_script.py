import argparse
import json
from dataclasses import dataclass
from typing import List, Optional

import matplotlib.pyplot as plt
import numpy as np
from matplotlib.cm import ScalarMappable
from matplotlib.patches import Rectangle
from mpl_toolkits.mplot3d.art3d import Poly3DCollection


@dataclass(frozen=True)
class Point3D:
    X: float
    Y: float
    Z: float

@dataclass(frozen=True)
class Sensor:
    Position: Point3D
    ComponentDirection: str

@dataclass
class Cell:
    CenterX: float
    CenterY: float
    CenterZ: float
    BoundX: float
    BoundY: float
    BoundZ: float
    Density: float
    SubdivisionLevel: float

def load_from_json(file_path: str) -> tuple[List[Cell], List[Sensor]]:
    """Загрузка данных из JSON файла с новой структурой"""
    try:
        with open(file_path, 'r') as f:
            data = json.load(f)
    except FileNotFoundError:
        raise ValueError(f"Файл {file_path} не найден")
    except json.JSONDecodeError:
        raise ValueError(f"Ошибка парсинга JSON в файле {file_path}")

    # Загрузка ячеек
    cells = []
    for cell_data in data['Cells']:
        cell = Cell(
            CenterX=cell_data['CenterX'],
            CenterY=cell_data['CenterY'],
            CenterZ=cell_data['CenterZ'],
            BoundX=cell_data['BoundX'],
            BoundY=cell_data['BoundY'],
            BoundZ=cell_data['BoundZ'],
            Density=cell_data['Density'],
            SubdivisionLevel=cell_data['SubdivisionLevel']
        )
        cells.append(cell)

    # Загрузка сенсоров
    sensors = []
    for sensor_data in data.get('sensors', []):
        pos_data = sensor_data['Position']
        sensors.append(Sensor(
            Position=Point3D(
                X=pos_data['X'],
                Y=pos_data['Y'],
                Z=pos_data['Z']
            ),
            ComponentDirection=sensor_data['ComponentDirection']
        ))

    if not cells:
        raise ValueError("Файл не содержит ячеек для визуализации")

    return cells, sensors

def get_cell_corners(cell: Cell) -> np.ndarray:
    """Возвращает координаты углов ячейки в виде массива 8x3"""
    x_min = cell.CenterX - cell.BoundX
    x_max = cell.CenterX + cell.BoundX
    y_min = cell.CenterY - cell.BoundY
    y_max = cell.CenterY + cell.BoundY
    z_min = cell.CenterZ - cell.BoundZ
    z_max = cell.CenterZ + cell.BoundZ

    return np.array([
        [x_min, y_min, z_min],  # Левый нижний задний угол
        [x_max, y_min, z_min],  # Правый нижний задний
        [x_max, y_max, z_min],  # Правый верхний задний
        [x_min, y_max, z_min],  # Левый верхний задний
        [x_min, y_min, z_max],  # Левый нижний передний
        [x_max, y_min, z_max],  # Правый нижний передний
        [x_max, y_max, z_max],  # Правый верхний передний
        [x_min, y_max, z_max]   # Левый верхний передний
    ])

def plot_cell_mesh(
        cells: List[Cell],
        sensors: List[Sensor],
        x_slice: Optional[float] = None,
        y_slice: Optional[float] = None,
        z_slice: Optional[float] = None
):
    """Визуализация ячеек с поддержкой сечений"""
    fig = plt.figure(figsize=(18, 12))
    gs = fig.add_gridspec(2, 2,
                          left=0.05, right=0.88,
                          top=0.95, bottom=0.05,
                          wspace=0.3, hspace=0.3)

    ax3d = fig.add_subplot(gs[0, 0], projection='3d')
    ax_top_right = fig.add_subplot(gs[0, 1])
    ax_bottom_left = fig.add_subplot(gs[1, 0])
    ax_bottom_right = fig.add_subplot(gs[1, 1])

    # Настройка цветовой карты для Density
    densities = [cell.Density for cell in cells]
    norm = plt.Normalize(min(densities), max(densities))
    cmap = plt.get_cmap('RdYlGn_r')
    mappable = ScalarMappable(norm=norm, cmap=cmap)

    # Расчет границ области визуализации
    all_corners = np.vstack([get_cell_corners(cell) for cell in cells])
    min_vals = all_corners.min(axis=0)
    max_vals = all_corners.max(axis=0)
    padding = 0.1
    bounds = {
        'x': (min_vals[0]-padding, max_vals[0]+padding),
        'y': (min_vals[1]-padding, max_vals[1]+padding),
        'z': (min_vals[2]-padding, max_vals[2]+padding)
    }

    # 3D визуализация ячеек
    if sensors:
        sensor_coords = np.array([[s.Position.X, s.Position.Y, s.Position.Z] for s in sensors])
        ax3d.scatter(sensor_coords[:,0], sensor_coords[:,1], sensor_coords[:,2],
                     c='red', s=50, alpha=0.5, label='Sensors')

    for cell in cells:
        color = cmap(norm(cell.Density))
        corners = get_cell_corners(cell)

        # Рисуем прозрачные грани
        faces = [
            [corners[0], corners[1], corners[2], corners[3]],  # Задняя
            [corners[4], corners[5], corners[6], corners[7]],  # Передняя
            [corners[0], corners[1], corners[5], corners[4]],  # Нижняя
            [corners[2], corners[3], corners[7], corners[6]],  # Верхняя
            [corners[0], corners[3], corners[7], corners[4]],  # Левая
            [corners[1], corners[2], corners[6], corners[5]]   # Правая
        ]

        ax3d.add_collection3d(Poly3DCollection(
            faces, facecolors=color, edgecolors='k', linewidths=0.3, alpha=0.3))

    ax3d.set(xlim=bounds['x'], ylim=bounds['y'], zlim=bounds['z'])
    ax3d.set_xlabel('X', fontsize=12, labelpad=15)
    ax3d.set_ylabel('Y', fontsize=12, labelpad=15)
    ax3d.set_zlabel('Z', fontsize=12, labelpad=15)
    ax3d.set_title('3D View', pad=20)

    # Функции для 2D проекций
    def draw_projection(ax, plane: str):
        ax.cla()
        ax.set_title(f"{plane.upper()} Projection", fontsize=10)
        ax.grid(True, linestyle='--', alpha=0.3)

        for cell in cells:
            color = cmap(norm(cell.Density))

            # Определение центра и размеров для проекции
            if plane == 'xy':
                x, y = cell.CenterX, cell.CenterY
                width, height = 2*cell.BoundX, 2*cell.BoundY
            elif plane == 'xz':
                x, y = cell.CenterX, cell.CenterZ
                width, height = 2*cell.BoundX, 2*cell.BoundZ
            else:  # yz
                x, y = cell.CenterY, cell.CenterZ
                width, height = 2*cell.BoundY, 2*cell.BoundZ

            rect = Rectangle(
                (x - width/2, y - height/2),
                width, height,
                facecolor=color, edgecolor='k', alpha=0.7, linewidth=0.5
            )
            ax.add_patch(rect)

        # Установка границ
        if plane == 'xy':
            ax.set_xlim(bounds['x'])
            ax.set_ylim(bounds['y'])
            ax.set_xlabel('X')
            ax.set_ylabel('Y')
        elif plane == 'xz':
            ax.set_xlim(bounds['x'])
            ax.set_ylim(bounds['z'])
            ax.set_xlabel('X')
            ax.set_ylabel('Z')
        else:
            ax.set_xlim(bounds['y'])
            ax.set_ylim(bounds['z'])
            ax.set_xlabel('Y')
            ax.set_ylabel('Z')

        ax.set_aspect('equal')

    # Отрисовка проекций
    draw_projection(ax_top_right, 'xy')
    draw_projection(ax_bottom_left, 'xz')
    draw_projection(ax_bottom_right, 'yz')

    # Цветовая шкала
    cbar_ax = fig.add_axes([0.90, 0.15, 0.02, 0.7])
    cbar = fig.colorbar(mappable, cax=cbar_ax, label='Density')
    cbar.ax.tick_params(labelsize=10)

    plt.savefig("graph.png", dpi=300, bbox_inches='tight')
    plt.show()

if __name__ == "__main__":
    parser = argparse.ArgumentParser(
        description='Визуализатор ячеек сетки',
        formatter_class=argparse.ArgumentDefaultsHelpFormatter
    )
    parser.add_argument('-f', '--file', default='mesh_data.json', help='Путь к JSON файлу')
    parser.add_argument('-x', '--x-slice', type=float, help='Позиция сечения по оси X')
    parser.add_argument('-y', '--y-slice', type=float, help='Позиция сечения по оси Y')
    parser.add_argument('-z', '--z-slice', type=float, help='Позиция сечения по оси Z')

    args = parser.parse_args()

    try:
        cells, sensors = load_from_json(args.file)
        plot_cell_mesh(
            cells=cells,
            sensors=sensors,
            x_slice=0,
            y_slice=0,
            z_slice=-9
        )
    except Exception as e:
        print(f"\nОшибка: {str(e)}")
        exit(1)