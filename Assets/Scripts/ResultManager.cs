using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultManager : MonoBehaviour
{
    public static ResultManager Instance { get; private set; }

    public GameObject resultUI;        // リザルト画面全体
    public Text dayText;               // 経過日数表示
    public Text moneyText;             // 所持金表示
    public Text soldCountText;         // 累計販売数表示
    public Button retryButton;         // リトライボタン
    public GameObject gameOverImage;         // 維持費払えなかった時用

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        resultUI.SetActive(false);

        if (retryButton != null)
        {
            retryButton.onClick.AddListener(RetryGame);
        }
    }

    public void OpenResult()
    {
        Time.timeScale = 0f; // 時間停止

        resultUI.SetActive(true);

        if(Parameter.money < 0){
            gameOverImage.SetActive(true);
        }else{
            gameOverImage.SetActive(false);
        }

        dayText.text = $"{Parameter.day - 1}";
        moneyText.text = $"{Parameter.money.ToString("N0")}";
        soldCountText.text = $"{Parameter.soldVegetableCount}";
    }

    private void RetryGame()
    {
        Parameter.ResetParameters();//パラメータ初期化
        Time.timeScale = 1f; // シーンロード前に時間を戻す
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public bool IsResultOpen()
    {
        return resultUI != null && resultUI.activeSelf;
    }
}
