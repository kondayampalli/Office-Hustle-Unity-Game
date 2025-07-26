using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EmployeeAI : MonoBehaviour
{
    [Header("Employee Info")]
    [SerializeField] private string employeeName = "Employee";
    [SerializeField] private EmployeeRole role;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] private float minIdleTime = 5f;
    [SerializeField] private float maxIdleTime = 15f;
    [SerializeField] private Transform[] waypoints;

    [Header("Visual Settings")]
    [SerializeField] private GameObject speechBubblePrefab;
    [SerializeField] private Transform speechBubblePosition;

    private NavMeshAgent agent;
    private Animator animator;
    private int currentWaypointIndex;
    private bool isMoving;
    private float idleTimer;

    public enum EmployeeRole
    {
        Boss,
        Manager,
        Developer,
        Designer,
        HR,
        Accountant,
        IT,
        Intern
    }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        agent.speed = walkSpeed;

        // Start the wandering behavior
        StartCoroutine(WanderRoutine());
    }

    private IEnumerator WanderRoutine()
    {
        // Start at a random waypoint
        if (waypoints.Length > 0)
        {
            currentWaypointIndex = Random.Range(0, waypoints.Length);
        }

        while (true)
        {
            // Idle at current position
            isMoving = false;
            idleTimer = Random.Range(minIdleTime, maxIdleTime);

            if (animator != null)
            {
                animator.SetBool("IsWalking", false);
            }

            yield return new WaitForSeconds(idleTimer);

            // Move to next random waypoint
            if (waypoints.Length > 0)
            {
                int nextWaypointIndex;
                do
                {
                    nextWaypointIndex = Random.Range(0, waypoints.Length);
                } while (waypoints.Length > 1 && nextWaypointIndex == currentWaypointIndex);

                currentWaypointIndex = nextWaypointIndex;
                Vector3 destination = waypoints[currentWaypointIndex].position;

                agent.SetDestination(destination);
                isMoving = true;

                if (animator != null)
                {
                    animator.SetBool("IsWalking", true);
                }

                // Wait until reached destination
                while (agent.pathPending || agent.remainingDistance > 0.5f)
                {
                    yield return null;
                }
            }
        }
    }

    public void ShowSpeechBubble(string message, float duration = 5f)
    {
        if (speechBubblePrefab != null && speechBubblePosition != null)
        {
            GameObject bubble = Instantiate(speechBubblePrefab, speechBubblePosition.position, Quaternion.identity);
            bubble.transform.SetParent(speechBubblePosition);

            // Set the message in the speech bubble
            var textComponent = bubble.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = message;
            }

            Destroy(bubble, duration);
        }
    }

    public string GetEmployeeName()
    {
        return employeeName;
    }

    public EmployeeRole GetRole()
    {
        return role;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Show random work-related comment
            string[] comments = GetRoleComments();
            if (comments.Length > 0)
            {
                ShowSpeechBubble(comments[Random.Range(0, comments.Length)]);
            }
        }
    }

    private string[] GetRoleComments()
    {
        switch (role)
        {
            case EmployeeRole.Boss:
                return new string[] {
                    "I need those reports ASAP!",
                    "We have a deadline to meet!",
                    "Time is money!",
                    "Where's my coffee?"
                };
            case EmployeeRole.Manager:
                return new string[] {
                    "Let's sync up later.",
                    "Can you handle this task?",
                    "We need to optimize our workflow.",
                    "Meeting in 5 minutes!"
                };
            case EmployeeRole.Developer:
                return new string[] {
                    "It works on my machine...",
                    "Just one more bug to fix.",
                    "Coffee is my fuel.",
                    "Compiling... again."
                };
            case EmployeeRole.Designer:
                return new string[] {
                    "The kerning is off!",
                    "Can we make it pop more?",
                    "I need more whitespace.",
                    "Comic Sans? Really?"
                };
            case EmployeeRole.HR:
                return new string[] {
                    "Remember the company policy!",
                    "Your timesheet is due.",
                    "Mandatory fun event tomorrow!",
                    "Did you get the memo?"
                };
            case EmployeeRole.Accountant:
                return new string[] {
                    "The numbers don't add up.",
                    "Budget cuts incoming!",
                    "Save those receipts!",
                    "Quarter end is approaching."
                };
            case EmployeeRole.IT:
                return new string[] {
                    "Have you tried turning it off and on?",
                    "Submit a ticket please.",
                    "The server is down... again.",
                    "Password must be 47 characters long."
                };
            case EmployeeRole.Intern:
                return new string[] {
                    "Is this my job?",
                    "Where's the coffee machine?",
                    "I'm learning so much!",
                    "Can someone help me?"
                };
            default:
                return new string[] { "Busy busy busy!" };
        }
    }
}