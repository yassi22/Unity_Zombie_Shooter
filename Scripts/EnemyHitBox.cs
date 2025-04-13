using UnityEngine;
using System.Collections;

public class EnemyHitBox : MonoBehaviour
{
    public int damage = 10; // Damage dealt by this hitbox
    public float damageCooldown = 0.5f; // Time before this enemy can hit the player again

    private bool canDamage = true; // Track if this enemy can damage the player

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && canDamage)
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(damage); // Deal damage to the player
                Debug.Log("Player hit by enemy!");

                StartCoroutine(DamageCooldown()); // Start cooldown for this enemy
            }
        }
    }

    private IEnumerator DamageCooldown()
    {
        canDamage = false; // Disable damage from this enemy
        yield return new WaitForSeconds(damageCooldown); // Wait for cooldown
        canDamage = true; // Re-enable damage
    }
}
