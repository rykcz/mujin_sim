using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class WorkerController : MonoBehaviour
{
    private TaskData currentTask;
    private bool isWorking = false;
    private bool isExecutingTask = false;

    private Vector3Int moveTargetCell;   // 実際に歩くゴール
    private Vector3Int actionTargetCell; // タスク実行する対象

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    public GameObject cabbagePrefab;
    public GameObject tomatoPrefab;

    //タスクに応じた作業待機時間
    public float tillTime = 3f;
    public float cropTime = 1.5f;
    public float harvestTime = 2f;
    private float waitTime;

    // タスク進行バー関連
    public GameObject taskProgressBarObject; // インスペクターからセット
    public GameObject taskProgressBarObjectBack;
    private Transform taskProgressBarFill;   // 進捗バーの中身
    private float taskProgress = 0f;          // タスク進行度 (0〜1)
    private bool isProgressActive = false;    // バー表示中か

    private Queue<Vector3Int> pathQueue = new Queue<Vector3Int>();

    // 現タスクアイコン用
    public SpriteRenderer taskIconRenderer; // インスペクターで設定
    public Sprite hoeIcon;
    public Sprite cabbageSeedIcon;
    public Sprite tomatoSeedIcon;
    public Sprite harvestIcon;
    public Sprite moveIcon;
    public Sprite homeIcon;

    //トマト複数収穫用
    public GameObject plus2IconPrefab;
    public GameObject plus3IconPrefab;
    private GameObject activePlusIcon;

    private void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
        TaskManager.Instance.RegisterWorker(this);

        if (taskProgressBarObject != null)
        {
            taskProgressBarFill = taskProgressBarObject.transform.Find("Fill");

        if (taskProgressBarFill == null)
        {
            Debug.LogError("❌ taskProgressBarFill が見つかりません！");
        }
        else
        {
            Debug.Log($"✅ taskProgressBarFill 発見: {taskProgressBarFill.name}");
        }

            taskProgressBarObject.SetActive(false); // 最初は非表示にしておく
            taskProgressBarObjectBack.SetActive(false);
        }        
    }

    public bool IsAvailable()
    {
        return !isWorking;
    }

    public void SetTask(TaskData task, bool force = false)
    {
        if (!IsInsideValidArea(task.targetCell))
        {
            Debug.LogWarning($"⚠️ Worker: 範囲外タスクなので拒否: {task.targetCell}");
            currentTask = null;
            isWorking = false;
            return;
        }

        Debug.Log("🌾 SetTask 呼び出し");

        if (isWorking)
        {
            if (currentTask != null && currentTask.taskType == TaskType.Move)
            {
                // ⭐ Moveタスク中なら無条件で新しいタスクに切り替え
                Debug.Log("🏃 Move中なので新しいタスクに強制切り替えます！");
                StopAllCoroutines();
                pathQueue.Clear();
                isExecutingTask = false;
                currentTask = null;
                isWorking = false;
            }
            else if (force)
            {
                // ⭐ force==trueなら作業中でも強制キャンセル
                Debug.Log("⚡ 強制タスク割り込み受け付けます！");
                StopAllCoroutines();
                pathQueue.Clear();
                isExecutingTask = false;
                currentTask = null;
                isWorking = false;
            }
            else
            {
                // ⭐ 通常作業中なら拒否
                Debug.LogWarning($"{gameObject.name} はまだ作業中なので、新しいタスクは受け付けません！");
                return;
            }
        }

        // ★ここから普通の新タスク受け付け
        Debug.Log("🌾 SetTask: タスク代入します");
        currentTask = task;
        actionTargetCell = task.targetCell;

        if (task.taskType == TaskType.Harvest ||
            task.taskType == TaskType.PlantCabbage ||
            task.taskType == TaskType.PlantTomato ||
            task.taskType == TaskType.Till)
        {
            Vector3Int adjacentCell = FindAdjacentEmptyCellNearTarget(actionTargetCell);

            if (adjacentCell.x == int.MinValue)
            {
                Debug.LogError("🌾 隣接マスが見つからなかったのでタスク受理せず終了");
                currentTask = null;
                return;
            }

            moveTargetCell = adjacentCell;
        }
        else
        {
            moveTargetCell = actionTargetCell;
        }

        isWorking = true;

        Debug.Log("🌾 SetTask: FindPathToTargetを呼びます");
        FindPathToTarget();
        Debug.Log("🌾 SetTask: 終了");
        
        // ⭐ Moveタスクだけ例外的にアイコンもバーも非表示にする
        if (currentTask.taskType == TaskType.Move)
        {
            // ⭐ ここで進捗リセットとバー非表示
            taskProgress = 0f;
            isProgressActive = false;

            if (taskProgressBarObject != null)
                taskProgressBarObject.SetActive(false);
            if (taskProgressBarObjectBack != null)
                taskProgressBarObjectBack.SetActive(false);
            if (taskIconRenderer != null)
                taskIconRenderer.gameObject.SetActive(false);
        }
        else
        {
            UpdateTaskIcon(currentTask.taskType); // ←通常はタスクアイコン更新
        }

    }

    private Vector3Int FindAdjacentEmptyCellNearTarget(Vector3Int targetCell)
    {
        Vector3Int[] directions = {
            new Vector3Int(1, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, -1, 0)
        };

        Vector3Int startCell = TilemapReference.Instance.tilemap.WorldToCell(transform.position);

        Vector3Int bestCell = Vector3Int.one * int.MinValue;
        int bestDistance = int.MaxValue;
        bool found = false;

        foreach (var dir in directions)
        {
            Vector3Int adjacent = targetCell + dir;

            // 1. 範囲外チェック
            if (!IsInsideValidArea(adjacent)) continue;

            // 2. 占有チェック
            if (OccupiedMapManager.Instance.IsCellOccupied(adjacent)) continue;

            // 3. パスが通るかチェック
            var testPath = SimpleAStar.FindPath(startCell, adjacent, allowOccupied: false, allowOccupiedGoal: true);

            if (testPath == null)
            {
                continue; // パスが通らないならこの隣接マスは無視
            }

            int distance = testPath.Count;

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestCell = adjacent;
                found = true;
            }
        }

        if (found)
        {
            Debug.Log($"🌾 最短隣接セルを選択: {bestCell} (パス長さ {bestDistance})");
            return bestCell;
        }
        else
        {
            Debug.LogError("🌾 隣接に到達可能な空きマスがないのでタスクキャンセルします！");
            return Vector3Int.one * int.MinValue; // 無効な座標
        }
    }

    private void Update()
    {
        if (isProgressActive)
        {
            UpdateTaskProgressBar();
        }

        if (!isWorking || currentTask == null)
        {
            isWorking = false;
            return;
        }

        if (pathQueue.Count > 0)
        {
            MoveAlongPath();
        }
        else if (!isExecutingTask)
        {
            LookAtTargetCell();

            // ⭐ ここ！！ Moveタスクなら何もしない
            if (currentTask.taskType == TaskType.Move)
            {
                // 移動タスクなら ExecuteTaskしない
                Debug.Log("🚶 Moveタスクなので作業実行しません。");
                currentTask = null;
                isWorking = false;
                isExecutingTask = false;
                ClearTaskIcon();
                if (taskProgressBarObject != null) taskProgressBarObject.SetActive(false);
                if (taskProgressBarObjectBack != null) taskProgressBarObjectBack.SetActive(false);
                isProgressActive = false;
            }
            else
            {
                ExecuteTask(); // 🌟移動以外なら作業実行
            }
        }
    }

    private void LookAtTargetCell()
    {
        Vector3 myPos = transform.position;
        Vector3 targetPos = TilemapReference.Instance.tilemap.GetCellCenterWorld(actionTargetCell);
        Vector3 dir = (targetPos - myPos).normalized;

        if (spriteRenderer != null && animator != null)
        {
            if (dir.y > 0)
            {
                animator.Play("Worker_LeftUp");
                spriteRenderer.flipX = dir.x > 0;
            }
            else
            {
                animator.Play("Worker_LeftDown");
                spriteRenderer.flipX = dir.x > 0;
            }
        }
    }

    private void LateUpdate()
    {
        if (spriteRenderer != null)
        {
            int baseSortingOrder = 1500;
            float visualY = spriteRenderer.transform.position.y;
            spriteRenderer.sortingOrder = baseSortingOrder - (int)(visualY * 100);
        }
    }

    private bool IsInsideValidArea(Vector3Int cell) //移動範囲の制限
    {
        return cell.x >= 0 && cell.x < 26 && cell.y >= 0 && cell.y < 26;
    }

    private void FindPathToTarget()
    {
        Vector3Int startCell = TilemapReference.Instance.tilemap.WorldToCell(transform.position);
        Vector3Int goalCell = moveTargetCell;
        bool allowOccupiedGoal = true;

        List<Vector3Int> removedVegetables = new List<Vector3Int>();

        List<Vector3Int> path = SimpleAStar.FindPath(startCell, goalCell, allowOccupied: false, allowOccupiedGoal: allowOccupiedGoal);

        if (path == null)
        {
            Debug.Log("❌ 最初のパス失敗 → リカバリモード突入");
        }
        else
        {
            Debug.Log("✅ 最初のパス成功 → リカバリ不要");
        }

        // ✅ 最初パス見つからなかったらリカバリ開始
        if (path == null)
        {
            Debug.LogWarning("⚠️ パス見つからないので邪魔な野菜を削除しながらリトライします");

            // 🥬 全野菜リストを取得
            List<Vector3Int> allVegetables = VegetableMapManager.Instance.GetAllVegetablePositions();

            // 距離が近い順にソート（より邪魔そうなものを優先）
            allVegetables.Sort((a, b) => Vector3Int.Distance(a, startCell).CompareTo(Vector3Int.Distance(b, startCell)));

            foreach (var vegCell in allVegetables)
            {
                // 1個壊す
                VegetableGrowth vegetable = VegetableMapManager.Instance.GetVegetable(vegCell);
                if (vegetable != null)
                {
                    Debug.Log($"🥬 邪魔な野菜を破壊します: {vegCell}");
                    vegetable.MarkForDestroy();
                    Destroy(vegetable.gameObject);

                    VegetableMapManager.Instance.UnregisterVegetable(vegCell);
                    OccupiedMapManager.Instance.UnregisterCell(vegCell);
                    AudioController.Instance.PlaySE("野菜破壊", 0.2f);
                    Debug.Log("野菜破壊SE");
                    removedVegetables.Add(vegCell);
                }

                // 再トライ
                path = SimpleAStar.FindPath(startCell, goalCell, allowOccupied: false, allowOccupiedGoal: allowOccupiedGoal);

                if (path != null)
                {
                    Debug.Log($"✅ 邪魔をどけたらパス発見！除去した野菜数: {removedVegetables.Count}");
                    break;
                }
            }
        }

        if (path != null && path.Count >= 1)
        {
            pathQueue = new Queue<Vector3Int>(path);
            Debug.Log($"🚶 パス見つかった！セル数: {path.Count}");
        }
        else
        {
            Debug.LogError("❌ どけてもパス見つかりませんでした…");
            currentTask = null;
            isWorking = false;
        }
    }

    private void MoveAlongPath()
    {
        if (pathQueue.Count == 0) return;

        Vector3Int nextCell = pathQueue.Peek();
        Vector3 targetPos = TilemapReference.Instance.tilemap.GetCellCenterWorld(nextCell);
        Vector3 moveDir = (targetPos - transform.position).normalized;

        transform.position = Vector3.MoveTowards(transform.position, targetPos, 2f * Time.deltaTime);
        UpdateFacingDirection(moveDir);

        if (Vector3.Distance(transform.position, targetPos) < 0.05f)
        {
            transform.position = targetPos;
            pathQueue.Dequeue();

            if (pathQueue.Count == 0) // 🏁 ゴールに着いたとき
            {
                Vector3Int myCell = TilemapReference.Instance.tilemap.WorldToCell(transform.position);

                if (VegetableMapManager.Instance.HasVegetable(myCell))
                {
                    Debug.Log($"🥬 ゴール地点 {myCell} に野菜がいたので破壊します");

                    VegetableGrowth veg = VegetableMapManager.Instance.GetVegetable(myCell);
                    if (veg != null)
                    {
                        veg.MarkForDestroy();
                        Destroy(veg.gameObject);

                        VegetableMapManager.Instance.UnregisterVegetable(myCell);
                        OccupiedMapManager.Instance.UnregisterCell(myCell);

                        AudioController.Instance.PlaySE("野菜破壊", 0.2f); // 🔥 SEも鳴らす
                    }
                }
            }
        }
    }

    private void UpdateFacingDirection(Vector3 dir)
    {
        if (spriteRenderer == null || animator == null) return;

        if (dir.y > 0)
        {
            animator.Play("Worker_LeftUp");
            spriteRenderer.flipX = dir.x > 0;
        }
        else
        {
            animator.Play("Worker_LeftDown");
            spriteRenderer.flipX = dir.x > 0;
        }
    }

    private void ExecuteTask()
    {
        if (currentTask == null) return;

        if (!isExecutingTask)
        {
            StartCoroutine(ExecuteTaskRoutine());
            isExecutingTask = true;
        }
    }

    private IEnumerator ExecuteTaskRoutine()
    {
        if (currentTask == null)
        {
            isExecutingTask = false;
            yield break;
        }

        Debug.Log($"🔍 ExecuteTaskRoutineスタート タイプ: {currentTask.taskType}");

        // まずタスクに応じてSE鳴らす
        switch (currentTask.taskType)
        {
            case TaskType.Till:
                AudioController.Instance.PlaySE("耕す", 0.2f);
                waitTime = tillTime;
                break;
            case TaskType.PlantCabbage:
            case TaskType.PlantTomato:
                AudioController.Instance.PlaySE("種まき", 0.2f);
                waitTime = cropTime;
                break;
            case TaskType.Harvest:
                AudioController.Instance.PlaySE("収穫", 0.4f);
                waitTime = harvestTime;
                break;
        }

        taskProgress = 0f;

        // 🛠 ここで分岐！GoHomeなら進捗バー出さない
        if (currentTask.taskType != TaskType.GoHome)
        {
            isProgressActive = true;

            if (taskProgressBarObject != null)
                taskProgressBarObject.SetActive(true);
            if (taskProgressBarObjectBack != null)
                taskProgressBarObjectBack.SetActive(true);

            // Fillをゼロにする
            if (taskProgressBarFill != null)
            {
                taskProgressBarFill.localScale = new Vector3(0.01f, 1f, 1f);
                taskProgressBarFill.localPosition = new Vector3(-0.35f, -0.5f, 0f);
            }
        }
        else
        {
            // GoHomeのときは進捗バー出さない
            isProgressActive = false;

            if (taskProgressBarObject != null)
                taskProgressBarObject.SetActive(false);
            if (taskProgressBarObjectBack != null)
                taskProgressBarObjectBack.SetActive(false);
        }

        float elapsed = 0f;

        while (elapsed < waitTime)
        {
            elapsed += Time.deltaTime;
            taskProgress = Mathf.Clamp01(elapsed / waitTime);  // ★ここで更新
            yield return null;
        }

        // 進捗完了後にバー非表示（GoHomeだった場合はもう非表示になってるからOK）
        if (taskProgressBarObject != null)
            taskProgressBarObject.SetActive(false);
        if (taskProgressBarObjectBack != null)
            taskProgressBarObjectBack.SetActive(false);

        isProgressActive = false;

        switch (currentTask.taskType)
        {
            case TaskType.Till:
                TillTile(actionTargetCell);
                break;
            case TaskType.PlantCabbage:
                PlantCrop(actionTargetCell, "Cabbage");
                break;
            case TaskType.PlantTomato:
                PlantCrop(actionTargetCell, "Tomato");
                break;
            case TaskType.Harvest:
                HarvestCrop(actionTargetCell);
                break;
            case TaskType.Move:
                Debug.Log("🚶 Moveタスク完了");

                // ⭐ ここで進捗リセット・タスク完了扱いにする！
                taskProgress = 0f;
                isProgressActive = false;

                if (taskProgressBarObject != null)
                    taskProgressBarObject.SetActive(false);
                if (taskProgressBarObjectBack != null)
                    taskProgressBarObjectBack.SetActive(false);

                ClearTaskIcon(); // アイコンも消す

                // ⭐ タスク終了処理！
                currentTask = null;
                isWorking = false;
                isExecutingTask = false;

                yield break; // 終了！

            case TaskType.GoHome:
                FadeOutAndDisable();
                yield break;
        }

        currentTask = null;
        isWorking = false;
        isExecutingTask = false;

        ClearTaskIcon();
    }

    private void UpdateTaskProgressBar()
    {
        if (taskProgressBarFill == null) return;

        float fill = Mathf.Clamp01(taskProgress);  // ここで必ず Clamp01

        // スケール更新
        taskProgressBarFill.localScale = new Vector3(Mathf.Max(fill, 0.01f), 1f, 1f);

        // 位置は固定！左端に揃えたまま
        taskProgressBarFill.localPosition = new Vector3(-0.35f, 0f, 0f);
    }

    private void TillTile(Vector3Int cell)
    {
        var tilemap = TilemapReference.Instance.tilemap;
        if (tilemap.GetTile(cell) != null)
        {
            tilemap.SetTile(cell, TilemapReference.Instance.soilTile);

            //耕したタイルのリミット登録
            WastelandRecoveryManager.Instance.RegisterTilledCell(cell);
        }
    }

    private void PlantCrop(Vector3Int cell, string vegetableType)
    {
        Vector3 worldPos = TilemapReference.Instance.tilemap.CellToWorld(cell);
        worldPos += new Vector3(0f, 0.25f, 0f);

        GameObject prefabToUse = null;
        VegetableType vegType = VegetableType.Cabbage;

        if (vegetableType == "Cabbage")
        {
            prefabToUse = cabbagePrefab;
            vegType = VegetableType.Cabbage;

            if (!SeedInventory.Instance.UseCabbageSeed())
            {
                Debug.LogWarning("⚠️ キャベツの種が足りないのでPlant中止！");
                return;
            }
        }
        else if (vegetableType == "Tomato")
        {
            prefabToUse = tomatoPrefab;
            vegType = VegetableType.Tomato;

            if (!SeedInventory.Instance.UseTomatoSeed())
            {
                Debug.LogWarning("⚠️ トマトの種が足りないのでPlant中止！");
                return;
            }
        }

        if (prefabToUse == null)
        {
            Debug.LogError($"❌ PlantCrop失敗: vegetableType={vegetableType}");
            return;
        }

        GameObject crop = Instantiate(prefabToUse, worldPos, Quaternion.identity);
        crop.transform.parent = GameObject.Find("Crops").transform;

        VegetableGrowth vg = crop.GetComponent<VegetableGrowth>();
        if (vg == null)
        {
            Debug.LogError("❌ 野菜プレハブに VegetableGrowth が付いてない！");
            return;
        }

        vg.vegetableType = vegType;

        // 🌱 植えた野菜をVegetableMapManagerに登録する！
        Vector3Int cellPos = TilemapReference.Instance.tilemap.WorldToCell(worldPos);
        VegetableMapManager.Instance.RegisterVegetable(cellPos, vg);
    }

    private void HarvestCrop(Vector3Int cell)
    {
        Debug.Log($"🌾 HarvestCrop 実行 cell: {cell}");

        // マップから野菜情報を取得
        if (!VegetableMapManager.Instance.HasVegetable(cell))
        {
            Debug.LogWarning("⚠️ 収穫対象がいません！");
            currentTask = null;
            isWorking = false;
            isExecutingTask = false;
            return;
        }

        VegetableGrowth vegetable = VegetableMapManager.Instance.GetVegetable(cell);

        if (vegetable == null)
        {
            Debug.LogWarning("⚠️ マップには登録されているが、VegetableGrowthコンポーネントが無い！");
            currentTask = null;
            isWorking = false;
            isExecutingTask = false;
            return;
        }

        // 成長しているか確認
        if (!vegetable.IsFullyGrown())
        {
            Debug.LogWarning("⚠️ 成長していないので収穫できません！");
            currentTask = null;
            isWorking = false;
            isExecutingTask = false;
            return;
        }

        // --- ここから収穫処理 ---
        int freeSlots = MarketManager.Instance.GetFreeStallCount();

        if (freeSlots <= 0)
        {
            Debug.LogWarning("⚠️ 販売所に空きがないので収穫中止します！");
            currentTask = null;
            isWorking = false;
            isExecutingTask = false;
            return;
        }

        if (vegetable.vegetableType == VegetableType.Tomato)
        {
            float randomValue = Random.value;
            int amount = 1;

            if (randomValue < 0.2f) //20%
            {
                amount = 3;
            }
            else if (randomValue < 0.55f) //35%
            {
                amount = 2;
            }

            // 空きがない分は捨てる
            int actualHarvestAmount = Mathf.Min(amount, freeSlots);

            Debug.Log($"🍅 トマトを {actualHarvestAmount} 個収穫します！（本来{amount}個）");

            for (int i = 0; i < actualHarvestAmount; i++)
            {
                MarketManager.Instance.AddItemToMarket(vegetable);
            }

            // 余剰分があった場合は警告
            if (actualHarvestAmount < amount)
            {
                Debug.LogWarning($"⚠️ {amount - actualHarvestAmount} 個のトマトは販売所に置けずに廃棄されました。");
            }

            // アイコン表示も actualHarvestAmount を基準にする
            if (actualHarvestAmount == 2)
            {
                ShowPlusIcon(plus2IconPrefab);
            }
            else if (actualHarvestAmount == 3)
            {
                ShowPlusIcon(plus3IconPrefab);
            }
        }
        else
        {
            // 🥬 キャベツなら1個
            MarketManager.Instance.AddItemToMarket(vegetable);
        }

        // ★ 野菜削除
        vegetable.MarkForDestroy();
        VegetableMapManager.Instance.UnregisterVegetable(cell);
        OccupiedMapManager.Instance.UnregisterCell(cell);

        Destroy(vegetable.gameObject);

        Debug.Log($"✅ 収穫成功！cell: {cell}");

        // タスク完了
        currentTask = null;
        isWorking = false;
        isExecutingTask = false;
    }

    private IEnumerator DestroyVegetablesNextFrame(List<VegetableGrowth> vegetables)
    {
        yield return null;

        foreach (var vegetable in vegetables)
        {
            if (vegetable != null)
            {
                Destroy(vegetable.gameObject);
            }
        }
    }

    public bool IsMoving()
    {
        return currentTask != null && currentTask.taskType == TaskType.Move;
    }

    public void ResetTask()
    {
        Debug.Log($"🔄 {gameObject.name}: タスクをリセットします！");
        currentTask = null;
        isWorking = false;
        isExecutingTask = false;
        pathQueue.Clear();
        StopAllCoroutines();

        ClearTaskIcon();
    }

    private void FadeOutAndDisable()
    {
        var fade = GetComponent<WorkerFadeController>();
        if (fade != null)
        {
            fade.FadeOutAndDisable();
        }
        else
        {
            Debug.LogWarning("⚠️ WorkerにFadeコンポーネントが付いていません！");
            gameObject.SetActive(false);
        }
    }

    private void UpdateTaskIcon(TaskType taskType)
    {
        if (taskIconRenderer == null) return;

        switch (taskType)
        {
            case TaskType.Till:
                taskIconRenderer.sprite = hoeIcon;
                break;
            case TaskType.PlantCabbage:
                taskIconRenderer.sprite = cabbageSeedIcon;
                break;
            case TaskType.PlantTomato:
                taskIconRenderer.sprite = tomatoSeedIcon;
                break;
            case TaskType.Harvest:
                taskIconRenderer.sprite = harvestIcon;
                break;
            case TaskType.Move:
                taskIconRenderer.sprite = moveIcon;
                break;
            case TaskType.GoHome:
                // GoHomeのときはアイコン非表示
                taskIconRenderer.sprite = null;
                taskIconRenderer.gameObject.SetActive(false);
                return;
            default:
                taskIconRenderer.sprite = null;
                break;
        }

        taskIconRenderer.gameObject.SetActive(true);
    }

    private void ClearTaskIcon()
    {
        if (taskIconRenderer == null) return;

        taskIconRenderer.sprite = null;
        taskIconRenderer.gameObject.SetActive(false);
    }

    private void ShowPlusIcon(GameObject iconPrefab)
    {
        if (iconPrefab == null) return;

        if (activePlusIcon != null)
        {
            Destroy(activePlusIcon); // 既存のアイコン削除
        }

        activePlusIcon = Instantiate(iconPrefab, transform);
        activePlusIcon.transform.localPosition = new Vector3(0f, 1.2f, 0f); // 表示位置

        StartCoroutine(AnimateAndDestroyPlusIcon(activePlusIcon));
    }

    private IEnumerator AnimateAndDestroyPlusIcon(GameObject icon)
    {
        if (icon == null) yield break;

        float duration = 1.0f; // 上昇＆フェードアウト時間
        float elapsed = 0f;

        Vector3 startPos = new Vector3(0f, 1.2f, 0f);
        Vector3 endPos = new Vector3(0f, 1.5f, 0f);

        SpriteRenderer sr = icon.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogWarning("⚠️ PlusアイコンにSpriteRendererが付いてない！");
            yield break;
        }

        Color startColor = sr.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // 上昇
            icon.transform.localPosition = Vector3.Lerp(startPos, endPos, t);

            // フェードアウト
            sr.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(1f, 0f, t));

            yield return null;
        }

        Destroy(icon); // 最後に削除
    }

    private IEnumerator HidePlusIconAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        if (activePlusIcon != null)
        {
            Destroy(activePlusIcon);
        }
    }
}