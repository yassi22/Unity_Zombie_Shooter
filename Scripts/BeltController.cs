using UnityEngine;

public class BeltController : MonoBehaviour
{
    public Transform headset;  // Reference to the player's headset (camera)
    public Vector3 offset = new Vector3(0, -0.5f, 0);  // Offset for the belt's position relative to the headset
    public float smoothFactor = 0.1f;  // Smoothing factor for movement

    void LateUpdate()
    {
        // Get the headset's position, ignoring vertical tilt
        Vector3 headsetPosition = headset.position;

        // Calculate the forward direction on the horizontal plane
        Vector3 forwardDirection = new Vector3(headset.forward.x, 0, headset.forward.z).normalized;

        // Calculate the target position for the belt
        Vector3 targetPosition = headsetPosition + forwardDirection * offset.z + Vector3.up * offset.y;

        // Smoothly move the belt to the target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothFactor);

        // Rotate the belt to face the same forward direction
        transform.rotation = Quaternion.LookRotation(forwardDirection);
    }
}
