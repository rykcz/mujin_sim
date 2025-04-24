#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ObstacleSnapperEditor : EditorWindow
{
    private Tilemap targetTilemap;

    [MenuItem("Tools/スナップ/タイル中央にスナップ（Obstacles）")]

    public static void ShowWindow()
    {
        GetWindow<ObstacleSnapperEditor>("Obstacle Snapper");
    }

    void OnGUI()
    {
        GUILayout.Label("障害物一括スナップツール", EditorStyles.boldLabel);
        targetTilemap = (Tilemap)EditorGUILayout.ObjectField("対象のTilemap", targetTilemap, typeof(Tilemap), true);

        if (GUILayout.Button("スナップ実行"))
        {
            if (targetTilemap == null)
            {
                Debug.LogError("Tilemapが指定されていません！");
                return;
            }

            SnapAllObstaclesToTileCenter();
        }
    }

    void SnapAllObstaclesToTileCenter()
    {
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");

        int count = 0;
        foreach (var obj in obstacles)
        {
            Vector3Int cell = targetTilemap.WorldToCell(obj.transform.position);
            Vector3 centerPos = targetTilemap.GetCellCenterWorld(cell);
            Undo.RecordObject(obj.transform, "Snap Obstacle");
            obj.transform.position = centerPos;
            count++;
        }

        Debug.Log($"障害物をスナップ完了: {count} 個スナップされました。");
    }
}
#endif
