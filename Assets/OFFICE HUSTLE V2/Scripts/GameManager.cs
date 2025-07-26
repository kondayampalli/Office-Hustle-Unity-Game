using System;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private int startingScore = 50;
    [SerializeField] private int maxScore = 100;
    [SerializeField] private int scorePerCompletedTask = 10;
    [SerializeField] private int scorePerFailedTask = -15;
    [SerializeField] private int scorePerDeclinedTask = -5;
    [SerializeField] private float stressIncreaseRate = 0.1f;
    [SerializeField] private float coffeeBoostAmount = 20f;

    [Header("UI Documents")]
    [SerializeField] private UIDocument gameplayUI;
    [SerializeField] private UIDocument menuUI;

    private int currentScore;
    private float currentStress;
    private GameState currentState = GameState.MainMenu;

    public event Action<int> OnScoreChanged;
    public event Action<float> OnStressChanged;
    public event Action<GameState> OnGameStateChanged;

    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        currentScore = startingScore;
        currentStress = 0f;
        SetGameState(GameState.MainMenu);
    }

    public void StartGame()
    {
        currentScore = startingScore;
        currentStress = 0f;
        SetGameState(GameState.Playing);
        Time.timeScale = 1f;
    }

    public void PauseGame()
    {
        if (currentState == GameState.Playing)
        {
            SetGameState(GameState.Paused);
            Time.timeScale = 0f;
        }
    }

    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
        {
            SetGameState(GameState.Playing);
            Time.timeScale = 1f;
        }
    }

    public void RestartGame()
    {
        StartGame();
    }

    private void Update()
    {
        if (currentState == GameState.Playing)
        {
            // Increase stress over time
            currentStress += stressIncreaseRate * Time.deltaTime;
            currentStress = Mathf.Clamp(currentStress, 0f, 100f);
            OnStressChanged?.Invoke(currentStress);

            // Check game over conditions
            CheckGameOverConditions();
        }
    }

    public void ModifyScore(int amount)
    {
        currentScore += amount;
        currentScore = Mathf.Clamp(currentScore, 0, maxScore);
        OnScoreChanged?.Invoke(currentScore);

        if (currentScore <= 0)
        {
            GameOver("Your performance score hit rock bottom! You're fired!");
        }
    }

    public void CompleteTask(bool onTime)
    {
        if (onTime)
        {
            ModifyScore(scorePerCompletedTask);
            currentStress = Mathf.Max(0, currentStress - 5f);
        }
        else
        {
            ModifyScore(scorePerFailedTask);
            currentStress = Mathf.Min(100, currentStress + 10f);
        }
        OnStressChanged?.Invoke(currentStress);
    }

    public void DeclineTask()
    {
        ModifyScore(scorePerDeclinedTask);
        currentStress = Mathf.Min(100, currentStress + 5f);
        OnStressChanged?.Invoke(currentStress);
    }

    public void ConsumeCoffee()
    {
        currentStress = Mathf.Max(0, currentStress - coffeeBoostAmount);
        OnStressChanged?.Invoke(currentStress);

        // Small score boost
        ModifyScore(5);
    }

    private void CheckGameOverConditions()
    {
        // Stress overload
        if (currentStress >= 100f)
        {
            GameOver("Stress overload! You had a mental breakdown and quit!");
        }

        // Perfect score achievement
        if (currentScore >= maxScore)
        {
            GameOver("Congratulations! You're promoted to Senior Office Hustler!");
        }

        // Check if too many pending tasks (implemented in TaskManager)
        if (TaskManager.Instance != null && TaskManager.Instance.GetPendingTaskCount() > 10)
        {
            GameOver("Task overflow! You couldn't keep up with the workload!");
        }
    }

    private void GameOver(string reason)
    {
        SetGameState(GameState.GameOver);
        Time.timeScale = 0f;

        // The UI will handle displaying the reason
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOverScreen(reason, currentScore);
        }
    }

    private void SetGameState(GameState newState)
    {
        currentState = newState;
        OnGameStateChanged?.Invoke(currentState);
    }

    public GameState GetCurrentState() => currentState;
    public int GetCurrentScore() => currentScore;
    public float GetCurrentStress() => currentStress;
}