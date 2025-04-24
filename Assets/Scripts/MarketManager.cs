using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MarketManager : MonoBehaviour
{
    public static MarketManager Instance { get; private set; }

    public Transform[] stallPositions; // 野菜を並べる位置
    private List<GameObject> itemsOnSale = new List<GameObject>();
    public int cabbagePrice = 400;
    public int tomatoPrice = 150;

    //メッセージ表示関連
    public GameObject soldMessage_cabbage;
    public GameObject soldMessage_tomato;
    public GameObject noSpaceMessage;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void Start()
    {
        soldMessage_cabbage.SetActive(false);
        soldMessage_tomato.SetActive(false);
        noSpaceMessage.SetActive(false);
    }

    public void AddItemToMarket(VegetableGrowth vegetable)
    {
        if (vegetable == null) return;

        GameObject prefab = null;

        // VegetableType によってPrefabを選ぶ
        if (vegetable.vegetableType == VegetableType.Cabbage)
        {
            prefab = StoreManager.Instance.cabbagePrefab;
        }
        else if (vegetable.vegetableType == VegetableType.Tomato)
        {
            prefab = StoreManager.Instance.tomatoPrefab;
        }

        if (prefab == null)
        {
            Debug.LogError("プレハブが見つかりません！");
            return;
        }

        // 空いている販売台（stall）を探す
        foreach (Transform stall in stallPositions)
        {
            if (stall.childCount == 0)
            {
                // ★販売台にプレハブを並べる
                GameObject newItem = Instantiate(prefab, stall.position, Quaternion.identity, stall);

                // ★販売リストにも登録（これが重要！）
                itemsOnSale.Add(newItem);

                return; // 最初に空いていた台に置けたらそこで終わり
            }
        }

        Debug.LogWarning("販売所に空きがありません！");
        StartCoroutine(ShowMessageController.Instance.ShowMessage(noSpaceMessage, 1.3f));
    }

    public void SellRandomItem()
    {
        if (itemsOnSale.Count == 0)
        {
            Debug.Log("売れるアイテムがない！");
            return;
        }

        var item = itemsOnSale[0];
        itemsOnSale.RemoveAt(0);

        int sellPrice = 100; // デフォルト

        var marketItem = item.GetComponent<MarketItem>();
        if (marketItem != null)
        {
            switch (marketItem.vegetableType)
            {
                case VegetableType.Cabbage:
                    sellPrice = cabbagePrice;
                    break;
                case VegetableType.Tomato:
                    sellPrice = tomatoPrice;
                    break;
            }
        }
        else
        {
            Debug.LogWarning("MarketItemコンポーネントが付いてないのでデフォルト価格で販売します！");
        }

        Destroy(item);

        Parameter.money += sellPrice;
        Parameter.soldVegetableCount += 1;
        AudioController.Instance.PlaySE("販売", 0.2f);

        Debug.Log($"🛒 {marketItem?.vegetableType} を {sellPrice} 円で売却！");
    }

    // アイテムを予約してリストから外す
    public GameObject ReserveFirstItem()
    {
        if (itemsOnSale.Count == 0)
            return null;

        GameObject item = itemsOnSale[0];
        itemsOnSale.RemoveAt(0); // リストから即外す
        return item;
    }

    // 予約しておいたアイテムを正式に売る
    public void FinishSellingItem(GameObject item)
    {
        if (item == null)
            return;

        int sellPrice = 100; // デフォルト

        var marketItem = item.GetComponent<MarketItem>();
        if (marketItem != null)
        {
            switch (marketItem.vegetableType)
            {
                case VegetableType.Cabbage:
                    sellPrice = cabbagePrice;
                    StartCoroutine(ShowMessageController.Instance.ShowMessage(soldMessage_cabbage, 1.3f));
                    break;
                case VegetableType.Tomato:
                    sellPrice = tomatoPrice;
                    StartCoroutine(ShowMessageController.Instance.ShowMessage(soldMessage_tomato, 1.3f));
                    break;
            }
        }

        Destroy(item);

        Parameter.money += sellPrice;
        Parameter.soldVegetableCount += 1;
        AudioController.Instance.PlaySE("販売", 0.2f);

        Debug.Log($"🛒 {marketItem.vegetableType} を {sellPrice} 円で売却！");
    }

    public float GetLastStallX()
    {
        if (stallPositions.Length == 0) return 0f;
        return stallPositions[stallPositions.Length - 1].position.x;
    }

    public Transform GetNearestStall(float customerX)
    {
        Transform nearestStall = null;
        float minDistance = float.MaxValue;

        foreach (Transform stall in stallPositions)
        {
            // ★子供（野菜）がいるStallだけ対象にする！
            if (stall.childCount > 0)
            {
                float distance = Mathf.Abs(customerX - stall.position.x);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestStall = stall;
                }
            }
        }

        return nearestStall;
    }

    public int GetFreeStallCount() //販売所空きチェック
    {
        int count = 0;
        foreach (Transform stall in stallPositions)
        {
            if (stall.childCount == 0)
                count++;
        }
        return count;
    }
}