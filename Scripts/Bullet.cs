using UnityEngine;

public class Bullet : MonoBehaviour
{
    // public float speed = 50f;
    public float maxLifetime = 5f;
    public GameObject bulletHolePrefab;
    public GameObject bulletEnemyImpactPrefab;
    public float damage = 50f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        Destroy(gameObject, maxLifetime); // Auto-destroy bullet after some time
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Gun") || collision.gameObject.CompareTag("Player")) return;
        // Handle damage for enemies
        Enemy enemy = collision.collider.GetComponent<Enemy>();
        if (enemy != null)
        {
            ContactPoint contact = collision.GetContact(0);

            // Instantiate the blood spout prefab
            GameObject bloodSpout = Instantiate(bulletEnemyImpactPrefab, contact.point, Quaternion.LookRotation(contact.normal));

            // Check if the enemy has a specific transform for parenting blood spouts
            Transform bloodParent = enemy.GetBloodParentTransform(); // Custom method in the Enemy class
            if (bloodParent != null)
            {
                bloodSpout.transform.SetParent(bloodParent);
            }
            else
            {
                // Fallback to parenting to the enemy's root transform
                bloodSpout.transform.SetParent(enemy.transform);
            }

            // Apply damage to the enemy
            enemy.TakeDamage(damage);
        }

        else if (bulletHolePrefab != null)
        {
            ContactPoint contact = collision.GetContact(0);
            Instantiate(bulletHolePrefab, contact.point, Quaternion.LookRotation(contact.normal));
            Rigidbody hitRb = collision.rigidbody;
            if (hitRb != null)
            {
                hitRb.AddForce(-contact.normal * 10f, ForceMode.Impulse); // Knockback effect
            }
        }

        Destroy(gameObject); // Destroy bullet after collision
    }
}
