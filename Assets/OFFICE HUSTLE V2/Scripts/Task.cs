using System;
using UnityEngine;

[System.Serializable]
public class Task
{
    public string taskId;
    public string title;
    public string description;
    public float timeLimit;
    public float timeRemaining;
    public string assignedBy;
    public TaskType taskType;
    public Vector3 taskLocation;
    public bool isActive;
    public bool isCompleted;
    public GameObject taskIndicator;

    public enum TaskType
    {
        PrintDocuments,
        DeliverPackage,
        OrganizeFiles,
        AttendMeeting,
        FixComputer,
        MakeCoffee,
        CleanDesk,
        AnswerPhone,
        SendEmail,
        PhotocopyCopies
    }

    public Task(string title, string description, float timeLimit, string assignedBy, TaskType type, Vector3 location)
    {
        this.taskId = Guid.NewGuid().ToString();
        this.title = title;
        this.description = description;
        this.timeLimit = timeLimit;
        this.timeRemaining = timeLimit;
        this.assignedBy = assignedBy;
        this.taskType = type;
        this.taskLocation = location;
        this.isActive = false;
        this.isCompleted = false;
    }

    public void UpdateTime(float deltaTime)
    {
        if (isActive && !isCompleted)
        {
            timeRemaining -= deltaTime;
            if (timeRemaining <= 0)
            {
                timeRemaining = 0;
            }
        }
    }

    public bool IsExpired()
    {
        return timeRemaining <= 0 && !isCompleted;
    }

    public float GetTimeProgress()
    {
        return timeRemaining / timeLimit;
    }

    public string GetFormattedTimeRemaining()
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}