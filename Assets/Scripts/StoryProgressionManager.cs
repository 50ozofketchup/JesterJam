using UnityEngine;

public class StoryProgressionTrigger : MonoBehaviour
{
    [SerializeField] private RotationMinigameManager rotationManager;

    private void Start()
    {
        rotationManager.OnSuccess += OnThirdShrineComplete;
    }

    private void OnThirdShrineComplete()
    {
        LetterManager.Instance.PlayOutroLetter();
    }

    private void OnDestroy()
    {
        rotationManager.OnSuccess -= OnThirdShrineComplete;
    }
}