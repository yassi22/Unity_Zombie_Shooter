using UnityEngine;

public class DisableMagazineCollision : MonoBehaviour
{
    private Magazine currentMagazine;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Controller") && !other.gameObject.CompareTag("Player")) return;
        Magazine[] magazines = FindObjectsByType<Magazine>(FindObjectsSortMode.InstanceID);
        foreach (var magazine in magazines)
        {
            if (magazine.CompareTag("CurrentMagazine"))
            {
                currentMagazine = magazine;
            }
        }
        if (currentMagazine != null)
        {
            EnableMagazineCollision(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.gameObject.CompareTag("Controller") && !other.gameObject.CompareTag("Player")) return;
        Magazine[] magazines = FindObjectsByType<Magazine>(FindObjectsSortMode.InstanceID);
        foreach (var magazine in magazines)
        {
            if (magazine.CompareTag("CurrentMagazine"))
            {
                currentMagazine = magazine;
            }
        }
        if (currentMagazine != null)
        {
            EnableMagazineCollision(true);
        }
    }

    public void EnableMagazineCollision(bool enabled)
    {
        if (currentMagazine == null)
        {
            Debug.LogWarning("EnableMagazineCollision called, but currentMagazine is not assigned.");
            return;
        }

        Rigidbody rb = currentMagazine.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody component is missing on the currentMagazine GameObject.");
            return;
        }

        rb.detectCollisions = enabled;
    }
}
