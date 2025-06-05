using System.Text.Json;
using Common.Data;

namespace ReverseProblem.Core.Services.ConverterService;

public static class ConverterService
{
    public static void Convert()
    {
        // Загрузка JSON
        string json = File.ReadAllText("exit_mesh_model.json");
        var grid = JsonSerializer.Deserialize<Common.Data.Mesh>(json)!;

        Mesh mesh = new Mesh();
        var edgeRegistry = new Dictionary<string, int>(); // Хранит уникальные рёбра
        int globalEdgeIndex = 0;

        foreach (var cell in grid.Cells)
        {
            // Создание узлов ячейки (8 углов параллелепипеда)
            var nodes = GenerateNodes(cell);

            // Создание рёбер и регистрация глобальных индексов
            var edges = new List<Edge>();
            foreach (var edgeNodes in GetCubeEdges(nodes))
            {
                string edgeKey = GenerateEdgeKey(edgeNodes[0], edgeNodes[1]);

                if (!edgeRegistry.TryGetValue(edgeKey, out int edgeIndex))
                {
                    edgeIndex = globalEdgeIndex++;
                    edgeRegistry[edgeKey] = edgeIndex;
                }

                edges.Add(new Edge { EdgeIndex = edgeIndex, Nodes = new List<Node> { edgeNodes[0], edgeNodes[1] } });
            }

            // Создание конечного элемента
            mesh.Elements.Add(new FiniteElement { Edges = edges, Mu = cell.Density });
        }

        // Сохранение результата
        File.WriteAllText("output.json", JsonSerializer.Serialize(mesh));
    }

    // Генерация 8 узлов для ячейки
    static List<Node> GenerateNodes(Cell cell)
    {
        double hx = cell.BoundX, hy = cell.BoundY, hz = cell.BoundZ;
        return new List<Node>
        {
            new Node
            {
                NodeIndex = 0,
                Coordinate = new Point3D { X = cell.CenterX - hx, Y = cell.CenterY - hy, Z = cell.CenterZ - hz }
            },
            new Node
            {
                NodeIndex = 1,
                Coordinate = new Point3D { X = cell.CenterX + hx, Y = cell.CenterY - hy, Z = cell.CenterZ - hz }
            },
            new Node
            {
                NodeIndex = 2,
                Coordinate = new Point3D { X = cell.CenterX - hx, Y = cell.CenterY + hy, Z = cell.CenterZ - hz }
            },
            new Node
            {
                NodeIndex = 3,
                Coordinate = new Point3D { X = cell.CenterX + hx, Y = cell.CenterY + hy, Z = cell.CenterZ - hz }
            },
            new Node
            {
                NodeIndex = 4,
                Coordinate = new Point3D { X = cell.CenterX - hx, Y = cell.CenterY - hy, Z = cell.CenterZ + hz }
            },
            new Node
            {
                NodeIndex = 5,
                Coordinate = new Point3D { X = cell.CenterX + hx, Y = cell.CenterY - hy, Z = cell.CenterZ + hz }
            },
            new Node
            {
                NodeIndex = 6,
                Coordinate = new Point3D { X = cell.CenterX - hx, Y = cell.CenterY + hy, Z = cell.CenterZ + hz }
            },
            new Node
            {
                NodeIndex = 7,
                Coordinate = new Point3D { X = cell.CenterX + hx, Y = cell.CenterY + hy, Z = cell.CenterZ + hz }
            }
        };
    }

    // Возвращает 12 рёбер куба (пары узлов)
    static IEnumerable<Node[]> GetCubeEdges(List<Node> nodes)
    {
        int[][] edges = new int[][]
        {
            new[]
            {
                0,
                1
            },
            new[]
            {
                1,
                3
            },
            new[]
            {
                3,
                2
            },
            new[]
            {
                2,
                0
            }, // Нижняя грань
            new[]
            {
                4,
                5
            },
            new[]
            {
                5,
                7
            },
            new[]
            {
                7,
                6
            },
            new[]
            {
                6,
                4
            }, // Верхняя грань
            new[]
            {
                0,
                4
            },
            new[]
            {
                1,
                5
            },
            new[]
            {
                2,
                6
            },
            new[]
            {
                3,
                7
            } // Вертикальные рёбра
        };

        foreach (var pair in edges)
            yield return new[]
            {
                nodes[pair[0]],
                nodes[pair[1]]
            };
    }

    // Уникальный ключ для ребра (A-B и B-A → одинаковый ключ)
    static string GenerateEdgeKey(Node a, Node b)
    {
        var p1 = a.Coordinate;
        var p2 = b.Coordinate;
        return p1.X < p2.X
            ? $"{p1}{p2}"
            : $"{p2}{p1}";
    }
}