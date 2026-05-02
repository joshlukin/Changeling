using UnityEngine;

public class TableInteractable : Interactable
{
    [Header("Table")]
    public Sprite tableArt;

    [Tooltip("The food plate prop that appears on the table after placement.")]
    public GameObject plateProp;

    private void Start()
    {
        repeatable = false;
    }

    protected override bool CanInteract()
    {
        // Only available after brunch is made
        return DayManager.Instance.GetFlag("brunch_made")
               && !DayManager.Instance.GetFlag("food_placed");
    }

    protected override void OnInteract()
    {
        ScenePanelManager.Instance.OpenPanelWithCallback(
            tableArt,
            "Dining Table",
            onClose: () =>
            {
                DayManager.Instance.SetFlag("food_placed");

                if (plateProp != null)
                    plateProp.SetActive(true);
                
            },
            onPanelReady: () =>
            {
                DialogueManager.Instance.PlayDialogue(
                    DialogueSequence.Create(
                        new DialogueLine("", "There. Food's on the table.")
                    )
                );
            }
        );
    }
}