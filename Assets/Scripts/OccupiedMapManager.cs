using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class OccupiedMapManager : MonoBehaviour
{
    public static OccupiedMapManager Instance { get; private set; }

    public Dictionary<Vector3Int, bool> occupiedMap = new Dictionary<Vector3Int, bool>();

    private Tilemap tilemap;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        tilemap = TilemapReference.Instance.tilemap;
        BuildOccupiedMap();
    }

    void BuildOccupiedMap()
    {
        occupiedMap.Clear();

        // "Obstacle" タグのオブジェクトを全て取得
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");

        foreach (GameObject obj in obstacles)
        {
            Vector3 worldPos = tilemap.GetCellCenterWorld(tilemap.WorldToCell(obj.transform.position));
            Vector3Int cell = tilemap.WorldToCell(worldPos);

            if (!occupiedMap.ContainsKey(cell))
            {
                occupiedMap[cell] = true;
                Debug.Log($"占有マス登録: {cell} ({obj.name})");
            }
        }

        Debug.Log($"OccupiedMap 作成完了：{occupiedMap.Count} タイルが占有中");
    }

    /// <summary>
    /// 指定セルが占有されているか
    /// </summary>
    public bool IsCellOccupied(Vector3Int cellPos)
    {
        // 🌊 まず池タイルなら常に占有扱い
        if (IsPondTile(cellPos))
            return true;

        // それ以外は通常の占有判定
        return occupiedMap.ContainsKey(cellPos);
    }

    public void RegisterCell(Vector3Int cell)
    {
        if (!occupiedMap.ContainsKey(cell))
            occupiedMap[cell] = true;
    }

    public void UnregisterCell(Vector3Int cell)
    {
        if (occupiedMap.ContainsKey(cell))
            occupiedMap.Remove(cell);
    }

    /// <summary>
    /// セルが池タイルかどうか判定する
    /// </summary>
    private bool IsPondTile(Vector3Int cell)
    {
        TileBase tile = tilemap.GetTile(cell);
        if (tile == null) return false;

        return tile.name == "mapchip_01_80"; // 水タイル
    }

    public bool IsPondCell(Vector3Int cellPos)
    {
        TileBase tile = TilemapReference.Instance.tilemap.GetTile(cellPos);
        if (tile == null) return false;

        return tile.name == "mapchip_01_80"; // 水タイル
    }
}
