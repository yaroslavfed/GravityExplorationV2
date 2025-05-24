import argparse
import json
from dataclasses import dataclass
from typing import List, Optional

import matplotlib.pyplot as plt
import numpy as np
from matplotlib.cm import ScalarMappable
from matplotlib.patches import Polygon
from mpl_toolkits.mplot3d.art3d import Line3DCollection
from scipy.spatial import ConvexHull


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
    """Загрузка данных из JSON файла"""
    try:
        with open(file_path, 'r') as f:
            data = json.load(f)
    except FileNotFoundError:
        raise ValueError(f"Файл {file_path} не найден")
    except json.JSONDecodeError:
        raise ValueError(f"Ошибка парсинга JSON в файле {file_path}")

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

    return cells, sensors


def get_cell_edges(cell: Cell) -> np.ndarray:
    """Генерирует рёбра ячейки в формате аналогичном FiniteElement"""
    corners = np.array([
        [cell.CenterX - cell.BoundX, cell.CenterY - cell.BoundY, cell.CenterZ - cell.BoundZ],
        [cell.CenterX + cell.BoundX, cell.CenterY - cell.BoundY, cell.CenterZ - cell.BoundZ],
        [cell.CenterX + cell.BoundX, cell.CenterY + cell.BoundY, cell.CenterZ - cell.BoundZ],
        [cell.CenterX - cell.BoundX, cell.CenterY + cell.BoundY, cell.CenterZ - cell.BoundZ],
        [cell.CenterX - cell.BoundX, cell.CenterY - cell.BoundY, cell.CenterZ + cell.BoundZ],
        [cell.CenterX + cell.BoundX, cell.CenterY - cell.BoundY, cell.CenterZ + cell.BoundZ],
        [cell.CenterX + cell.BoundX, cell.CenterY + cell.BoundY, cell.CenterZ + cell.BoundZ],
        [cell.CenterX - cell.BoundX, cell.CenterY + cell.BoundY, cell.CenterZ + cell.BoundZ]
    ])

    # Определение рёбер куба
    edges = [
        [corners[0], corners[1]], [corners[1], corners[2]], [corners[2], corners[3]], [corners[3], corners[0]],
        [corners[4], corners[5]], [corners[5], corners[6]], [corners[6], corners[7]], [corners[7], corners[4]],
        [corners[0], corners[4]], [corners[1], corners[5]], [corners[2], corners[6]], [corners[3], corners[7]]
    ]
    return np.array(edges)


def get_cell_corners(cell: Cell) -> np.ndarray:
    """Возвращает все 8 углов ячейки"""
    return np.array([
        [cell.CenterX - cell.BoundX, cell.CenterY - cell.BoundY, cell.CenterZ - cell.BoundZ],
        [cell.CenterX + cell.BoundX, cell.CenterY - cell.BoundY, cell.CenterZ - cell.BoundZ],
        [cell.CenterX + cell.BoundX, cell.CenterY + cell.BoundY, cell.CenterZ - cell.BoundZ],
        [cell.CenterX - cell.BoundX, cell.CenterY + cell.BoundY, cell.CenterZ - cell.BoundZ],
        [cell.CenterX - cell.BoundX, cell.CenterY - cell.BoundY, cell.CenterZ + cell.BoundZ],
        [cell.CenterX + cell.BoundX, cell.CenterY - cell.BoundY, cell.CenterZ + cell.BoundZ],
        [cell.CenterX + cell.BoundX, cell.CenterY + cell.BoundY, cell.CenterZ + cell.BoundZ],
        [cell.CenterX - cell.BoundX, cell.CenterY + cell.BoundY, cell.CenterZ + cell.BoundZ]
    ])


