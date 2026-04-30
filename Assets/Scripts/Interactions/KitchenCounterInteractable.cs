using UnityEngine;

public class KitchenCounterInteractable : Interactable
{
    [Header("Kitchen")]
    public Sprite kitchenArt;

    private void Start()
    {
        repeatable = false;
    }

    protected override bool CanInteract()
    {
        return !DayManager.Instance.GetFlag("brunch_made");
    }

    protected override void OnInteract()
    {
        ScenePanelManager.Instance.OpenPanelWithCallback(
            kitchenArt,
            "Kitchen",
            onClose: () =>
            {
                DayManager.Instance.SetFlag("brunch_made");
                DayManager.Instance.SetFlag("kitchen_objective_complete");
            },
            onPanelReady: () =>
            {
                DialogueManager.Instance.PlayDialogue(
                    DialogueSequence.Create(
                        new DialogueLine("", "I should make something to eat.")
                    )
                );
            }
        );
    }
}