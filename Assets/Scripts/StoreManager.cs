using UnityEngine;

public class StoreManager : MonoBehaviour
{
    public GameObject workerPrefab;  // 雇うバイトのプレハブ
    public Transform workerSpawnPoint; // バイトを出現させる場所

    public int cabbageSeedCost = 600;
    public int tomatoSeedCost = 800;
    public int workerCost = 5000;

    public static StoreManager Instance { get; private set; }

     [Header("販売する野菜プレハブ")]
    public GameObject cabbagePrefab;
    public GameObject tomatoPrefab;
    public GameObject noMoneyMessage;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    public void Start()
    {
        noMoneyMessage.SetActive(false);
    }
    public GameObject GetPrefabByType(VegetableType type)
    {
        switch (type)
        {
            case VegetableType.Cabbage:
                return cabbagePrefab;
            case VegetableType.Tomato:
                return tomatoPrefab;
            default:
                return null;
        }
    }

    public void BuyCabbageSeed()
    {
        if (Parameter.money >= cabbageSeedCost)
        {
            Parameter.money -= cabbageSeedCost;
            SeedInventory.Instance.AddCabbageSeed(5); // 種をn個追加
            Debug.Log("キャベツの種を購入");
            AudioController.Instance.PlaySE("種購入", 0.2f);
        }
        else
        {
            Debug.Log("お金が足りない");
            StartCoroutine(ShowMessageController.Instance.ShowMessage(noMoneyMessage, 1.3f));
        }
    }

    public void BuyTomatoSeed()
    {
        if (Parameter.money >= tomatoSeedCost)
        {
            Parameter.money -= tomatoSeedCost;
            SeedInventory.Instance.AddTomatoSeed(10); // 種をn個追加
            Debug.Log("トマトの種を購入");
            AudioController.Instance.PlaySE("種購入", 0.2f);
        }
        else
        {
            Debug.Log("お金が足りない");
            StartCoroutine(ShowMessageController.Instance.ShowMessage(noMoneyMessage, 1.3f));

        }
    }

    public void HireWorker()
    {
        if (Parameter.money >= workerCost)
        {
            Parameter.money -= workerCost;
            Instantiate(workerPrefab, workerSpawnPoint.position, Quaternion.identity);
            Debug.Log("バイトを雇った");
        }
        else
        {
            Debug.Log("お金が足りない");
            StartCoroutine(ShowMessageController.Instance.ShowMessage(noMoneyMessage, 1.3f));
        }
    }

}
