using System.Collections.Generic;
using UnityEngine;

public class VegetableMapManager : MonoBehaviour
{
    public static VegetableMapManager Instance { get; private set; }

    private Dictionary<Vector3Int, VegetableGrowth> vegetableMap = new Dictionary<Vector3Int, VegetableGrowth>();

    public List<Vector3Int> GetAllVegetablePositions()
    {
        return new List<Vector3Int>(vegetableMap.Keys);
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // ★ 登録
    public void RegisterVegetable(Vector3Int cell, VegetableGrowth vegetable)
    {
        if (!vegetableMap.ContainsKey(cell))
        {
            vegetableMap.Add(cell, vegetable);
            Debug.Log($"🌱 野菜登録: {cell}");
        }
    }

    // ★ 削除
    public void UnregisterVegetable(Vector3Int cell)
    {
        if (vegetableMap.ContainsKey(cell))
        {
            vegetableMap.Remove(cell);
            Debug.Log($"🧹 野菜削除: {cell}");
        }
    }

    // ★ 存在チェック
    public bool HasVegetable(Vector3Int cell)
    {
        return vegetableMap.ContainsKey(cell);
    }

    // ★ cell位置にあるVegetableGrowthを返す
    public VegetableGrowth GetVegetable(Vector3Int cell)
    {
        if (vegetableMap.TryGetValue(cell, out VegetableGrowth vegetable))
        {
            return vegetable;
        }
        return null;
    }

}
