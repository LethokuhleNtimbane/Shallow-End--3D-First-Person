using System.Collections;
using TMPro;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public enum TaskID
    {
        SpeakToThomasFirst,
        SearchWoodenBox,
        ReadJournal,
        CraftTent,
        SpeakToThomasSecond,
        BuildRaft
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

    [SerializeField] private TextMeshProUGUI taskText1;
    [SerializeField] private TextMeshProUGUI taskText2;


    [SerializeField] private Color activeTaskColour = Color.white;
    [SerializeField] private Color completedTaskColour = Color.green;


    [SerializeField] private float completedTaskDisplayTime = 2f;

    private TaskData[] tasks;

    private int currentTaskIndex = 0;

    private bool showingCompletedTask = false;
    private Coroutine completedTaskCoroutine;

    private void Start()
    {
        CreateTasks();

        currentTaskIndex = 0;

        UpdateTaskUI();
    }

    private void CreateTasks()
    {
        tasks = new TaskData[]
        {
            new TaskData(
                TaskID.SpeakToThomasFirst,
                "Go speak to Thomas Montgomery"
            ),

            new TaskData(
                TaskID.SearchWoodenBox,
                "Search wooden box"
            ),

            new TaskData(
                TaskID.ReadJournal,
                "Read the journal"
            ),

            new TaskData(
                TaskID.CraftTent,
                "Craft tent"
            ),

            new TaskData(
                TaskID.SpeakToThomasSecond,
                "Go speak to Thomas Montgomery"
            ),

            new TaskData(
                TaskID.BuildRaft,
                "Build a raft"
            )
        };
    }

    public void CompleteTask(TaskID taskID)
    {
        if (tasks == null)
            return;

        if (currentTaskIndex >= tasks.Length)
            return;

        if (tasks[currentTaskIndex].taskID != taskID)
            return;

        if (showingCompletedTask)
            return;

        showingCompletedTask = true;

        if (completedTaskCoroutine != null)
        {
            StopCoroutine(completedTaskCoroutine);
        }

        completedTaskCoroutine =
            StartCoroutine(ShowCompletedTask());
    }

    private IEnumerator ShowCompletedTask()
    {
        UpdateCompletedTaskUI();

        yield return new WaitForSeconds(completedTaskDisplayTime);

        currentTaskIndex++;

        showingCompletedTask = false;

        UpdateTaskUI();
    }

    private void UpdateTaskUI()
    {
        if (taskText1 == null || taskText2 == null)
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
        if (taskText1 == null || taskText2 == null)
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
    }

    public bool IsTaskCompleted(TaskID taskID)
    {
        if (tasks == null)
            return false;

        if (currentTaskIndex >= tasks.Length)
            return false;

        return tasks[currentTaskIndex].taskID == taskID &&
               showingCompletedTask;
    }

    public bool IsCurrentTask(TaskID taskID)
    {
        if (tasks == null)
            return false;

        if (currentTaskIndex >= tasks.Length)
            return false;

        return tasks[currentTaskIndex].taskID == taskID;
    }

    public TaskID GetCurrentTaskID()
    {
        if (tasks == null || currentTaskIndex >= tasks.Length)
            return TaskID.BuildRaft;

        return tasks[currentTaskIndex].taskID;
    }
}

