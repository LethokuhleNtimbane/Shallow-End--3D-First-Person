
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public enum TaskID
    {
        SpeakToThomasFirst,
        SearchWoodenBox,
        ReadJournal,
        GatherResources,
        CraftTools,
        FindFood,
        SurviveNight,
        CraftTent,
        BuildRaft,
        SpeakToThomasSecond
    }

    [System.Serializable]
    public class TaskData
    {
        public TaskID taskID;
        public string taskText;

        public TaskData(TaskID id, string text)
        {
            taskID = id;
            taskText = text;
        }
    }

    [Header("Task UI")]
    [SerializeField] private TextMeshProUGUI taskText1;
    [SerializeField] private TextMeshProUGUI taskText2;

    [Header("Task Colours")]
    [SerializeField] private Color activeTaskColour = Color.white;
    [SerializeField] private Color completedTaskColour = Color.green;

    [Header("Display")]
    [SerializeField] private float completedTaskDisplayTime = 2f;

    private TaskData[] tasks;
    private int currentTaskIndex;

    private bool showingCompletedTask;
    private Coroutine completedTaskCoroutine;

    private readonly HashSet<TaskID> completedTasks =
        new HashSet<TaskID>();

    private int woodCollected;
    private int vinesCollected;
    private int rocksCollected;

    private bool hammerCrafted;
    private bool spearCrafted;
    private bool coconutOpened;
    private bool crabCaught;
    private bool firstNightSurvived;
    private bool tentBuilt;
    private bool raftBuilt;

    private void Start()
    {
        CreateTasks();
        currentTaskIndex = 0;

        SkipAlreadyCompletedTasks();
        UpdateTaskUI();
        TryCompleteCurrentTask();
    }

    private void CreateTasks()
    {
        tasks = new TaskData[]
        {
            new TaskData(
                TaskID.SpeakToThomasFirst,
                "Go find Thomas Montgomery"
            ),
            new TaskData(
                TaskID.SearchWoodenBox,
                "Find Thomas's chest and retrieve the axe"
            ),
            new TaskData(
                TaskID.ReadJournal,
                "Read the journal"
            ),
            new TaskData(
                TaskID.SurviveNight,
                "Survive the night"
            ),
            new TaskData(
                TaskID.CraftTent,
                "Build a tent"
            ),
            new TaskData(
                TaskID.BuildRaft,
                "Build a raft"
            ),
            new TaskData(
                TaskID.SpeakToThomasSecond,
                "Say goodbye to Thomas"
            )
        };
    }

    public void CompleteTask(TaskID taskID)
    {
        completedTasks.Add(taskID);
        TryCompleteCurrentTask();
    }

    public void RegisterThomasFirstConversationFinished()
    {
        CompleteTask(TaskID.SpeakToThomasFirst);
    }

    public void RegisterAxeFound()
    {
        CompleteTask(TaskID.SearchWoodenBox);
    }

    public void RegisterJournalRead()
    {
        CompleteTask(TaskID.ReadJournal);
    }

    public void RegisterWoodCollected(int amount = 1)
    {
        woodCollected += Mathf.Max(0, amount);
    }

    public void RegisterVinesCollected(int amount = 1)
    {
        vinesCollected += Mathf.Max(0, amount);
    }

    public void RegisterRocksCollected(int amount = 1)
    {
        rocksCollected += Mathf.Max(0, amount);
    }

    public void RegisterItemCrafted(string itemName)
    {
        if (string.IsNullOrWhiteSpace(itemName))
            return;

        string item = itemName.Trim().ToLowerInvariant();

        if (item == "hammer")
            hammerCrafted = true;
        else if (item == "spear")
            spearCrafted = true;
        else if (item == "tent")
        {
            tentBuilt = true;
            completedTasks.Add(TaskID.CraftTent);
        }
        else if (item == "raft")
        {
            raftBuilt = true;
            completedTasks.Add(TaskID.BuildRaft);
        }

        UpdateTaskUI();
        TryCompleteCurrentTask();
    }

    public void RegisterCoconutOpened()
    {
        coconutOpened = true;
    }

    public void RegisterCrabCaught()
    {
        crabCaught = true;
    }

    public void RegisterFirstNightSurvived()
    {
        firstNightSurvived = true;
        TryCompleteCurrentTask();
    }

    public void RegisterTentBuilt()
    {
        tentBuilt = true;
        completedTasks.Add(TaskID.CraftTent);
        TryCompleteCurrentTask();
    }

    public void RegisterRaftBuilt()
    {
        raftBuilt = true;
        completedTasks.Add(TaskID.BuildRaft);
        TryCompleteCurrentTask();
    }

    public void RegisterThomasEndingFinished()
    {
        CompleteTask(TaskID.SpeakToThomasSecond);
    }

    private bool IsObjectiveComplete(TaskID taskID)
    {
        if (completedTasks.Contains(taskID))
            return true;

        switch (taskID)
        {
            case TaskID.GatherResources:
                return woodCollected >= 5
                    && vinesCollected >= 5
                    && rocksCollected >= 5;

            case TaskID.CraftTools:
                return hammerCrafted && spearCrafted;

            case TaskID.FindFood:
                return coconutOpened && crabCaught;

            case TaskID.SurviveNight:
                return firstNightSurvived;

            case TaskID.CraftTent:
                return tentBuilt;

            case TaskID.BuildRaft:
                return raftBuilt;

            default:
                return false;
        }
    }

    private void TryCompleteCurrentTask()
    {
        if (tasks == null || showingCompletedTask)
            return;

        if (currentTaskIndex >= tasks.Length)
            return;

        TaskID currentID = tasks[currentTaskIndex].taskID;

        if (IsObjectiveComplete(currentID))
        {
            if (!completedTasks.Contains(currentID))
                completedTasks.Add(currentID);

            if (IsCurrentTask(currentID))
            {
                showingCompletedTask = true;

                if (completedTaskCoroutine != null)
                    StopCoroutine(completedTaskCoroutine);

                completedTaskCoroutine =
                    StartCoroutine(ShowCompletedTask());
            }
        }

        UpdateTaskUI();
    }

    private IEnumerator ShowCompletedTask()
    {
        UpdateCompletedTaskUI();

        yield return new WaitForSeconds(completedTaskDisplayTime);

        currentTaskIndex++;
        showingCompletedTask = false;
        completedTaskCoroutine = null;

        SkipAlreadyCompletedTasks();

        UpdateTaskUI();
        TryCompleteCurrentTask();
    }

    private void SkipAlreadyCompletedTasks()
    {
        if (tasks == null)
            return;

        while (currentTaskIndex < tasks.Length)
        {
            TaskID id = tasks[currentTaskIndex].taskID;

            if (!IsObjectiveComplete(id))
                break;

            completedTasks.Add(id);
            currentTaskIndex++;
        }
    }

    private void UpdateTaskUI()
    {
        if (taskText1 == null || taskText2 == null || tasks == null)
            return;

        if (currentTaskIndex < tasks.Length)
        {
            taskText1.text = tasks[currentTaskIndex].taskText;
            taskText1.color = activeTaskColour;
            taskText1.gameObject.SetActive(true);
        }
        else
        {
            taskText1.text = "";
            taskText1.gameObject.SetActive(false);
        }

        if (currentTaskIndex + 1 < tasks.Length)
        {
            taskText2.text = tasks[currentTaskIndex + 1].taskText;
            taskText2.color = activeTaskColour;
            taskText2.gameObject.SetActive(false);
        }
        else
        {
            taskText2.text = "";
            taskText2.gameObject.SetActive(false);
        }
    }

    private void UpdateCompletedTaskUI()
    {
        if (taskText1 == null || taskText2 == null || tasks == null)
            return;

        if (currentTaskIndex < tasks.Length)
        {
            taskText1.text = tasks[currentTaskIndex].taskText;
            taskText1.color = completedTaskColour;
            taskText1.gameObject.SetActive(true);
        }

        if (currentTaskIndex + 1 < tasks.Length)
        {
            taskText2.text = tasks[currentTaskIndex + 1].taskText;
            taskText2.color = activeTaskColour;
            taskText2.gameObject.SetActive(true);
        }
        else
        {
            taskText2.text = "";
            taskText2.gameObject.SetActive(false);
        }
    }

    public bool IsTaskCompleted(TaskID taskID)
    {
        return completedTasks.Contains(taskID)
            || IsObjectiveComplete(taskID);
    }

    public bool IsCurrentTask(TaskID taskID)
    {
        if (tasks == null || currentTaskIndex >= tasks.Length)
            return false;

        return tasks[currentTaskIndex].taskID == taskID;
    }

    public TaskID GetCurrentTaskID()
    {
        if (tasks == null || currentTaskIndex >= tasks.Length)
            return TaskID.SpeakToThomasSecond;

        return tasks[currentTaskIndex].taskID;
    }



    public bool HasSurvivedFirstNight()
    {
        return firstNightSurvived;
    }
}
