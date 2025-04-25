using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    public GameObject customerPrefab;

    public float spawnIntervalMin = 3f;  // 出現間隔最小
    public float spawnIntervalMax = 8f;  // 出現間隔最大

    private float timer = 0f;
    private float nextSpawnTime;

    [Header("出現位置とゴール位置")]
    public Transform spawnPoint;   // スポーン位置
    public Transform goalPoint;    // 目標位置

    private void Start()
    {
        SetNextSpawnTime();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= nextSpawnTime)
        {
            SpawnCustomer();
            SetNextSpawnTime();
            timer = 0f;
        }
    }

    private void SpawnCustomer()
    {
        if (customerPrefab == null || spawnPoint == null || goalPoint == null)
        {
            Debug.LogError("CustomerSpawnerに必要な設定がない");
            return;
        }

        GameObject customer = Instantiate(customerPrefab, spawnPoint.position, Quaternion.identity);
        CustomerController controller = customer.GetComponent<CustomerController>();

        if (controller != null)
        {
            controller.SetGoal(goalPoint.position); // 出現後にゴール設定
        }
    }

    private void SetNextSpawnTime()
    {
        nextSpawnTime = Random.Range(spawnIntervalMin, spawnIntervalMax);
    }
}
