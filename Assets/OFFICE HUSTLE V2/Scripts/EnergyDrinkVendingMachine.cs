// EnergyDrinkVendingMachine.cs
using UnityEngine;

public class EnergyDrinkVendingMachine : MonoBehaviour, IInteractable
{
    [Header("Vending Machine Settings")]
    [SerializeField] private int drinkCost = 5;
    [SerializeField] private float energyBoost = 15f;
    [SerializeField] private AudioClip vendingSound;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void Interact()
    {
        if (GameManager.Instance.GetCurrentState() != GameManager.GameState.Playing)
            return;

        int currentScore = GameManager.Instance.GetCurrentScore();

        if (currentScore >= drinkCost)
        {
            // Deduct cost
            GameManager.Instance.ModifyScore(-drinkCost);

            // Apply energy boost
            float currentStress = GameManager.Instance.GetCurrentStress();
            GameManager.Instance.ModifyStress(-energyBoost);

            // Play sound
            if (vendingSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(vendingSound);
            }
        }
    }

    public string GetInteractionPrompt()
    {
        int currentScore = GameManager.Instance.GetCurrentScore();
        return currentScore >= drinkCost ?
            $"Press E to buy energy drink (Cost: {drinkCost} points)" :
            "Not enough points for energy drink";
    }
}
