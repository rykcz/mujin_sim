using UnityEngine;
using System.Collections;

public class CustomerController : MonoBehaviour
{
    public float moveSpeed = 2f;

    private bool hasPassedShop = false; // 🛍 販売所通過チェック
    private bool isBuying = false;

    private Vector3 goalPosition;
    private Vector3 moveDirection;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private GameObject reservedItem = null;
    public float sellChance = 0.7f; //購入する確率。1なら100%購入

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // ゴールに向かう方向を正規化（Start時にセット）
        moveDirection = (goalPosition - transform.position).normalized;
    }

    private void Update()
    {
        if (isBuying) return; // 買い物中は移動禁止

        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        // まだ販売所を通過してないならチェックする
        if (!hasPassedShop)
        {
            // CheckStallProximity();

            float customerX = transform.position.x;
            Transform nearestStall = MarketManager.Instance.GetNearestStall(customerX);

            if (nearestStall != null)
            {
                float stallX = nearestStall.position.x;

                // ❗ ここで都度最新のstallXを取ってくる！
                if (Mathf.Abs(customerX - stallX) < sellChance)
                {
                    hasPassedShop = true;

                    float chance = Random.value;
                    if (chance < sellChance)
                    {
                        StartCoroutine(BuyItemRoutine());
                    }
                    else
                    {
                        Debug.Log("🚶 通行人は素通りしました！");
                    }
                }
            }

        }

        // ゴールに着いたら消す
        if (Vector3.Distance(transform.position, goalPosition) < 0.1f)
        {
            Destroy(gameObject);
        }
    }

    public void SetGoal(Vector3 goalPos)
    {
        this.goalPosition = goalPos;
    }

    private IEnumerator BuyItemRoutine()
    {
        isBuying = true;

        reservedItem = MarketManager.Instance.ReserveFirstItem();

        if (reservedItem == null)
        {
            Debug.LogWarning("🛒 購入できるアイテムがありませんでした！");
            isBuying = false;
            yield break;
        }

        if (animator != null)
        {
            animator.Play("Customer_LeftUp");
        }

        yield return new WaitForSeconds(1f);

        MarketManager.Instance.FinishSellingItem(reservedItem);

        isBuying = false;

        if (animator != null)
        {
            animator.Play("Customer_LeftDown"); //アニメーション戻す
        }
    }
}
