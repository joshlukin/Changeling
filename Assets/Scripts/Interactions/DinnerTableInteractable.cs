using UnityEngine;

/// <summary>
/// Evening version of the table — calls Siofra for dinner.
/// Starts disabled. Day1Sequence enables it after dinner_made is set.
/// </summary>
public class DinnerTableInteractable : Interactable
{
    [Header("Dinner Table")]
    public Sprite tableArt;
    public GameObject plateProp;

    private void Start()
    {
        repeatable = false;
        gameObject.SetActive(false);
    }

    protected override bool CanInteract()
    {
        return DayManager.Instance.GetFlag("dinner_made")
               && !DayManager.Instance.GetFlag("dinner_placed");
    }

    protected override void OnInteract()
    {
        ScenePanelManager.Instance.OpenPanel(
            tableArt,
            "Dining Table",
            onClose: () =>
            {
                DayManager.Instance.SetFlag("dinner_placed");

                if (plateProp != null)
                    plateProp.SetActive(true);

                ScenePanelManager.Instance.LockPlayer(true);

                DialogueManager.Instance.PlayDialogue(
                    DialogueSequence.Create(
                        new DialogueLine("", "Dinner's ready!"),
                        new DialogueLine("Siofra", "Steamed eggs, my favorite!")
                    ),
                    onComplete: () => ScenePanelManager.Instance.LockPlayer(false)
                );
            }
        );
    }
}
