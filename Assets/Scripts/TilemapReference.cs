using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapReference : MonoBehaviour
{
    public static TilemapReference Instance { get; private set; }

    public Grid grid;          // ★ Gridへの参照を追加
    public Tilemap tilemap;
    public TileBase soilTile;
    public TileBase wastelandTile;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
}
