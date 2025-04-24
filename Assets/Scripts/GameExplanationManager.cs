using UnityEngine;
using UnityEngine.UI;

public class GameExplanationManager : MonoBehaviour
{
    public static GameExplanationManager Instance { get; private set; }

    public GameObject explanationUI;       // 説明全体のUI
    public Image explanationImage;         // 表示する画像
    public Sprite[] explanationPages;      // ページ画像リスト

    public Button nextButton;               // 次へ進むボタン
    public GameObject daySelectPanel;       // 日数選択ボタンをまとめたパネル
    public Button day5Button;
    public Button day10Button;
    public Button day30Button;
    public Button noLimitButton;

    private int currentPage = 0;
    private bool isDuringExplanation = true;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
            Time.timeScale = 0f;
    }

    private void Start()
    {
        // 最初に説明開始
        StartExplanation();
    }

    public void StartExplanation()
    {
        Time.timeScale = 0f;
        isDuringExplanation = true;

        currentPage = 0;
        explanationUI.SetActive(true);
        daySelectPanel.SetActive(false);
        UpdatePage();

        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(NextPage);

        // 日数ボタン設定
        day5Button.onClick.AddListener(() => SelectDayCount(5));
        day10Button.onClick.AddListener(() => SelectDayCount(10));
        day30Button.onClick.AddListener(() => SelectDayCount(30));
        noLimitButton.onClick.AddListener(() => SelectDayCount(0)); // 0なら制限なし
    }

    private void UpdatePage()
    {
        if (currentPage >= explanationPages.Length)
        {
            // 最後まで来たら日数選択パネルを表示
            daySelectPanel.SetActive(true);
            nextButton.gameObject.SetActive(false);
        }
        else
        {
            explanationImage.sprite = explanationPages[currentPage];
        }
    }

    public void NextPage()
    {
        currentPage++;
        UpdatePage();
    }

    private void SelectDayCount(int days)
    {
        Debug.Log($"🎮 選択された日数: {days}");

        // ここでゲームの最大プレイ日設定
        Parameter.limitDay = days + 1; //最終日過ぎたら終了
        CloseExplanation();
    }

    public void CloseExplanation()
    {
        explanationUI.SetActive(false);
        Time.timeScale = 1f;
        isDuringExplanation = false;
    }

    public void OpenExplanationAgain()
    {
        // ゲーム中にヘルプ開く場合
        StartExplanation();
        //他スクリプトから呼び出す例：GameExplanationManager.Instance.OpenExplanationAgain();
    }

    public bool IsDuringExplanation()
    {
        return isDuringExplanation;
    }
}
