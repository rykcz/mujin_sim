using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WastelandRecoveryManager : MonoBehaviour
{
    public static WastelandRecoveryManager Instance { get; private set; }

    private class TilledCellInfo
    {
        public Vector3Int cellPos;
        public int tilledDay; // 耕した日数保存用
    }

    private List<TilledCellInfo> tilledCells = new List<TilledCellInfo>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // 耕したときに登録
    public void RegisterTilledCell(Vector3Int cell)
    {
        tilledCells.Add(new TilledCellInfo
        {
            cellPos = cell,
            tilledDay = Parameter.day
        });
    }

    // 日付が進んだときにチェック
    public void CheckAndRecoverTiles()
    {
        Tilemap tilemap = TilemapReference.Instance.tilemap;
        List<TilledCellInfo> toRemove = new List<TilledCellInfo>();

        foreach (var info in tilledCells)
        {
            if (Parameter.day >= info.tilledDay + 2) // 2日経過したら
            {
                // 野菜がいるかチェック
                if (VegetableMapManager.Instance.HasVegetable(info.cellPos))
                {
                    Debug.Log($"土タイル上に野菜がいるので破壊 {info.cellPos}");

                    var veg = VegetableMapManager.Instance.GetVegetable(info.cellPos);
                    if (veg != null)
                    {
                        veg.MarkForDestroy();
                        Destroy(veg.gameObject);

                        VegetableMapManager.Instance.UnregisterVegetable(info.cellPos);
                        OccupiedMapManager.Instance.UnregisterCell(info.cellPos);

                        AudioController.Instance.PlaySE("野菜破壊"); // 破壊音
                    }
                }

                // 荒れ地に戻す
                tilemap.SetTile(info.cellPos, TilemapReference.Instance.wastelandTile);

                toRemove.Add(info);
            }
        }

        // 登録解除
        foreach (var info in toRemove)
        {
            tilledCells.Remove(info);
        }
    }
}
