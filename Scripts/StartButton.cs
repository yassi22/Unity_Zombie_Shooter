using UnityEngine;

public class StartButton : MonoBehaviour
{
    public GameObject UiPanel;
    public GameObject GunTable;
    public GameObject Button;

    private void OnTriggerEnter(Collider other)
    {
        if (UiPanel != null)
        {
            UiPanel.SetActive(false);
        }

        if (GunTable != null)
        {
            GunTable.SetActive(false);
        }

        FindAnyObjectByType<EnemySpawner>().StartGame();

        if (Button != null)
        {
            Button.SetActive(false);
        }
        
    }
}
