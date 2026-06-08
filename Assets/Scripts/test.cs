using UnityEngine;

public class TestTrigger : MonoBehaviour
{
    [SerializeField] private TracingMinigameManager tracingManager;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
            tracingManager.Open();
    }
}