def plot_cell_mesh(
        cells: List[Cell],
        sensors: List[Sensor],
        x_slice: Optional[float] = None,
        y_slice: Optional[float] = None,
        z_slice: Optional[float] = None
):
    """Функция визуализации с идентичным стилем"""
    fig = plt.figure(figsize=(18, 12))
    gs = fig.add_gridspec(2, 2,
                          left=0.05, right=0.88,
                          top=0.95, bottom=0.05,
                          wspace=0.3, hspace=0.3)

    ax3d = fig.add_subplot(gs[0, 0], projection='3d')
    ax_top_right = fig.add_subplot(gs[0, 1])
    ax_bottom_left = fig.add_subplot(gs[1, 0])
    ax_bottom_right = fig.add_subplot(gs[1, 1])

    # Настройки как в оригинальном скрипте
    densities = [cell.Density for cell in cells]
    norm = plt.Normalize(min(densities), max(densities))
    cmap = plt.get_cmap('RdYlGn_r')
    mappable = ScalarMappable(norm=norm, cmap=cmap)

    # Пересчитаем границы с учётом всех углов ячеек
    all_points = np.vstack([get_cell_corners(cell) for cell in cells])
    min_vals = all_points.min(axis=0)
    max_vals = all_points.max(axis=0)
    padding = 0.1

    # Вычисляем диапазоны с учетом padding
    ranges = max_vals - min_vals
    padding_3d = padding * np.max(ranges)

    # Устанавливаем одинаковые пропорции для всех осей
    ax3d.set_box_aspect([
        max_vals[0] - min_vals[0] + 2 * padding_3d,
        max_vals[1] - min_vals[1] + 2 * padding_3d,
        max_vals[2] - min_vals[2] + 2 * padding_3d
    ])

    # Устанавливаем пределы с одинаковым padding для всех осей
    ax3d.set_xlim(min_vals[0] - padding_3d, max_vals[0] + padding_3d)
    ax3d.set_ylim(min_vals[1] - padding_3d, max_vals[1] + padding_3d)
    ax3d.set_zlim(min_vals[2] - padding_3d, max_vals[2] + padding_3d)

    # 3D визуализация (как у FiniteElement)
    if sensors:
        sensor_coords = np.array([[s.Position.X, s.Position.Y, s.Position.Z] for s in sensors])
        ax3d.scatter(
            sensor_coords[:, 0], sensor_coords[:, 1], sensor_coords[:, 2],
            c='red', marker='o', s=50, edgecolors='black', linewidths=0.3,
            label='Sensors', alpha=0.3
        )

    # Отрисовка рёбер вместо граней
    for cell in cells:
        color = cmap(norm(cell.Density))
        edges = get_cell_edges(cell)
        line_collection = Line3DCollection(
            edges,
            colors=color,
            linewidths=1.5,
            alpha=0.7
        )
        ax3d.add_collection3d(line_collection)

    # Одинаковые настройки осей
    ax3d.xaxis.set_pane_color((0.95, 0.95, 0.95, 0.1))
    ax3d.yaxis.set_pane_color((0.95, 0.95, 0.95, 0.1))
    ax3d.zaxis.set_pane_color((0.95, 0.95, 0.95, 0.1))
    ax3d.xaxis._axinfo["grid"].update({"linewidth": 0.5, "color": "gray"})
    ax3d.yaxis._axinfo["grid"].update({"linewidth": 0.5, "color": "gray"})
    ax3d.zaxis._axinfo["grid"].update({"linewidth": 0.5, "color": "gray"})

    ax3d.set_xlabel('X', fontsize=12, labelpad=15)
    ax3d.set_ylabel('Y', fontsize=12, labelpad=15)
    ax3d.set_zlabel('Z', fontsize=12, labelpad=15)
    ax3d.set_title('3D View', pad=20)

    # Функции для проекций (аналогичные оригиналу)
    def get_cell_projection_points(cell: Cell, plane: str) -> List[List[float]]:
        corners = [
            (cell.CenterX - cell.BoundX, cell.CenterY - cell.BoundY, cell.CenterZ - cell.BoundZ),
            (cell.CenterX + cell.BoundX, cell.CenterY - cell.BoundY, cell.CenterZ - cell.BoundZ),
            (cell.CenterX + cell.BoundX, cell.CenterY + cell.BoundY, cell.CenterZ - cell.BoundZ),
            (cell.CenterX - cell.BoundX, cell.CenterY + cell.BoundY, cell.CenterZ - cell.BoundZ),
            (cell.CenterX - cell.BoundX, cell.CenterY - cell.BoundY, cell.CenterZ + cell.BoundZ),
            (cell.CenterX + cell.BoundX, cell.CenterY - cell.BoundY, cell.CenterZ + cell.BoundZ),
            (cell.CenterX + cell.BoundX, cell.CenterY + cell.BoundY, cell.CenterZ + cell.BoundZ),
            (cell.CenterX - cell.BoundX, cell.CenterY + cell.BoundY, cell.CenterZ + cell.BoundZ)
        ]

        proj_points = []
        for point in corners:
            if plane == 'xy':
                proj_points.append([point[0], point[1]])
            elif plane == 'xz':
                proj_points.append([point[0], point[2]])
            else:
                proj_points.append([point[1], point[2]])
        return proj_points

    def draw_projection(ax, plane: str):
        ax.cla()
        ax.set_title(f"{plane.upper()} Projection")
        ax.grid(True, linestyle='--', alpha=0.3)

        for cell in cells:
            color = cmap(norm(cell.Density))
            points = get_cell_projection_points(cell, plane)

            try:
                hull = ConvexHull(points)
                poly = Polygon(
                    np.array(points)[hull.vertices],
                    closed=True,
                    facecolor=color,
                    edgecolor='k',
                    alpha=1.0
                )
                ax.add_patch(poly)
            except:
                for edge in get_cell_edges(cell):
                    proj_edge = [
                        [edge[0][0], edge[0][1]] if plane == 'xy' else
                        [edge[0][0], edge[0][2]] if plane == 'xz' else
                        [edge[0][1], edge[0][2]],
                        [edge[1][0], edge[1][1]] if plane == 'xy' else
                        [edge[1][0], edge[1][2]] if plane == 'xz' else
                        [edge[1][1], edge[1][2]]
                    ]
                    ax.plot(
                        [proj_edge[0][0], proj_edge[1][0]],
                        [proj_edge[0][1], proj_edge[1][1]],
                        color=color, linewidth=1
                    )

        ax.set_xlim(bounds[plane]['x'])
        ax.set_ylim(bounds[plane]['y'])
        ax.set_aspect('equal')

    # Обработка сечений (полностью аналогичная оригиналу)
    def draw_slice(ax, axis: str, position: float):
        ax.cla()
        ax.set_title(f"Сечение по {axis}={position:.2f}")
        ax.grid(True, linestyle='dotted', alpha=0.5)

        axis_index = {'X': 0, 'Y': 1, 'Z': 2}[axis]
        for cell in cells:
            color = cmap(norm(cell.Density))
            edges = get_cell_edges(cell)
            slice_points = []

            for edge in edges:
                coord1 = edge[0][axis_index]
                coord2 = edge[1][axis_index]

                if (coord1 <= position <= coord2) or (coord2 <= position <= coord1):
                    t = (position - coord1) / (coord2 - coord1 + 1e-9)
                    point = edge[0] + t * (edge[1] - edge[0])
                    slice_points.append(
                        [point[i] for i in [0, 1, 2] if i != axis_index]
                    )

            if len(slice_points) >= 3:
                try:
                    hull = ConvexHull(slice_points)
                    poly = Polygon(
                        np.array(slice_points)[hull.vertices],
                        closed=True,
                        facecolor=color,
                        edgecolor='k',
                        alpha=1.0
                    )
                    ax.add_patch(poly)
                except:
                    pass

        ax.autoscale_view()
        ax.set_aspect('equal')

    # Настройка отображения (как в оригинале)
    if x_slice is not None:
        draw_slice(ax_bottom_right, 'X', x_slice)
        ax_bottom_right.set(xlabel='Y', ylabel='Z')
    else:
        draw_projection(ax_bottom_right, 'yz')

    if y_slice is not None:
        draw_slice(ax_bottom_left, 'Y', y_slice)
        ax_bottom_left.set(xlabel='X', ylabel='Z')
    else:
        draw_projection(ax_bottom_left, 'xz')

    if z_slice is not None:
        draw_slice(ax_top_right, 'Z', z_slice)
        ax_top_right.set(xlabel='X', ylabel='Y')
    else:
        draw_projection(ax_top_right, 'xy')

    # Цветовая шкала
    cbar_ax = fig.add_axes([0.90, 0.15, 0.02, 0.7])
    fig.colorbar(mappable, cax=cbar_ax, label='Mu')

    plt.savefig("graph.png", dpi=300)
    plt.show()


if __name__ == "__main__":
    parser = argparse.ArgumentParser(
        description='Визуализатор ячеек сетки',
        formatter_class=argparse.ArgumentDefaultsHelpFormatter
    )
    parser.add_argument('-f', '--file', default='mesh_data.json', help='Путь к JSON файлу')
    parser.add_argument('-x', '--x-slice', type=float)
    parser.add_argument('-y', '--y-slice', type=float)
    parser.add_argument('-z', '--z-slice', type=float)

    args = parser.parse_args()

    try:
        cells, sensors = load_from_json(args.file)
        plot_cell_mesh(
            cells=cells,
            sensors=sensors,
            x_slice=0,
            y_slice=0,
            z_slice=-7
        )
    except Exception as e:
        print(f"\nОшибка: {str(e)}")
        exit(1)
