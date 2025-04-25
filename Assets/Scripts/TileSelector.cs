using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public enum CommandMode
{
    None,
    Till,
    PlantCabbage,
    PlantTomato,
    Harvest,
    Move
}

public class TileSelector : MonoBehaviour
{
    public static TileSelector Instance { get; private set; }

    private Camera mainCamera;
    private CommandMode currentMode = CommandMode.None;

    // 追従アイコン関連
    public Image mouseIcon;
    public Sprite tillSprite;
    public Sprite plantCabbageSprite;
    public Sprite plantTomatoSprite;
    public Sprite harvestSprite;
    public Sprite moveSprite;

    public Sprite errorSprite;
    public float errorDisplayTime = 0.8f;
    private Coroutine errorCoroutine;

    //メッセージ表示関連
    public GameObject stillWorkingMessage;
    public GameObject cantDoMessage;
    public GameObject emptySeedMessage;
    public GameObject noSpaceMessage;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        mainCamera = Camera.main;
        if (mouseIcon != null)
        {
            mouseIcon.gameObject.SetActive(false);
        }

        stillWorkingMessage.SetActive(false);
        cantDoMessage.SetActive(false);
        emptySeedMessage.SetActive(false);
        noSpaceMessage.SetActive(false);

    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log("UI上なのでタイルクリック無効");
                return;
            }

            Vector3 mouseScreenPos = Input.mousePosition;
            mouseScreenPos.z = 10f;

            if (mainCamera == null)
            {
                Debug.LogError("mainCameraがnull");
                return;
            }

            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
            Vector3Int cellPos = TilemapReference.Instance.tilemap.WorldToCell(mouseWorldPos);

            if (TilemapReference.Instance.tilemap.GetTile(cellPos) == null)
            {
                Debug.LogWarning("タイルがない場所をクリック");
                // SetMouseIcon(null);
                return;
            }

            Debug.Log($"クリック座標: {cellPos}");

            ExecuteCommand(cellPos);
        }

        if (mouseIcon != null && mouseIcon.gameObject.activeSelf)
        {
            mouseIcon.transform.position = Input.mousePosition;
        }
    }

    public void SetCommandMode(int mode)
    {
        currentMode = (CommandMode)mode;

        // ハイライトもONにする
        TileHighlighter highlighter = FindObjectOfType<TileHighlighter>();
        if (highlighter != null)
        {
            highlighter.SetActive(true);
        }

        // マウス追従アイコンを切り替え
        switch (currentMode)
        {
            case CommandMode.Till:
                SetMouseIcon(tillSprite);
                break;
            case CommandMode.PlantCabbage:
                SetMouseIcon(plantCabbageSprite);
                break;
            case CommandMode.PlantTomato:
                SetMouseIcon(plantTomatoSprite);
                break;
            case CommandMode.Harvest:
                SetMouseIcon(harvestSprite);
                break;
            case CommandMode.Move:
                SetMouseIcon(moveSprite);
                break;
            case CommandMode.None:
                SetMouseIcon(null);
                break;
        }
    }

    private bool IsInsideValidArea(Vector3Int cell)
    {
        return cell.x >= 0 && cell.x < 26 && cell.y >= 0 && cell.y < 26;
    }

    private void SetMouseIcon(Sprite sprite)
    {
        if (mouseIcon == null) return;

        if (sprite != null)
        {
            mouseIcon.sprite = sprite;
            mouseIcon.gameObject.SetActive(true);
        }
        else
        {
            mouseIcon.gameObject.SetActive(false);

            // アイコン消したらハイライトもOFF
            TileHighlighter highlighter = FindObjectOfType<TileHighlighter>();
            if (highlighter != null)
            {
                highlighter.SetActive(false);
            }
        }
    }

    private bool CanPlantAt(Vector3Int cell)
    {
        if (OccupiedMapManager.Instance.IsCellOccupied(cell))
            return false; // 占有セル（池含む）には植えない

        return !VegetableMapManager.Instance.HasVegetable(cell);
    }

    private bool IsSoilTile(Vector3Int cell)
    {
        TileBase tile = TilemapReference.Instance.tilemap.GetTile(cell);
        if (tile == null) return false;

        return tile.name == "mapchip_01_7"; // 土タイル
    }

    private bool IsWastelandTile(Vector3Int cell)
    {
        TileBase tile = TilemapReference.Instance.tilemap.GetTile(cell);
        if (tile == null) return false;

        return tile.name == "mapchip_01_1"; // 荒地タイル
    }

    private void ExecuteCommand(Vector3Int cell)
    {

        TileBase clickedTile = TilemapReference.Instance.tilemap.GetTile(cell);
        if (clickedTile != null)
        {
            Debug.Log($"クリックしたタイルの名前: {clickedTile.name}");
        }

        if (!IsInsideValidArea(cell))
        {
            Debug.LogWarning("範囲外のタイル");
            ShowErrorIcon();
            StartCoroutine(ShowMessageController.Instance.ShowMessage(cantDoMessage, 1.3f));
            return;
        }

        if (OccupiedMapManager.Instance.IsPondCell(cell))
        {
            Debug.LogWarning("池タイルなので作業不可");
            ShowErrorIcon();
            StartCoroutine(ShowMessageController.Instance.ShowMessage(cantDoMessage, 1.3f));
            return;
        }

        bool taskAssigned = false;

        switch (currentMode)
        {
            case CommandMode.Till:
                if (!IsWastelandTile(cell)) //荒地タイル以外では実行不可
                {
                    ShowErrorIcon();
                    return;
                }
                taskAssigned = TaskManager.Instance.AssignTask(new TaskData(TaskType.Till, cell));
                break;

            case CommandMode.PlantCabbage:
                if (!IsSoilTile(cell))
                {
                    Debug.LogWarning("土の上でないので種まき不可");
                    ShowErrorIcon();
                    StartCoroutine(ShowMessageController.Instance.ShowMessage(cantDoMessage, 1.3f));
                    break;
                }
                if (!CanPlantAt(cell))
                {
                    Debug.LogWarning("すでに野菜が存在");
                    ShowErrorIcon();
                    StartCoroutine(ShowMessageController.Instance.ShowMessage(cantDoMessage, 1.3f));
                    break;
                }
                if (SeedInventory.Instance.cabbageSeedCount > 0)  // ここで数だけ確認
                {
                    taskAssigned = TaskManager.Instance.AssignTask(new TaskData(TaskType.PlantCabbage, cell));
                }
                else
                {
                    Debug.Log("キャベツの種が不足");
                    ShowErrorIcon();
                    StartCoroutine(ShowMessageController.Instance.ShowMessage(emptySeedMessage, 1.3f));
                }
                break;

            case CommandMode.PlantTomato:
                if (!IsSoilTile(cell))
                {
                    Debug.LogWarning("土の上でないので種まき不可");
                    ShowErrorIcon();
                    StartCoroutine(ShowMessageController.Instance.ShowMessage(cantDoMessage, 1.3f));
                    break;
                }
                if (!CanPlantAt(cell))
                {
                    Debug.LogWarning("すでに野菜が存在");
                    ShowErrorIcon();
                    StartCoroutine(ShowMessageController.Instance.ShowMessage(cantDoMessage, 1.3f));
                    break;
                }
                if (SeedInventory.Instance.tomatoSeedCount > 0)
                {
                    taskAssigned = TaskManager.Instance.AssignTask(new TaskData(TaskType.PlantTomato, cell));
                }
                else
                {
                    Debug.Log("トマトの種が不足");
                    ShowErrorIcon();
                    StartCoroutine(ShowMessageController.Instance.ShowMessage(emptySeedMessage, 1.3f));
                }
                break;

            case CommandMode.Harvest:
            
                // 野菜が存在しない又は成長してないなら収穫不可
                VegetableGrowth veg = VegetableMapManager.Instance.GetVegetable(cell);
                if (veg == null || !veg.IsFullyGrown())
                {
                    Debug.LogWarning("成長した野菜がないので収穫不可");
                    ShowErrorIcon();
                    StartCoroutine(ShowMessageController.Instance.ShowMessage(cantDoMessage, 1.3f));
                    break;
                }

                // ★ 販売所に空きがないなら収穫不可
                if (MarketManager.Instance.GetFreeStallCount() <= 0)
                {
                    Debug.LogWarning("販売所に空きがないので収穫不可");
                    ShowErrorIcon();
                    StartCoroutine(ShowMessageController.Instance.ShowMessage(noSpaceMessage, 1.3f));
                    break;
                }

                if (!VegetableMapManager.Instance.HasVegetable(cell))
                {
                    Debug.LogWarning("収穫できる野菜が存在しない");
                    ShowErrorIcon();
                    StartCoroutine(ShowMessageController.Instance.ShowMessage(cantDoMessage, 1.3f));
                    return; // ★ここでreturnしてタスク登録を防ぐ！
                }

                Debug.Log($"Harvestタスクを登録、ターゲットセル: {cell}");
                taskAssigned = TaskManager.Instance.AssignTask(new TaskData(TaskType.Harvest, cell));
                break;
                
            case CommandMode.Move:
                Debug.Log($"Moveタスクを登録、ターゲットセル: {cell}");
                taskAssigned = TaskManager.Instance.AssignTask(new TaskData(TaskType.Move, cell));
                break;

        }

        if (!taskAssigned && currentMode != CommandMode.None)
        {
            Debug.LogWarning("作業可能なバイトが存在しない");
            ShowErrorIcon();
            StartCoroutine(ShowMessageController.Instance.ShowMessage(stillWorkingMessage, 1.3f));
        }

        // SetMouseIcon(null); // どんな場合でもクリック後にアイコンを消す
        // currentMode = CommandMode.None;
    }

    public void ShowErrorIcon()
    {
        if (errorCoroutine != null)
        {
            StopCoroutine(errorCoroutine); // 前のエラー表示を止める
        }
        errorCoroutine = StartCoroutine(ShowErrorIconCoroutine());
    }

    private IEnumerator ShowErrorIconCoroutine()
    {
        if (mouseIcon == null) yield break;

        AudioController.Instance.PlaySE("NG", 0.2f);
        mouseIcon.sprite = errorSprite;
        mouseIcon.gameObject.SetActive(true);

        yield return new WaitForSeconds(errorDisplayTime);

        // エラー表示が終わったら今のcurrentModeに応じて再設定
        SetMouseIcon(GetSpriteForCurrentMode());
    }

    // タスクモードに応じたスプライトを返す
    private Sprite GetSpriteForCurrentMode()
    {
        switch (currentMode)
        {
            case CommandMode.Till: return tillSprite;
            case CommandMode.PlantCabbage: return plantCabbageSprite;
            case CommandMode.PlantTomato: return plantTomatoSprite;
            case CommandMode.Harvest: return harvestSprite;
            case CommandMode.Move: return moveSprite;
            case CommandMode.None: return null;
            default: return null;
        }
    }

}
