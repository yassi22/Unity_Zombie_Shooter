using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class FloatingLoot : MonoBehaviour
{
    public float floatHeight = 0.2f;  // Floating range
    public float floatSpeed = 2.5f;     // Speed of the floating effect
    private Vector3 startPos;
    private bool isGrabbed = false;
    private Transform player;
    private Rigidbody rb;
    private XRGrabInteractable grabInteractable;

    public bool droppedByEnemy = true;

    void Start()
    {
        if (!droppedByEnemy)
        {
            Destroy(this);
        }
        startPos = transform.position;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<XRGrabInteractable>();

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrabbed); // Listen for XR grab event
        }
    }

    void Update()
    {
        if (isGrabbed) return;

        // Floating animation
        transform.position = startPos + new Vector3(0, Mathf.Sin(Time.time * floatSpeed) * floatHeight, 0);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        isGrabbed = true;

        // Enable physics so it behaves normally
        if (rb != null)
        {
            Debug.Log("Grabbed magazine");
            rb.isKinematic = false; // Enable physics
            rb.useGravity = true;   // Allow gravity to affect it
        }

        // Stop floating effect
        Destroy(this); // Remove floating behavior but keep the magazine
    }
}