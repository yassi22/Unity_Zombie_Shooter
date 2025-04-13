using UnityEngine;

public class OnEnterDisableCollision : MonoBehaviour
{
    public Rigidbody toDisableCollision;

    private void OnTriggerEnter(Collider other)
    {
        toDisableCollision.detectCollisions = false;
    }

    private void OnTriggerExit(Collider other)
    {
        toDisableCollision.detectCollisions = true;
    }
}
