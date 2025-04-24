using UnityEngine;
using TMPro; // ★ここを追加

public class SeedInventory : MonoBehaviour
{
    public static SeedInventory Instance { get; private set; }

    public int cabbageSeedCount = 0;
    public int tomatoSeedCount = 0;

    public TMP_Text cabbageSeedText;
    public TMP_Text tomatoSeedText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        UpdateUI();
    }

    public void AddCabbageSeed(int amount)
    {
        cabbageSeedCount += amount;
        UpdateUI();
    }

    public void AddTomatoSeed(int amount)
    {
        tomatoSeedCount += amount;
        UpdateUI();
    }

    public bool UseCabbageSeed()
    {
        if (cabbageSeedCount > 0)
        {
            cabbageSeedCount--;
            UpdateUI();
            return true;
        }
        return false;
    }

    public bool UseTomatoSeed()
    {
        if (tomatoSeedCount > 0)
        {
            tomatoSeedCount--;
            UpdateUI();
            return true;
        }
        return false;
    }

    private void UpdateUI()
    {
        if (cabbageSeedText != null)
            cabbageSeedText.text = $"{cabbageSeedCount}";

        if (tomatoSeedText != null)
            tomatoSeedText.text = $"{tomatoSeedCount}";
    }
}
