using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class Enemy : MonoBehaviour
{
    public EnemySpawner spawner; // Referentie naar de spawner
    [Header("Stats")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Attack Settings")]
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    private bool canAttack = true;

    [Header("Movement Settings")]
    public float detectionRange = 10f;
    public Transform player;

    [Header("Animations")]
    public Animator animator;

    [Header("Hitbox")]
    public Collider hitbox;

    private NavMeshAgent navAgent;

    public Collider rightHandHitbox;
    public Collider leftHandHitbox;

    public GameObject magazinePrefab;

    [Header("Patrolling Settings")]
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;
    private bool isPatrolling = true;

    public float patrolSpeed = 1.5f;  // Walking speed
    public float chaseSpeed = 4f;    // Running speed

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] idleSounds;
    public AudioClip chaseSound;
    public AudioClip attackSound;
    public AudioClip damageSound;
    public AudioClip deathSound;

    private bool isPlayingChaseSound = false;
    private bool isAlive = true;


    void Start()
    {
        currentHealth = maxHealth;
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent == null)
        {
            return;
        }

        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            navAgent.SetDestination(patrolPoints[0].position);
        }

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.spatialBlend = 1.0f; // 3D sound
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 20f;

        StartCoroutine(PlayIdleSounds());
    }

    void Update()
    {
        if (!isAlive) return;

        if (player == null)
        {
            Debug.Log("No player reference!");
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Check if the enemy is on the NavMesh
        if (!navAgent.isOnNavMesh)
        {
            Debug.LogError($"Enemy {gameObject.name} is not on NavMesh!");
            return;
        }

        if (distanceToPlayer <= detectionRange)
        {
            // Player detected, stop patrolling and chase
            isPatrolling = false;
            if (distanceToPlayer > attackRange)
            {
                isPatrolling = false;
                navAgent.isStopped = false;
                navAgent.speed = chaseSpeed; // Set to running speed
                navAgent.SetDestination(player.position);

                animator.SetBool("IsMoving", true);
                animator.SetFloat("Speed", chaseSpeed); // Adjust animation to match speed

                PlayChaseSound();
            }
            else
            {
                // Attack mode
                navAgent.isStopped = true;
                animator.SetBool("IsMoving", false);
                if (canAttack)
                {
                    StartCoroutine(AttackPlayer());
                }
            }
        }
        else
        {
            // Player out of range, resume patrolling
            if (patrolPoints != null && patrolPoints.Length > 0)
            {
                Patrol();
            }
            else
            {
                navAgent.isStopped = true;
                animator.SetBool("IsMoving", false);
            }

            StopChaseSound();
        }
    }

    private IEnumerator PlayIdleSounds()
    {
        while (isAlive)
        {
            yield return new WaitForSeconds(Random.Range(3f, 5f));

            if (!isPlayingChaseSound && !audioSource.isPlaying && idleSounds.Length > 0)
            {
                AudioClip randomIdleSound = idleSounds[Random.Range(0, idleSounds.Length)];
                audioSource.PlayOneShot(randomIdleSound);
            }
        }
    }

    private void PlayChaseSound()
    {
        if (!isPlayingChaseSound && chaseSound != null)
        {
            isPlayingChaseSound = true;
            audioSource.clip = chaseSound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    private void StopChaseSound()
    {
        if (isPlayingChaseSound)
        {
            isPlayingChaseSound = false;
            audioSource.Stop();
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        
        animator.SetTrigger("Hit");

        if (currentHealth <= 0)
        {
            Die();
        }
        else if (damageSound != null)
        {
            audioSource.PlayOneShot(damageSound);
        }
    } 
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    
    public void Die()
    {
    
        if (spawner != null) // Controleer of de spawner-referentie is ingesteld
        {
            spawner.RemoveEnemy(gameObject); // Verwijder de vijand uit de lijst
        }
        animator.SetTrigger("Die");
        navAgent.isStopped = true;
        hitbox.enabled = false; // Disable hitbox
        this.enabled = false; // Disable this script

        StopChaseSound();

        if (deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        // Spawn floating magazine
        SpawnMagazine();
        isAlive = false;

        Destroy(gameObject, 60f); // Destroy na 60 seconden
    }
    
    public IEnumerator AttackPlayer()
    {
        canAttack = false;
        navAgent.isStopped = true; // Stop movement while attacking
        animator.SetBool("IsMoving", false); // Ensure the zombie doesn't move
        animator.SetTrigger("Attack");

        if (attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }

        yield return new WaitForSeconds(0.1f);
        animator.ResetTrigger("Attack");

        if (player != null)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(10f);
            }
        }

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
        navAgent.isStopped = false; // Resume movement after attack
    }

    public Transform GetBloodParentTransform()
    {
        // Return a specific bone or transform for parenting (e.g., chest or root)
        return transform.Find("Base HumanPelvis") ?? transform; 
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            Bullet bullet = other.GetComponent<Bullet>();
            if (bullet != null)
            {
                TakeDamage(bullet.damage);
                Destroy(other.gameObject); // Destroy the bullet
            }
        }
    }

    public void EnableRightHitbox()
    {
        rightHandHitbox.enabled = true;
    }

    public void DisableRightHitbox()
    {
        rightHandHitbox.enabled = false;
    }

    public void EnableLeftHitbox()
    {
        leftHandHitbox.enabled = true;
    }

    public void DisableLeftHitbox()
    {
        leftHandHitbox.enabled = false;
    }

    private void SpawnMagazine()
    {
        if (player == null) return; // Ensure there's a player reference

        Vector3 spawnPosition = transform.position + new Vector3(0, 1.5f, 0); // Slightly above the zombie
        if (magazinePrefab != null)
        {
            GameObject newMagazine = Instantiate(magazinePrefab, spawnPosition, Quaternion.identity);
            newMagazine.AddComponent<FloatingLoot>(); // Attach floating behavior
        }
    }

    private void Patrol()
    {
        if (!isPatrolling)
        {
            isPatrolling = true;
            navAgent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }

        if (navAgent.remainingDistance < 0.5f && !navAgent.pathPending)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            navAgent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }

        navAgent.isStopped = false;
        navAgent.speed = patrolSpeed; // Set to walking speed
        animator.SetBool("IsMoving", true);
        animator.SetFloat("Speed", patrolSpeed); // Adjust animation to match speed
    }


    public void SetPatrolPoints(Transform[] points)
    {
        patrolPoints = points;
    }

}
