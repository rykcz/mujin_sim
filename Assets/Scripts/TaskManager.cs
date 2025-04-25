using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance { get; private set; }

    private List<WorkerController> workers = new List<WorkerController>();

    private Queue<TaskData> taskQueue = new Queue<TaskData>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void RegisterWorker(WorkerController worker)
    {
        workers.Add(worker);
    }

    // TileSelectorから呼ばれる
    public void EnqueueTask(TaskData task)
    {
        taskQueue.Enqueue(task);
    }

    // Workerが使う
    public TaskData GetNextTask()
    {
        if (taskQueue.Count > 0)
            return taskQueue.Dequeue();
        else
            return null;
    }

    public bool AssignTask(TaskData task)
    {
        Debug.Log($"TaskManager: タスク登録開始、タスクタイプ: {task.taskType} ターゲットセル: {task.targetCell}");

        foreach (var worker in workers)
        {
            Debug.Log($"TaskManager: Worker {worker.name} を確認中");

            bool available = worker.IsAvailable();

            // Move中だけ他タスク受け付ける
            bool moveInProgress = worker.IsMoving();

            Debug.Log($"TaskManager: Worker {worker.name} isAvailable={available}, isMoving={moveInProgress}");

            if (available || moveInProgress)
            {
                Debug.Log($"TaskManager: Worker {worker.name} にタスク割り当て");
                worker.SetTask(task);
                return true;
            }
        }

        Debug.LogWarning("TaskManager: 作業可能なWorkerがいない");
        return false;
    }

    public void ForceAllWorkersGoHome()
    {
        Debug.Log("ForceAllWorkersGoHome: Woker帰宅");

        foreach (var worker in workers)
        {
            if (worker != null)
            {
                worker.ResetTask();

                // GoHomeタスク割り当て
                Vector3Int homeCell = new Vector3Int(25, 25, 0);
                worker.SetTask(new TaskData(TaskType.GoHome, homeCell), force: true);
            }
        }
    }

}
