using UnityEngine;

public class StoryProgressionTrigger : MonoBehaviour
{
    [SerializeField] private SimonSaysMinigameManager simonSaysManager;

    private void Start()
    {
        simonSaysManager.OnSuccess += OnThirdShrineComplete;
    }

    private void OnThirdShrineComplete()
    {
        LetterManager.Instance.PlayOutroLetter();
    }

    private void OnDestroy()
    {
        simonSaysManager.OnSuccess -= OnThirdShrineComplete;
    }
}