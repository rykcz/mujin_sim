using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameTimeManager : MonoBehaviour
{
    public static GameTimeManager Instance { get; private set; }

    public float currentTime = 6f;
    public float timeSpeed = 12f;
    private bool newDayStarted = false;

    //維持費メッセージ用
    public GameObject costMessage;

    [Header("UI")]
    public Image speedImage;

    // Worker用リスト
    private List<WorkerController> allWorkers = new List<WorkerController>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // 全Workerを確認
        allWorkers.AddRange(FindObjectsOfType<WorkerController>(true));
    }

    private void Start()
    {
        costMessage.SetActive(false);
    }

    private void Update()
    {
        // スタート時の説明中は時間制御も進行もスキップ
        if (GameExplanationManager.Instance != null && GameExplanationManager.Instance.IsDuringExplanation())
        {
            return;
        }

        //リザルトでも停止
        if (ResultManager.Instance != null && ResultManager.Instance.IsResultOpen())
        {
            return;
        }

        AdjustGameSpeed();

        currentTime += Time.deltaTime * timeSpeed / 60f;

        if (currentTime >= 18f && currentTime < 18.1f)
        {
            TaskManager.Instance.ForceAllWorkersGoHome();
        }

        if (currentTime >= 24f)
        {
            currentTime -= 24f;
            newDayStarted = false;
            Parameter.day += 1;

            if (Parameter.day == (Parameter.limitDay))
            {
                // リザルト処理
                ResultManager.Instance.OpenResult();
            }
            else
            {
                Parameter.money -= Parameter.dayCostMoney;
                if(Parameter.money < 0)
                {
                    // 維持費払えずリザルト処理
                    ResultManager.Instance.OpenResult();
                    return;
                }
                StartCoroutine(ShowMessageController.Instance.ShowMessage(costMessage, 12f));
                WastelandRecoveryManager.Instance.CheckAndRecoverTiles();
            }
        }

        if (!newDayStarted && Mathf.FloorToInt(currentTime) == 6)
        {
            StartNewDay();
            newDayStarted = true;
        }
    }

    private void AdjustGameSpeed()
    {
        if (currentTime >= 18f || currentTime < 5.9f)
        {
            Time.timeScale = 6.0f;
            UpdateSpeedImage(true);
        }
        else
        {
            Time.timeScale = 1.0f;
            UpdateSpeedImage(false);
        }
    }

    private void UpdateSpeedImage(bool isFast)
    {
        if (speedImage == null) return;
        speedImage.gameObject.SetActive(isFast);
    }

    private void StartNewDay()
    {
        // 作物すべてに NewDay渡す
        VegetableGrowth[] crops = FindObjectsOfType<VegetableGrowth>();
        foreach (var crop in crops)
        {
            crop.NewDay();
        }

        // Worker復活
        foreach (var worker in allWorkers)
        {
            if (worker == null) continue;

            worker.gameObject.SetActive(true); // 表示

            // 状態リセット
            worker.ResetTask();  // タスク情報など初期化

            var fade = worker.GetComponent<WorkerFadeController>();
            if (fade != null)
            {
                fade.FadeIn();
            }
        }
    }

    public (int hours, int minutes) GetCurrentTime()
    {
        int totalMinutes = Mathf.FloorToInt(currentTime * 60f);
        int hours = (totalMinutes / 60) % 24;
        int minutes = totalMinutes % 60;
        return (hours, minutes);
    }

    public string GetTimeOfDay()
    {
        var (hours, _) = GetCurrentTime();

        if (hours >= 6 && hours < 12)
            return "Morning";
        else if (hours >= 12 && hours < 17)
            return "Afternoon";
        else if (hours >= 17 && hours < 19)
            return "Evening";
        else if (hours >= 4 && hours < 6)
            return "Dawn";
        else
            return "Night";
    }
}
