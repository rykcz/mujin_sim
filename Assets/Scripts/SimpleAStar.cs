using System.Collections.Generic;
using UnityEngine;

public class SimpleAStar
{
    private static readonly Vector3Int[] directions = new Vector3Int[]
    {
        new Vector3Int(1, 0, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, -1, 0)
    };

    public static List<Vector3Int> FindPath(Vector3Int start, Vector3Int goal, bool allowOccupied = false, bool allowOccupiedGoal = false)
    {
        int maxSearchCount = 10000;
        int searchCount = 0;

        var openSet = new PriorityQueue<Vector3Int>();
        var cameFrom = new Dictionary<Vector3Int, Vector3Int>();
        var gScore = new Dictionary<Vector3Int, int>();

        openSet.Enqueue(start, 0);
        gScore[start] = 0;

        while (openSet.Count > 0)
        {
            if (searchCount++ > maxSearchCount)
            {
                Debug.LogError("❌ A*探索が上限に達しました！");
                return null;
            }

            Vector3Int current = openSet.Dequeue();

            if (current == goal)
            {
                return ReconstructPath(cameFrom, current);
            }

            foreach (var dir in directions)
            {
                Vector3Int neighbor = current + dir;

                if (!IsInsideValidArea(neighbor))
                {
                    continue;
                }

                bool isGoalCell = (neighbor == goal);

                if (OccupiedMapManager.Instance.IsCellOccupied(neighbor))
                {
                    if (!allowOccupied && !(allowOccupiedGoal && isGoalCell))
                    {
                        continue;
                    }
                }

                int tentativeG = gScore[current] + 1;

                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    int fScore = tentativeG + Heuristic(neighbor, goal);
                    openSet.Enqueue(neighbor, fScore); // ★距離順に追加するから正しく探索続行できる！
                }
            }
        }

        Debug.LogWarning("❌ パスが見つかりませんでした！");
        return null;
    }

    private static int Heuristic(Vector3Int a, Vector3Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y); // マンハッタン距離
    }

    private static List<Vector3Int> ReconstructPath(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int current)
    {
        List<Vector3Int> path = new List<Vector3Int> { current };

        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Add(current);
        }

        path.Reverse();
        return path;
    }

    private static bool IsInsideValidArea(Vector3Int cell)
    {
        return cell.x >= 0 && cell.x < 26 && cell.y >= 0 && cell.y < 26;
    }
}
