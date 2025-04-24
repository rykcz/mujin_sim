using System.Collections.Generic;
using UnityEngine;

public enum TaskType
{
    None,
    Till,   // 耕す
    PlantCabbage,
    PlantTomato,
    Water,
    Harvest,
    GoHome,
    Move
}

public class TaskData
{
    public TaskType taskType;
    public Vector3Int targetCell; // タイルマップ上の座標

    public TaskData(TaskType type, Vector3Int cell)
    {
        taskType = type;
        targetCell = cell;
    }
}
