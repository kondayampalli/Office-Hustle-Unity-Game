using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Documents")]
    [SerializeField] private UIDocument gameplayUIDocument;
    [SerializeField] private UIDocument menuUIDocument;

    // Root elements
    private VisualElement gameplayRoot;
    private VisualElement menuRoot;

    // Gameplay UI elements
    private Label scoreLabel;
    private ProgressBar stressBar;
    private Label currentTaskLabel;
    private ProgressBar taskProgressBar;
    private ScrollView taskQueueScrollView;
    private VisualElement taskProgressOverlay;

    // Menu UI elements
    private VisualElement mainMenuPanel;
    private VisualElement pauseMenuPanel;
    private VisualElement gameOverPanel;
    private Label gameOverReasonLabel;
    private Label finalScoreLabel;

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
        InitializeUI();
        SubscribeToEvents();
    }

    private void InitializeUI()
    {
        // Get root elements
        gameplayRoot = gameplayUIDocument.rootVisualElement;
        menuRoot = menuUIDocument.rootVisualElement;

        // Cache gameplay UI elements
        scoreLabel = gameplayRoot.Q<Label>("score-label");
        stressBar = gameplayRoot.Q<ProgressBar>("stress-bar");
        currentTaskLabel = gameplayRoot.Q<Label>("current-task-label");
        taskProgressBar = gameplayRoot.Q<ProgressBar>("task-progress-bar");
        taskQueueScrollView = gameplayRoot.Q<ScrollView>("task-queue");
        taskProgressOverlay = gameplayRoot.Q<VisualElement>("task-progress-overlay");

        // Cache menu UI elements
        mainMenuPanel = menuRoot.Q<VisualElement>("main-menu-panel");
        pauseMenuPanel = menuRoot.Q<VisualElement>("pause-menu-panel");
        gameOverPanel = menuRoot.Q<VisualElement>("game-over-panel");
        gameOverReasonLabel = menuRoot.Q<Label>("game-over-reason");
        finalScoreLabel = menuRoot.Q<Label>("final-score");

        // Setup button callbacks
        SetupMenuButtons();

        // Initial state
        ShowMainMenu();
        taskProgressOverlay.style.display = DisplayStyle.None;
    }

    private void SetupMenuButtons()
    {
        // Main Menu buttons
        menuRoot.Q<Button>("start-button")?.RegisterCallback<ClickEvent>(evt => OnStartGame());
        menuRoot.Q<Button>("quit-button")?.RegisterCallback<ClickEvent>(evt => OnQuitGame());

        // Pause Menu buttons
        menuRoot.Q<Button>("resume-button")?.RegisterCallback<ClickEvent>(evt => OnResumeGame());
        menuRoot.Q<Button>("restart-button")?.RegisterCallback<ClickEvent>(evt => OnRestartGame());
        menuRoot.Q<Button>("main-menu-button")?.RegisterCallback<ClickEvent>(evt => OnReturnToMainMenu());

        // Game Over buttons
        menuRoot.Q<Button>("game-over-restart")?.RegisterCallback<ClickEvent>(evt => OnRestartGame());
        menuRoot.Q<Button>("game-over-main-menu")?.RegisterCallback<ClickEvent>(evt => OnReturnToMainMenu());
    }

    private void SubscribeToEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged += UpdateScore;
            GameManager.Instance.OnStressChanged += UpdateStress;
            GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        }

        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.OnTaskQueueUpdated += UpdateTaskQueue;
            TaskManager.Instance.OnTaskAccepted += UpdateCurrentTask;
            TaskManager.Instance.OnTaskCompleted += OnTaskCompleted;
        }
    }

    private void UpdateScore(int score)
    {
        scoreLabel.text = $"Score: {score}";
    }

    private void UpdateStress(float stress)
    {
        stressBar.value = stress;
        stressBar.title = $"Stress: {stress:F0}%";

        // Change color based on stress level
        if (stress > 75)
            stressBar.AddToClassList("high-stress");
        else if (stress > 50)
            stressBar.AddToClassList("medium-stress");
        else
            stressBar.RemoveFromClassList("high-stress");
    }

    private void UpdateCurrentTask(Task task)
    {
        if (task != null)
        {
            currentTaskLabel.text = $"{task.title} - {task.assignedBy}";
            taskProgressBar.style.display = DisplayStyle.Flex;
        }
    }

    private void OnTaskCompleted(Task task)
    {
        currentTaskLabel.text = "No active task";
        taskProgressBar.style.display = DisplayStyle.None;
    }

    private void UpdateTaskQueue()
    {
        taskQueueScrollView.Clear();

        var tasks = TaskManager.Instance.GetTaskQueue();
        foreach (var task in tasks)
        {
            var taskElement = CreateTaskElement(task);
            taskQueueScrollView.Add(taskElement);
        }
    }

    private VisualElement CreateTaskElement(Task task)
    {
        var container = new VisualElement();
        container.AddToClassList("task-item");

        var infoContainer = new VisualElement();
        infoContainer.AddToClassList("task-info");

        var titleLabel = new Label(task.title);
        titleLabel.AddToClassList("task-title");

        var detailsLabel = new Label($"From: {task.assignedBy} | Time: {task.GetFormattedTimeRemaining()}");
        detailsLabel.AddToClassList("task-details");

        var descriptionLabel = new Label(task.description);
        descriptionLabel.AddToClassList("task-description");

        infoContainer.Add(titleLabel);
        infoContainer.Add(detailsLabel);
        infoContainer.Add(descriptionLabel);

        var buttonContainer = new VisualElement();
        buttonContainer.AddToClassList("task-buttons");

        var acceptButton = new Button(() => TaskManager.Instance.AcceptTask(task.taskId)) { text = "Accept" };
        acceptButton.AddToClassList("accept-button");

        var declineButton = new Button(() => TaskManager.Instance.DeclineTask(task.taskId)) { text = "Decline" };
        declineButton.AddToClassList("decline-button");

        buttonContainer.Add(acceptButton);
        buttonContainer.Add(declineButton);

        container.Add(infoContainer);
        container.Add(buttonContainer);

        // Update time remaining color
        if (task.GetTimeProgress() < 0.3f)
        {
            container.AddToClassList("urgent-task");
        }

        return container;
    }

    public void ShowTaskProgress(float duration)
    {
        taskProgressOverlay.style.display = DisplayStyle.Flex;
        var progressBar = taskProgressOverlay.Q<ProgressBar>("task-completion-progress");
        if (progressBar != null)
        {
            progressBar.value = 0;
            progressBar.schedule.Execute(() =>
            {
                progressBar.value += (100f / duration) * 0.1f;
                if (progressBar.value >= 100)
                {
                    taskProgressOverlay.style.display = DisplayStyle.None;
                }
            }).Every(100).Until(() => progressBar.value >= 100);
        }
    }

    private void Update()
    {
        // Update current task timer
        var currentTask = TaskManager.Instance?.GetCurrentTask();
        if (currentTask != null && currentTask.isActive)
        {
            taskProgressBar.value = currentTask.GetTimeProgress() * 100;
            taskProgressBar.title = currentTask.GetFormattedTimeRemaining();
        }
    }

    private void OnGameStateChanged(GameManager.GameState newState)
    {
        switch (newState)
        {
            case GameManager.GameState.MainMenu:
                ShowMainMenu();
                break;
            case GameManager.GameState.Playing:
                ShowGameplayUI();
                break;
            case GameManager.GameState.Paused:
                ShowPauseMenu();
                break;
            case GameManager.GameState.GameOver:
                // ShowGameOverScreen is called separately with reason
                break;
        }
    }

    private void ShowMainMenu()
    {
        gameplayRoot.style.display = DisplayStyle.None;
        menuRoot.style.display = DisplayStyle.Flex;
        mainMenuPanel.style.display = DisplayStyle.Flex;
        pauseMenuPanel.style.display = DisplayStyle.None;
        gameOverPanel.style.display = DisplayStyle.None;
    }

    private void ShowGameplayUI()
    {
        gameplayRoot.style.display = DisplayStyle.Flex;
        menuRoot.style.display = DisplayStyle.None;
    }

    private void ShowPauseMenu()
    {
        menuRoot.style.display = DisplayStyle.Flex;
        mainMenuPanel.style.display = DisplayStyle.None;
        pauseMenuPanel.style.display = DisplayStyle.Flex;
        gameOverPanel.style.display = DisplayStyle.None;
    }

    public void ShowGameOverScreen(string reason, int finalScore)
    {
        menuRoot.style.display = DisplayStyle.Flex;
        mainMenuPanel.style.display = DisplayStyle.None;
        pauseMenuPanel.style.display = DisplayStyle.None;
        gameOverPanel.style.display = DisplayStyle.Flex;

        gameOverReasonLabel.text = reason;
        finalScoreLabel.text = $"Final Score: {finalScore}";
    }

    // Button callbacks
    private void OnStartGame()
    {
        GameManager.Instance.StartGame();
    }

    private void OnResumeGame()
    {
        GameManager.Instance.ResumeGame();
    }

    private void OnRestartGame()
    {
        GameManager.Instance.RestartGame();
    }

    private void OnReturnToMainMenu()
    {
        GameManager.Instance.SetGameState(GameManager.GameState.MainMenu);
    }

    private void OnQuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged -= UpdateScore;
            GameManager.Instance.OnStressChanged -= UpdateStress;
            GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        }

        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.OnTaskQueueUpdated -= UpdateTaskQueue;
            TaskManager.Instance.OnTaskAccepted -= UpdateCurrentTask;
            TaskManager.Instance.OnTaskCompleted -= OnTaskCompleted;
        }
    }
}