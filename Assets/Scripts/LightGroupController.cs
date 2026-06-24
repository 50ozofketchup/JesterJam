using UnityEngine;

public class LightGroupController : MonoBehaviour
{
    [SerializeField] private GameObject tracingLights;
    [SerializeField] private GameObject rotationLights;
    [SerializeField] private GameObject simonSaysLights;

    [SerializeField] private TracingMinigameManager tracingManager;
    [SerializeField] private RotationMinigameManager rotationManager;
    [SerializeField] private SimonSaysMinigameManager simonSaysManager;

    private void Start()
    {
        // deactivates a groupf of lights
        tracingManager.OnSuccess += () => tracingLights.SetActive(false);
        rotationManager.OnSuccess += () => rotationLights.SetActive(false);
        simonSaysManager.OnSuccess += () => simonSaysLights.SetActive(false);
    }
}