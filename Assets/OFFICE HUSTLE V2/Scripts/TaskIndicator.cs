using UnityEngine;

public class TaskIndicator : MonoBehaviour
{
    [Header("Indicator Settings")]
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float floatAmplitude = 0.5f;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private Color normalColor = Color.yellow;
    [SerializeField] private Color urgentColor = Color.red;
    [SerializeField] private float urgentThreshold = 30f; // seconds

    [Header("Components")]
    [SerializeField] private MeshRenderer indicatorRenderer;
    [SerializeField] private Light indicatorLight;
    [SerializeField] private ParticleSystem urgentParticles;

    private Vector3 startPosition;
    private Material indicatorMaterial;
    private Task associatedTask;

    private void Start()
    {
        startPosition = transform.position;

        if (indicatorRenderer != null)
        {
            indicatorMaterial = indicatorRenderer.material;
        }

        if (indicatorLight != null)
        {
            indicatorLight.color = normalColor;
        }
    }

    private void Update()
    {
        // Floating animation
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);

        // Rotation animation
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Update urgency visual
        if (associatedTask != null)
        {
            UpdateUrgencyVisual();
        }

        // Always face the camera
        if (Camera.main != null)
        {
            Vector3 lookDirection = Camera.main.transform.position - transform.position;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }
    }

    public void SetTask(Task task)
    {
        associatedTask = task;
    }

    private void UpdateUrgencyVisual()
    {
        if (associatedTask.timeRemaining <= urgentThreshold)
        {
            // Change to urgent color
            if (indicatorMaterial != null)
            {
                indicatorMaterial.color = Color.Lerp(normalColor, urgentColor,
                    Mathf.PingPong(Time.time * 3f, 1f));
            }

            if (indicatorLight != null)
            {
                indicatorLight.color = urgentColor;
                indicatorLight.intensity = Mathf.Lerp(1f, 3f, Mathf.PingPong(Time.time * 3f, 1f));
            }

            // Enable urgent particles
            if (urgentParticles != null && !urgentParticles.isPlaying)
            {
                urgentParticles.Play();
            }
        }
        else
        {
            // Normal state
            if (indicatorMaterial != null)
            {
                indicatorMaterial.color = normalColor;
            }

            if (indicatorLight != null)
            {
                indicatorLight.color = normalColor;
                indicatorLight.intensity = 1f;
            }

            // Disable urgent particles
            if (urgentParticles != null && urgentParticles.isPlaying)
            {
                urgentParticles.Stop();
            }
        }
    }
}