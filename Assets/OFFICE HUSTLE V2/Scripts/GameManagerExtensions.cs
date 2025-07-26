using UnityEngine;

// Extension methods for GameManager to handle stress modification
public static class GameManagerExtensions
{
    public static void ModifyStress(this GameManager gameManager, float amount)
    {
        // Get current stress through reflection or make the field accessible
        var stressField = typeof(GameManager).GetField("currentStress",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (stressField != null)
        {
            float currentStress = (float)stressField.GetValue(gameManager);
            currentStress += amount;
            currentStress = Mathf.Clamp(currentStress, 0f, 100f);
            stressField.SetValue(gameManager, currentStress);

            // Trigger the event
            var stressEvent = typeof(GameManager).GetField("OnStressChanged",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (stressEvent != null)
            {
                var eventDelegate = stressEvent.GetValue(gameManager) as System.Action<float>;
                eventDelegate?.Invoke(currentStress);
            }
        }
    }

    public static void SetGameState(this GameManager gameManager, GameManager.GameState newState)
    {
        // Use reflection to access private method
        var method = typeof(GameManager).GetMethod("SetGameState",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (method != null)
        {
            method.Invoke(gameManager, new object[] { newState });
        }
    }
}