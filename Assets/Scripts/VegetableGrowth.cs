using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VegetableGrowth : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer growthBarRenderer;

    private float growTimer = 0f;
    private int growthStage = 0; // 0=苗, 1=生育中, 2=収穫期

    public Sprite[] growthSprites; // 成長段階ごとのスプライト
    public VegetableType vegetableType;
    public GameObject growthBarObject;
    public GameObject growthBarBackObject;

    private bool isAlive = true;
    public bool isHarvested = false;

    public int cabbagePrice = 200;
    public int tomatoPrice = 100;

    private float growthSpeedMultiplier = 1f; // ★成長速度の倍率（池の隣なら2倍）
    private float maxGrowthTime = 10f;
    public float cabbageGrowthTime = 10f;
    public float tomatoGrowthTime = 7f;

    [SerializeField] private Material flashMaterial; // ← Inspectorでセットする
    private Material originalMaterial;
    private bool isFlashing = false;

    void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        originalMaterial = spriteRenderer.material;

        if (growthBarObject != null)
        {
            growthBarRenderer = growthBarObject.GetComponent<SpriteRenderer>();
        }

        Vector3Int myCell = TilemapReference.Instance.tilemap.WorldToCell(transform.position);

        // 🌊 自分が池の隣なら成長速度2倍にする
        if (IsNearPond(myCell))
        {
            growthSpeedMultiplier = 2f;
            Debug.Log($"💧 {gameObject.name}: 池の隣なので成長速度2倍！");
        }

        // 🌱 ここで野菜ごとに成長時間を設定！
        if (vegetableType == VegetableType.Tomato)
        {
            maxGrowthTime = tomatoGrowthTime;
        }
        else if (vegetableType == VegetableType.Cabbage)
        {
            maxGrowthTime = cabbageGrowthTime;
        }
    }

    void Update()
    {
        if (spriteRenderer == null || growthSprites == null || growthSprites.Length < 3 || !isAlive) return;

        growTimer += Time.deltaTime * growthSpeedMultiplier;

        // 成長ステージを動的に判定
        if (growTimer > maxGrowthTime * 0.5f && growthStage == 0)
        {
            growthStage = 1;
            spriteRenderer.sprite = growthSprites[1];
        }
        else if (growTimer > maxGrowthTime && growthStage == 1)
        {
            growthStage = 2;
            spriteRenderer.sprite = growthSprites[2];
        }

        UpdateGrowthBar();
    }

    public void MarkForDestroy()
    {
        isAlive = false;
    }

    public void NewDay()
    {
        // ★水やりシステム撤廃！何もなし
    }

    public bool IsFullyGrown()
    {
        return growthStage == 2 && !isHarvested;
    }

    public void MarkAsHarvested()
    {
        isHarvested = true;
    }

    public int GetSellPrice()
    {
        switch (vegetableType)
        {
            case VegetableType.Cabbage:
                return cabbagePrice;
            case VegetableType.Tomato:
                return tomatoPrice;
            default:
                return 100;
        }
    }

    /// <summary>
    /// 指定セルが池タイルか調べる（上下左右）
    /// </summary>
    private bool IsNearPond(Vector3Int cell)
    {
        Vector3Int[] directions = {
            new Vector3Int(1, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, -1, 0)
        };

        foreach (var dir in directions)
        {
            Vector3Int adjacent = cell + dir;

            if (TilemapReference.Instance.tilemap.GetTile(adjacent) != null &&
                TilemapReference.Instance.tilemap.GetTile(adjacent).name == "mapchip_01_80") // ←池タイル名で判定！
            {
                return true;
            }
        }

        return false;
    }

    private void UpdateGrowthBar()
    {
        if (growthBarRenderer == null) return;

        if (growthStage == 2)
        {
            // 収穫期に到達したらバーを非表示
            if (growthBarRenderer.gameObject.activeSelf)
            {
                growthBarRenderer.gameObject.SetActive(false);
                growthBarBackObject.SetActive(false);
            }

            if (!isFlashing) // ★まだフラッシュしてなかったら
            {
                StartCoroutine(FlashWhiteEffect());
                isFlashing = true;
            }
            return;
        }

        float fillAmount = Mathf.Clamp01(growTimer / maxGrowthTime);

        // 🌱 スケール変更
        growthBarRenderer.transform.localScale = new Vector3(Mathf.Max(fillAmount, 0.01f), 1f, 1f);

        growthBarRenderer.transform.localPosition = new Vector3(-0.35f, 0.55f, 0f);

        if (!growthBarRenderer.gameObject.activeSelf)
        {
            growthBarRenderer.gameObject.SetActive(true);
        }

    }

    private IEnumerator FlashWhiteEffect()
    {
        spriteRenderer.material = flashMaterial; // 🔥 Additiveマテリアルに切り替え！

        float flashDuration = 0.2f; // 完全白になるまでの時間
        float fadeDuration = 0.3f; // フラッシュする時間
        float timer = 0f;

        AudioController.Instance.PlaySE("野菜成長", 0.4f);

        // 最初は真っ白にしておく
        yield return new WaitForSeconds(flashDuration);

        // 徐々にフェードさせて戻す
        Material mat = spriteRenderer.material;
        Color startColor = mat.GetColor("_Color");
        Color endColor = new Color(1f, 1f, 1f, 0f); // 完全透明に向かう

        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;

            Color lerpedColor = Color.Lerp(startColor, endColor, t);
            mat.SetColor("_Color", lerpedColor);

            yield return null;
        }

        spriteRenderer.material = originalMaterial; // 🔙 元のマテリアルに戻す
    }

}
