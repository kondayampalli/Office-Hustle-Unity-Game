using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance { get; private set; }

    [Header("Task Settings")]
    [SerializeField] private float minTaskInterval = 5f;
    [SerializeField] private float maxTaskInterval = 15f;
    [SerializeField] private int maxQueueSize = 15;
    [SerializeField] private GameObject taskIndicatorPrefab;
    [SerializeField] private Transform[] taskLocations;

    [Header("Employee Names")]
    [SerializeField]
    private string[] employeeNames = {
        "Boss Karen", "Manager Bob", "HR Susan", "IT Mike",
        "Accountant Sarah", "Intern Tim", "Designer Lisa", "Developer John"
    };

    private List<Task> taskQueue = new List<Task>();
    private Task currentTask;
    private float nextTaskTimer;

    public event Action<Task> OnTaskAdded;
    public event Action<Task> OnTaskAccepted;
    public event Action<Task> OnTaskDeclined;
    public event Action<Task> OnTaskCompleted;
    public event Action<Task> OnTaskFailed;
    public event Action OnTaskQueueUpdated;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        nextTaskTimer = UnityEngine.Random.Range(minTaskInterval, maxTaskInterval);
    }

    private void Update()
    {
        if (GameManager.Instance.GetCurrentState() != GameManager.GameState.Playing)
            return;

        // Generate new tasks
        nextTaskTimer -= Time.deltaTime;
        if (nextTaskTimer <= 0 && taskQueue.Count < maxQueueSize)
        {
            GenerateRandomTask();
            nextTaskTimer = UnityEngine.Random.Range(minTaskInterval, maxTaskInterval);
        }

        // Update active task
        if (currentTask != null && currentTask.isActive)
        {
            currentTask.UpdateTime(Time.deltaTime);

            if (currentTask.IsExpired())
            {
                FailTask(currentTask);
            }
        }

        // Update queued tasks (they slowly lose time even in queue)
        foreach (var task in taskQueue)
        {
            task.UpdateTime(Time.deltaTime * 0.3f); // Slower decay in queue
        }

        // Remove expired queued tasks
        taskQueue.RemoveAll(t => t.IsExpired());
    }

    private void GenerateRandomTask()
    {
        var taskTypes = Enum.GetValues(typeof(Task.TaskType)).Cast<Task.TaskType>().ToList();
        var randomType = taskTypes[UnityEngine.Random.Range(0, taskTypes.Count)];

        var taskData = GetTaskData(randomType);
        var randomEmployee = employeeNames[UnityEngine.Random.Range(0, employeeNames.Length)];
        var randomLocation = taskLocations[UnityEngine.Random.Range(0, taskLocations.Length)].position;

        var newTask = new Task(
            taskData.title,
            taskData.description,
            taskData.timeLimit,
            randomEmployee,
            randomType,
            randomLocation
        );

        taskQueue.Add(newTask);
        OnTaskAdded?.Invoke(newTask);
        OnTaskQueueUpdated?.Invoke();
    }

    private (string title, string description, float timeLimit) GetTaskData(Task.TaskType type)
    {
        switch (type)
        {
            case Task.TaskType.PrintDocuments:
                return ("Print Reports", "Print the quarterly reports ASAP!", UnityEngine.Random.Range(60f, 120f));
            case Task.TaskType.DeliverPackage:
                return ("Package Delivery", "Deliver this package to the reception", UnityEngine.Random.Range(45f, 90f));
            case Task.TaskType.OrganizeFiles:
                return ("File Organization", "Sort these files alphabetically", UnityEngine.Random.Range(90f, 150f));
            case Task.TaskType.AttendMeeting:
                return ("Emergency Meeting", "Join the meeting room NOW!", UnityEngine.Random.Range(30f, 60f));
            case Task.TaskType.FixComputer:
                return ("IT Support", "My computer is frozen again!", UnityEngine.Random.Range(120f, 180f));
            case Task.TaskType.MakeCoffee:
                return ("Coffee Run", "We need coffee for the team meeting", UnityEngine.Random.Range(60f, 90f));
            case Task.TaskType.CleanDesk:
                return ("Clean Workspace", "The boss is coming, clean your desk!", UnityEngine.Random.Range(45f, 75f));
            case Task.TaskType.AnswerPhone:
                return ("Important Call", "Answer the ringing phone immediately", UnityEngine.Random.Range(20f, 40f));
            case Task.TaskType.SendEmail:
                return ("Urgent Email", "Send the report to the client NOW", UnityEngine.Random.Range(60f, 100f));
            case Task.TaskType.PhotocopyCopies:
                return ("Make Copies", "We need 50 copies of this document", UnityEngine.Random.Range(90f, 120f));
            default:
                return ("Mystery Task", "Do something productive!", 60f);
        }
    }

    public void AcceptTask(string taskId)
    {
        var task = taskQueue.FirstOrDefault(t => t.taskId == taskId);
        if (task != null && currentTask == null)
        {
            currentTask = task;
            currentTask.isActive = true;
            taskQueue.Remove(task);

            // Create task indicator
            if (taskIndicatorPrefab != null)
            {
                currentTask.taskIndicator = Instantiate(taskIndicatorPrefab, currentTask.taskLocation + Vector3.up * 2f, Quaternion.identity);
            }

            OnTaskAccepted?.Invoke(currentTask);
            OnTaskQueueUpdated?.Invoke();
        }
    }

    public void DeclineTask(string taskId)
    {
        var task = taskQueue.FirstOrDefault(t => t.taskId == taskId);
        if (task != null)
        {
            taskQueue.Remove(task);
            GameManager.Instance.DeclineTask();
            OnTaskDeclined?.Invoke(task);
            OnTaskQueueUpdated?.Invoke();
        }
    }

    public void CompleteCurrentTask()
    {
        if (currentTask != null && currentTask.isActive)
        {
            currentTask.isCompleted = true;
            bool completedOnTime = currentTask.timeRemaining > 0;

            GameManager.Instance.CompleteTask(completedOnTime);

            if (currentTask.taskIndicator != null)
            {
                Destroy(currentTask.taskIndicator);
            }

            OnTaskCompleted?.Invoke(currentTask);
            currentTask = null;
        }
    }

    private void FailTask(Task task)
    {
        if (task.taskIndicator != null)
        {
            Destroy(task.taskIndicator);
        }

        GameManager.Instance.CompleteTask(false);
        OnTaskFailed?.Invoke(task);

        if (currentTask == task)
        {
            currentTask = null;
        }
    }

    public List<Task> GetTaskQueue() => new List<Task>(taskQueue);
    public Task GetCurrentTask() => currentTask;
    public int GetPendingTaskCount() => taskQueue.Count + (currentTask != null ? 1 : 0);

    public bool IsPlayerAtTaskLocation(Vector3 playerPosition, float threshold = 2f)
    {
        if (currentTask == null) return false;
        return Vector3.Distance(playerPosition, currentTask.taskLocation) <= threshold;
    }
}