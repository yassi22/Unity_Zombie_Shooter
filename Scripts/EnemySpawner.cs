using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI; // UI TextMeshPro

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform player;
    public SpawnPoint[] spawnPoints;

    [Header("Wave Instellingen")] public int startEnemyCount = 10;
    public float waveDuration = 120f; // 5 minuten
    public float timeBetweenWaves = 5f; // Tijd tussen waves
    public int maxWaves = 10;

    private int currentWave = 0;
    private int enemyCount;
    private float waveStartTime;
    private float remainingWaveTime;
    private float nextWaveTime;

    [Header("UI Elements")] public TextMeshProUGUI waveTimerText;
    public TextMeshProUGUI enemyCountText;
    public TextMeshProUGUI nextWaveText;
    public TextMeshProUGUI currentWaveText;

    private List<GameObject> activeEnemies = new List<GameObject>();
    private bool gameStarted = false; 
    
    public void Start()
    {
        // Zoek eerst de Canvas
        Canvas mainCanvas = Object.FindFirstObjectByType<Canvas>();

        if (mainCanvas == null)
        {
            Debug.LogError("Geen Canvas gevonden in de scene!");
            return;
        }

        // Zoek alle TextMeshProUGUI componenten in de Canvas
        TextMeshProUGUI[] allTexts = mainCanvas.GetComponentsInChildren<TextMeshProUGUI>();

        // Loop door alle gevonden tekst componenten
        foreach (TextMeshProUGUI text in allTexts)
        {
            switch (text.gameObject.name)
            {
                case "waveTimerText":
                    waveTimerText = text;
                    break;
                case "enemyCountText":
                    enemyCountText = text;
                    break;
                case "nextWaveText":
                    nextWaveText = text;
                    break;
                case "currentWaveText":
                    currentWaveText = text;
                    break;
            }
        }

        // Debug informatie
        Debug.Log($"Wave Timer Text found: {waveTimerText != null}");
        Debug.Log($"Enemy Count Text found: {enemyCountText != null}");
        Debug.Log($"Next Wave Text found: {nextWaveText != null}");
        Debug.Log($"Current Wave Text found: {currentWaveText != null}");
    }  

    public void StartGame()
    {
        Debug.Log("Clicked start button!");
        if (gameStarted) return;

        gameStarted = true;
        enemyCount = startEnemyCount;

        Debug.Log("Game gestart!");

        StartCoroutine(WaveSystem());
    }


    public IEnumerator WaveSystem()
    {
        while (currentWave <= maxWaves)
        {
            currentWave++;
            waveStartTime = Time.time;
            remainingWaveTime = waveDuration;
            nextWaveTime = timeBetweenWaves;

            Debug.Log($"Wave {currentWave} gestart met {enemyCount} vijanden.");

            UpdateWaveUI();
            SpawnEnemies(enemyCount);
            StartCoroutine(UpdateUITimer());

            yield return new WaitForSeconds(waveDuration);

            Debug.Log($"Wave {currentWave} afgelopen. Volgende wave begint in {timeBetweenWaves} seconden.");

            if (currentWave == maxWaves)
            {
                Debug.Log("Gefeliciteerd! Je hebt overleefd tot wave 10!");
                if (currentWaveText != null)
                {
                    currentWaveText.text = "🎉 GEWONNEN! 🎉";
                }

                foreach (GameObject activeEnemy in activeEnemies.ToArray()) // Ensure safe iteration
                {
                    if (activeEnemy != null)
                    {
                        Enemy enemy = activeEnemy.GetComponent<Enemy>();
                        if (enemy != null)
                        {
                            enemy.Die();
                        }
                    }
                }

                activeEnemies.Clear(); // Ensure no references remain

                yield break; // Stop coroutine
            }

            yield return StartCoroutine(WaitForNextWave());

            enemyCount += 2; // Elke wave 2 extra vijanden
        }

        Debug.Log("Max waves bereikt. Geen nieuwe waves meer.");
    }


    public IEnumerator WaitForNextWave()
    {
        while (nextWaveTime > 0)
        {
            if (nextWaveText != null)
            {
                nextWaveTime -= Time.deltaTime;
                nextWaveText.text = $"Volgende Wave: {Mathf.Ceil(nextWaveTime)}s";
                Debug.Log($"Updated Next Wave Timer to: {Mathf.Ceil(nextWaveTime)}s");
            }
            else
            {
                Debug.LogError("nextWaveText is null!");
            }
            yield return null;
        }
    }

    public void SpawnEnemies(int count)
    {
        if (spawnPoints.Length == 0)
        {
            Debug.LogError("Geen spawnpoints ingesteld!");
            return;
        }

        Debug.Log($"Spawning {count} vijanden...");
        for (int i = 0; i < count; i++)
        {
            int spawnIndex = i % spawnPoints.Length;
            SpawnPoint spawnPoint = spawnPoints[spawnIndex];

            Vector3 spawnPosition = spawnPoint.transform.position + new Vector3(
                Random.Range(-1f, 1f),
                0,
                Random.Range(-1f, 1f)
            );

            GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            activeEnemies.Add(newEnemy);

            Enemy enemyScript = newEnemy.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                enemyScript.player = player;
                enemyScript.spawner = this;

                // Assign patrol points based on spawn location
                if (spawnPoint.patrolPoints != null && spawnIndex < spawnPoint.patrolPoints.Length)
                {
                    enemyScript.SetPatrolPoints(spawnPoint.patrolPoints);
                }


            Debug.Log($"Enemy {i + 1} gespawned op {spawnPosition}");
            }
            else
            {
                Debug.LogError("Enemy script niet gevonden op prefab!");
            }
        }

        UpdateEnemyCountUI();
    }

    public IEnumerator UpdateUITimer()
    {
        while (remainingWaveTime > 0)
        {
            if (waveTimerText != null)
            {
                remainingWaveTime -= Time.deltaTime;
                waveTimerText.text = $"Wave Tijd: {Mathf.Ceil(remainingWaveTime)}s";
                 Debug.Log($"Updated Timer to: {Mathf.Ceil(remainingWaveTime)}s");
            }
            else
            {
                Debug.LogError("waveTimerText is null!");
            }
            yield return null;
        }
    }

    public void RemoveEnemy(GameObject enemy)
    {
        activeEnemies.Remove(enemy);
        UpdateEnemyCountUI();

        // Check if all enemies are dead and start next wave early
        if (activeEnemies.Count == 0 && currentWave < maxWaves)
        {
            StopCoroutine(WaveSystem()); // Stop current wave timer if running
            StartCoroutine(WaveSystem()); // Start the next wave immediately
        }
    }


    public void UpdateWaveUI()
    {
        if (currentWaveText != null)
        {
            currentWaveText.text = $"Wave: {currentWave}";
            Debug.Log($"Updated Wave Text to: Wave {currentWave}");
        }
        else
        {
            Debug.LogError("currentWaveText is null!");
        }
    }

    public void UpdateEnemyCountUI()
    {
        Debug.Log($"Update UI: {activeEnemies.Count} vijanden over.");
        if (enemyCountText != null)
            enemyCountText.text = $"Enemies: {activeEnemies.Count}";
        else
            Debug.LogError("enemyCountText niet toegewezen in de Inspector!");
    }
 }
 