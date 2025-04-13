using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Player : MonoBehaviour
{
    public int health = 100;
    private bool alive = true;

    [Header("Vignette Settings")]
    public GameObject redVignette; // Assign your vignette CanvasGroup in the Inspector
    public GameObject redDamageVignette;
    public float maxVignetteAlpha = 0.8f; // Maximum vignette intensity
    public float vignetteFadeSpeed = 2f; // Speed of fading effect
    public float flashIntensity = 1f; // Intensity of the flash when hit
    public float flashDuration = 0.1f; // Duration of the flash
    public float lowHealthThreshold = 30f; // Health percentage for constant vignette

    [Header("Health Regeneration")]
    public float regenDelay = 3f; // Time before regeneration starts
    public float regenDuration = 15f; // Time to fully regenerate health
    public int maxHealth = 100; // Maximum health
    private Coroutine regenCoroutine;
    private Coroutine vignetteFadeCoroutine;
    private float lastDamageTime;
    private bool isLowHealth;

    public TextMeshProUGUI healthText;

    void Update()
    {
        // Constant vignette when health is below the threshold
        if (health < maxHealth * (lowHealthThreshold / 100f))
        {
            redVignette.SetActive(true);
            isLowHealth = true;
        }
        else if (isLowHealth)
        {
            isLowHealth = false;
            redVignette.SetActive(false);
        }

        // Start health regeneration if enough time has passed since the last damage
        if (Time.time - lastDamageTime > regenDelay && regenCoroutine == null && health < maxHealth)
        {
            regenCoroutine = StartCoroutine(RegenerateHealth());
        }
    }

    private void UpdateHealthText()
    {
        healthText.text = "HP: " + health.ToString();
    }

    public void TakeDamage(int damage)
    {
        if (alive)
        {
            health -= damage;
            Debug.Log("Player health: " + health.ToString());
            UpdateHealthText();

            lastDamageTime = Time.time; // Update the time of the last damage

            // Cancel health regeneration if it's currently happening
            if (regenCoroutine != null)
            {
                StopCoroutine(regenCoroutine);
                regenCoroutine = null;
            }

            // Trigger vignette flash
            TriggerVignetteFlash();

            if (health <= 0)
            {
                Die();
            }
            else if(health < 30)
            {
                redVignette.SetActive(true);
            }
        }
    }

    void Die()
    {
        alive = false;
        Debug.LogError("Player died");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void TriggerVignetteFlash()
    {
        // Stop any ongoing vignette fade coroutine
        if (vignetteFadeCoroutine != null)
        {
            StopCoroutine(vignetteFadeCoroutine);
        }

        // Start the flash and fade
        vignetteFadeCoroutine = StartCoroutine(VignetteFlashAndFade());
    }

    private IEnumerator VignetteFlashAndFade()
    {
        redDamageVignette.SetActive(true);

        //// Wait for flash duration
        yield return new WaitForSeconds(flashDuration);
        redDamageVignette.SetActive(false);

        
    }

    private IEnumerator RegenerateHealth()
    {
        float regenRate = maxHealth / regenDuration; // Amount of health regenerated per second

        while (health < maxHealth)
        {
            health += Mathf.CeilToInt(regenRate * Time.deltaTime);
            health = Mathf.Clamp(health, 0, maxHealth); // Ensure health doesn't exceed maxHealth

            UpdateHealthText();
            yield return null; // Wait for next frame
        }

        health = maxHealth; // Ensure health is fully restored
        regenCoroutine = null; // Mark regeneration as finished
    }
}
