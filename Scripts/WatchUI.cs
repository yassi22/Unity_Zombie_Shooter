using TMPro;
using UnityEngine;

public class WatchUI : MonoBehaviour
{
    public TextMeshProUGUI[] watchUi;
    private int textIndex = 0;

    private void Start()
    {
        if (watchUi == null || watchUi.Length == 0)
        {
            Debug.LogError("Watch UI array is not assigned or empty.");
            return;
        }

        // Ensure only the first UI element is active at start
        UpdateUI();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Gun") && !other.CompareTag("ControllerPointer")) return;

        int previousIndex = textIndex;
        textIndex = (textIndex + 1) % watchUi.Length; // Cycle using modulo

        watchUi[previousIndex].enabled = false;
        watchUi[textIndex].enabled = true;
    }

    private void UpdateUI()
    {
        for (int i = 0; i < watchUi.Length; i++)
        {
            watchUi[i].enabled = (i == textIndex);
        }
    }
}
