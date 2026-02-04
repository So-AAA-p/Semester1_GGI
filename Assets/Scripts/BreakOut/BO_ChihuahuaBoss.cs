using UnityEngine;

namespace BreakOut
{
    public class BO_ChihuahuaBoss : MonoBehaviour
    {
    public enum Phase { Puppy, Teen, Alpha }
    public Phase currentPhase = Phase.Puppy;

    public float maxHealth = 100f;
    private float currentHealth;

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        UpdatePhase();
    }

    void UpdatePhase()
    {
        float healthPercent = currentHealth / maxHealth;

        if (healthPercent < 0.33f) currentPhase = Phase.Alpha;
        else if (healthPercent < 0.66f) currentPhase = Phase.Teen;
        else currentPhase = Phase.Puppy;

        // Trigger visual changes or accuracy increases here
    }
    }
}

