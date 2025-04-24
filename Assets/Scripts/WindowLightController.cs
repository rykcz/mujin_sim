using UnityEngine;

public class WindowLightController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (GameTimeManager.Instance.GetTimeOfDay() == "Night")
        {
            // 夜なら窓を光らせる
            spriteRenderer.color = new Color(1f, 1f, 0.7f, 1f); // 少し黄色っぽい白
        }
        else
        {
            // 昼・夕方なら暗くしておく
            spriteRenderer.color = new Color(0.3f, 0.3f, 0.3f, 1f); // 暗い灰色
        }
    }
}
