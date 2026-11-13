using System;
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
        SpeakToThomasResources,
        OpenCraftingBench,
        SpeakToThomasNight,
        SurviveNight,
        CraftTent,
        BuildRaft,
        SpeakToThomasEnding,
        SailAway
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

    [Header("Progression References")]
    [SerializeField] private TimeController timeController;
    [SerializeField] private BuildItems tentBuildItem;
    [SerializeField] private BuildItems raftBuildItem;

    private TaskData[] tasks;
    private int currentTaskIndex;

    private bool showingCompletedTask;
    private Coroutine completedTaskCoroutine;

    private bool journalOpenRequested;
    private bool journalWasOpen;
    private bool journalFinished;

    private readonly HashSet<TaskID> completedTasks =
        new HashSet<TaskID>();

    private readonly DateTime dayTwoSixAM =
        new DateTime(2000, 1, 2, 6, 0, 0);

    private void Start()
    {
        if (timeController == null)
            timeController = TimeController.instance;

        CreateTasks();

        currentTaskIndex = 0;

        UpdateTaskUI();
        TryCompleteCurrentTask();
    }

    private void Update()
    {
        if (timeController == null)
        {
            timeController = TimeController.instance;

            if (timeController == null)
                timeController = FindFirstObjectByType<TimeController>();
        }

        CheckJournalReading();
        CheckCraftingBench();
        CheckSurviveNight();
        CheckBuildProgress();
    }

    private void CreateTasks()
    {
        tasks = new TaskData[]
        {
            new TaskData(
                TaskID.SpeakToThomasFirst,
                "Talk to Thomas Montgomery"
            ),

            new TaskData(
                TaskID.SearchWoodenBox,
                "Search Thomas's wooden box"
            ),

            new TaskData(
                TaskID.ReadJournal,
                "Read Thomas's journal"
            ),

            new TaskData(
                TaskID.SpeakToThomasResources,
                "Speak to Thomas"
            ),

            new TaskData(
                TaskID.OpenCraftingBench,
                "Open the crafting bench"
            ),

            new TaskData(
                TaskID.SpeakToThomasNight,
                "Speak to Thomas"
            ),

            new TaskData(
                TaskID.SurviveNight,
                "Survive until Day 2, 06:00"
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
                TaskID.SpeakToThomasEnding,
                "Say goodbye to Thomas"
            ),

            new TaskData(
                TaskID.SailAway,
                "Sail away"
            )
        };
    }

    public void CompleteTask(TaskID taskID)
    {
        if (!Enum.IsDefined(typeof(TaskID), taskID))
            return;

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
        if (journalFinished)
            return;

        journalOpenRequested = true;

        if (BookManager.IsBookOpen)
            journalWasOpen = true;
    }

    private void CheckJournalReading()
    {
        if (!journalOpenRequested || journalFinished)
            return;

        if (BookManager.IsBookOpen)
        {
            journalWasOpen = true;
            return;
        }

        if (!journalWasOpen)
            return;

        journalFinished = true;

        CompleteTask(TaskID.ReadJournal);
    }

    private void CheckCraftingBench()
    {
        if (!IsCurrentTask(TaskID.OpenCraftingBench))
            return;

        if (CraftingBench.IsCraftingOpen)
            CompleteTask(TaskID.OpenCraftingBench);
    }

    private void CheckSurviveNight()
    {
        if (!IsCurrentTask(TaskID.SurviveNight))
            return;

        if (timeController == null)
            return;

        if (timeController.CurrentTime >= dayTwoSixAM)
            CompleteTask(TaskID.SurviveNight);
    }

    private void CheckBuildProgress()
    {
        if (IsCurrentTask(TaskID.CraftTent))
        {
            if (tentBuildItem != null && tentBuildItem.IsBuilt)
                CompleteTask(TaskID.CraftTent);
        }

        if (IsCurrentTask(TaskID.BuildRaft))
        {
            if (raftBuildItem != null && raftBuildItem.IsBuilt)
                CompleteTask(TaskID.BuildRaft);
        }
    }

    public void RegisterTentBuilt()
    {
        CompleteTask(TaskID.CraftTent);
    }

    public void RegisterRaftBuilt()
    {
        CompleteTask(TaskID.BuildRaft);
    }

    public void RegisterThomasEndingFinished()
    {
        CompleteTask(TaskID.SpeakToThomasEnding);
    }

    public void RegisterSailAway()
    {
        CompleteTask(TaskID.SailAway);
    }

    private void TryCompleteCurrentTask()
    {
        if (tasks == null || showingCompletedTask)
            return;

        if (currentTaskIndex >= tasks.Length)
            return;

        TaskID currentID = tasks[currentTaskIndex].taskID;

        if (!completedTasks.Contains(currentID))
        {
            UpdateTaskUI();
            return;
        }

        showingCompletedTask = true;

        UpdateCompletedTaskUI();

        if (completedTaskCoroutine != null)
            StopCoroutine(completedTaskCoroutine);

        completedTaskCoroutine =
            StartCoroutine(ShowCompletedTask());
    }

    private IEnumerator ShowCompletedTask()
    {
        if (completedTaskDisplayTime > 0f)
            yield return new WaitForSeconds(completedTaskDisplayTime);

        currentTaskIndex++;

        showingCompletedTask = false;
        completedTaskCoroutine = null;

        UpdateTaskUI();

        TryCompleteCurrentTask();
    }

    private void UpdateTaskUI()
    {
        if (tasks == null)
            return;

        if (taskText1 != null)
        {
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
        }

        if (taskText2 != null)
        {
            taskText2.text = "";
            taskText2.gameObject.SetActive(false);
        }
    }

    private void UpdateCompletedTaskUI()
    {
        if (tasks == null)
            return;

        if (taskText1 != null)
        {
            if (currentTaskIndex < tasks.Length)
            {
                taskText1.text = tasks[currentTaskIndex].taskText;
                taskText1.color = completedTaskColour;
                taskText1.gameObject.SetActive(true);
            }
        }

        if (taskText2 != null)
        {
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
    }

    public bool IsTaskCompleted(TaskID taskID)
    {
        return completedTasks.Contains(taskID);
    }

    public bool IsCurrentTask(TaskID taskID)
    {
        if (tasks == null || currentTaskIndex >= tasks.Length)
            return false;

        if (showingCompletedTask || completedTasks.Contains(taskID))
            return false;

        return tasks[currentTaskIndex].taskID == taskID;
    }

    public TaskID GetCurrentTaskID()
    {
        if (tasks == null || currentTaskIndex >= tasks.Length)
            return TaskID.SailAway;

        return tasks[currentTaskIndex].taskID;
    }

    public bool HasSurvivedFirstNight()
    {
        return IsTaskCompleted(TaskID.SurviveNight);
    }

    public bool HasCompletedAllTasks()
    {
        return tasks != null && currentTaskIndex >= tasks.Length;
    }

    public void RegisterWoodCollected(int amount = 1)
    {
    }

    public void RegisterVinesCollected(int amount = 1)
    {
    }

    public void RegisterRocksCollected(int amount = 1)
    {
    }

    public void RegisterItemCrafted(string itemName)
    {
        if (string.Equals(itemName, "tent", StringComparison.OrdinalIgnoreCase))
        {
            RegisterTentBuilt();
            return;
        }

        if (string.Equals(itemName, "raft", StringComparison.OrdinalIgnoreCase))
            RegisterRaftBuilt();
    }

    public void RegisterCoconutOpened()
    {
    }

    public void RegisterCrabCaught()
    {
    }

    public void RegisterFirstNightSurvived()
    {
    }
}