using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI dayText;
    public Image brightnessOverlay;

    private Color targetColor;
    private float colorLerpSpeed = 5.0f;
    private int currentDay = 1;
    private string lastTimeOfDay = "";

    private void Update()
    {
        UpdateGameTimeDisplay();
        UpdateMoneyDisplay();
        UpdateBrightness();
        UpdateDayDisplay();
    }

    private void UpdateGameTimeDisplay()
    {
        var (hours, minutes) = GameTimeManager.Instance.GetCurrentTime();
        timeText.text = $"{hours:D2}:{minutes:D2}";
    }

    private void UpdateMoneyDisplay()
    {
        moneyText.text = $"{Parameter.money:N0}";

        if (Parameter.money < 0)
        {
            moneyText.color = Color.red;
        }
        else
        {
            moneyText.color = Color.white;
        }
    }

    private void UpdateDayDisplay()
    {
        if(currentDay != Parameter.day){
            currentDay = Parameter.day;
            dayText.text = $"{Parameter.day}";
        }
    }

    public void AddMoney(int amount)
    {
        Parameter.money += amount;
    }

    public void SetMoney(int amount)
    {
        Parameter.money = amount;
    }

    public int GetMoney()
    {
        return Parameter.money;
    }

    private void UpdateBrightness()
    {
        if (brightnessOverlay == null) return;

        string timeOfDay = GameTimeManager.Instance.GetTimeOfDay();
        Color newTargetColor = Color.clear; // デフォルト透明

        if (timeOfDay != lastTimeOfDay)
        {
            // 時間帯が変わった時にSE
            switch (timeOfDay)
            {
                case "Morning":

                    break;
                case "Afternoon":

                    break;
                case "Evening":
                    AudioController.Instance.PlaySE("夕方", 0.2f);
                    break;
                case "Night":
                    AudioController.Instance.PlaySE("夜", 0.2f);
                    break;
                case "Dawn":
                    AudioController.Instance.PlaySE("夜明け", 0.2f);
                    break;
            }

            lastTimeOfDay = timeOfDay; // 今の時間帯を記録
        }

        switch (timeOfDay)
        {
            case "Morning":
                newTargetColor = new Color(1f, 1f, 1f, 0.03f);
                break;
            case "Afternoon":
                newTargetColor = new Color(1f, 1f, 1f, 0f);
                break;
            case "Evening":
                newTargetColor = new Color(1f, 0.7f, 0.4f, 0.1f);
                break;
            case "Night":
                newTargetColor = new Color(0.2f, 0.2f, 0.4f, 0.8f);
                break;
            case "Dawn":
                newTargetColor = new Color(0.4f, 0.7f, 1f, 0.2f);
                break;
        }

        // ターゲットカラーが変わったときだけ更新
        if (newTargetColor != targetColor)
        {
            targetColor = newTargetColor;
        }

        // 現在の色をターゲットに向かって補間
        brightnessOverlay.color = Color.Lerp(brightnessOverlay.color, targetColor, Time.deltaTime * colorLerpSpeed);
    }
}
