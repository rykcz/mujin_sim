using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;

public class TileHighlighter : MonoBehaviour
{
    public Tilemap highlightTilemap;  // ハイライト用Tilemap
    public TileBase highlightTile;    // ハイライト用タイル

    private Vector3Int previousCell;
    public bool isActive = false;

    void Update()
    {
        if (!isActive)
        {
            highlightTilemap.ClearAllTiles(); // 無効なら消す
            return;
        }

        // マウスがUI上ならハイライト非表示
        if (EventSystem.current.IsPointerOverGameObject())
        {
            highlightTilemap.SetTile(previousCell, null);
            return;
        }

        Vector3 mouseScreenPos = Input.mousePosition;
        
        mouseScreenPos.z = Mathf.Abs(Camera.main.transform.position.z);

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        Vector3Int cellPos = highlightTilemap.WorldToCell(worldPos);

        if (cellPos != previousCell)
        {
            highlightTilemap.SetTile(previousCell, null);    // 前のタイル消す
            highlightTilemap.SetTile(cellPos, highlightTile); // 新しいタイル置く
            previousCell = cellPos;
        }
    }

    public void SetActive(bool active)
    {
        isActive = active;
        if (!active)
        {
            highlightTilemap.ClearAllTiles(); // OFFにする時は全部消す
        }
    }

}
