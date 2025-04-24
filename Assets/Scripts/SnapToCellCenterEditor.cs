#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteInEditMode]
public class SnapToCellCenterEditor : MonoBehaviour
{
    [ContextMenu("Snap To Cell Center")]
    void Snap()
    {
        Tilemap tilemap = FindObjectOfType<Tilemap>();
        if (tilemap == null) return;

        Vector3Int cell = tilemap.WorldToCell(transform.position);
        transform.position = tilemap.GetCellCenterWorld(cell);
    }
}
#endif
