using UnityEngine;

public class KitchenCounterInteractable : Interactable
{
    [Header("Kitchen")]
    public Sprite kitchenArt;

    [Header("Wwise Events")]
    public AK.Wwise.Event cookingStartEvent;
    public AK.Wwise.Event cookingExitEvent;

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
                cookingExitEvent?.Post(gameObject);

                DayManager.Instance.SetFlag("brunch_made");
                DayManager.Instance.SetFlag("kitchen_objective_complete");
            },
            onPanelReady: () =>
            {
                DialogueManager.Instance.PlayDialogue(
                    DialogueSequence.Create(
                        new DialogueLine("", "I should make something to eat.")
                    ),
                    onComplete: () =>
                    {
                        cookingStartEvent?.Post(gameObject);
                    }
                );
            }
        );
    }
}