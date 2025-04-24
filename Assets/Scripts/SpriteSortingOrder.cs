using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteSortingOrder : MonoBehaviour
{
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        int baseSortingOrder = 1500;
        float yOffset = 0.25f; // Spriteの表示が0.25上にずれている場合など

        sr.sortingOrder = baseSortingOrder - (int)((transform.position.y + yOffset) * 100);
    }
}