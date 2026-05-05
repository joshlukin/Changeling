using UnityEngine;

/// <summary>
/// Evening version of the kitchen counter — makes dinner.
/// Starts disabled. Day1Sequence enables it after the evening piano visit.
/// </summary>
public class DinnerCounterInteractable : Interactable
{
    [Header("Dinner")]
    public Sprite kitchenArt;

    private void Start()
    {
        repeatable = false;
    }

    protected override bool CanInteract()
    {
        return !DayManager.Instance.GetFlag("dinner_made");
    }

    protected override void OnInteract()
    {
        ScenePanelManager.Instance.OpenPanelWithCallback(
            kitchenArt,
            "Kitchen",
            onClose: null,
            onPanelReady: () =>
            {
                DayManager.Instance.SetFlag("dinner_made");

                DialogueManager.Instance.PlayDialogue(
                    DialogueSequence.Create(
                        new DialogueLine("", "Steamed eggs. Her favorite.")
                    )
                );
            }
        );
    }
}
