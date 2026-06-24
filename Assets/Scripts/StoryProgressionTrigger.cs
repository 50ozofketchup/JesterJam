using UnityEngine;
public class StoryProgressionTrigger : MonoBehaviour
{
    [SerializeField] private SimonSaysMinigameManager simonSaysManager;

    private void Start()
    {
        simonSaysManager.OnSuccess += OnThirdShrineComplete;
    }

    // triggered once the third shrine's Simon Says minigame is solved, advances to the outro letter
    private void OnThirdShrineComplete()
    {
        LetterManager.Instance.PlayOutroLetter();
    }

    private void OnDestroy()
    {
        simonSaysManager.OnSuccess -= OnThirdShrineComplete;
    }
}