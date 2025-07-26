// CoffeeMachine.cs
using System.Collections;
using UnityEngine;

public class CoffeeMachine : MonoBehaviour, IInteractable
{
    [Header("Coffee Settings")]
    [SerializeField] private float cooldownTime = 30f;
    [SerializeField] private AudioClip brewSound;
    [SerializeField] private ParticleSystem steamParticles;

    private bool isAvailable = true;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Ensure the particle system is stopped at start
        if (steamParticles != null)
        {
            steamParticles.Stop();
        }
    }

    public void Interact()
    {
        if (isAvailable && GameManager.Instance.GetCurrentState() == GameManager.GameState.Playing)
        {
            StartCoroutine(BrewCoffee());
        }
    }

    public string GetInteractionPrompt()
    {
        return isAvailable ? "Press E to brew coffee" : "Coffee machine is brewing...";
    }

    private IEnumerator BrewCoffee()
    {
        isAvailable = false;

        // Play brewing effects
        if (brewSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(brewSound);
        }

        if (steamParticles != null)
        {
            steamParticles.Play();
        }

        // Wait for brewing animation
        yield return new WaitForSeconds(2f);

        // Apply coffee boost
        GameManager.Instance.ConsumeCoffee();

        // Show feedback
        if (UIManager.Instance != null)
        {
            // You could add a notification system here
        }

        // Cooldown
        yield return new WaitForSeconds(cooldownTime);
        isAvailable = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && steamParticles != null)
        {
            steamParticles.Play();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && steamParticles != null)
        {
            steamParticles.Stop();
        }
    }
}