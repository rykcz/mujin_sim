using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapObstacle : MonoBehaviour
{
    private Tilemap tilemap;

    void Start()
    {
        tilemap = TilemapReference.Instance.tilemap;
        Register();
    }

    public void Register()
    {
        Vector3Int cell = tilemap.WorldToCell(transform.position);
        OccupiedMapManager.Instance.RegisterCell(cell);
    }

    public void Unregister()
    {
        Vector3Int cell = tilemap.WorldToCell(transform.position);
        OccupiedMapManager.Instance.UnregisterCell(cell);
    }
}