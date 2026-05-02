using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShamanVisitInteractable : Interactable
{
    [Header("Shaman Visit")]
    public Sprite shamanArt;
    public Inventory shamanInventory;
    public Inventory playerInventory;

    [Header("Remedy Choice UI")]
    public GameObject remedyChoiceRoot;
    public Button remedyButtonA;
    public Button remedyButtonB;
    public TextMeshProUGUI remedyALabel;
    public TextMeshProUGUI remedyBLabel;

    private bool _remedyPurchasedToday = false;
    // Tracks whether the panel was closed intentionally after a remedy choice,
    // vs closed by the player pressing E before choosing
    private bool _panelClosedByChoice = false;

    protected override bool CanInteract()
    {
        return !DayManager.Instance.GetFlag("shaman_visited_today");
    }

    protected override void OnInteract()
    {
        // Reset purchase state for this visit
        _remedyPurchasedToday = false;
    
        // Set up buttons BEFORE opening (don't hide inside SetupRemedyButtons)
        SetupRemedyButtons(); // remove the HideRemedyChoice() call from inside this

        ScenePanelManager.Instance.OpenPanelWithCallback(
            shamanArt,
            "The Shaman",
            onClose: OnShamanPanelClosed,
            onPanelReady: () =>
            {
                DialogueManager.Instance.PlayDialogue(
                    DialogueSequence.Create(
                        new DialogueLine("Shaman", "Ah, you've come again."),
                        new DialogueLine("Shaman", "What will it be today?")
                    ),
                    onComplete: ShowRemedyChoice  // buttons show AFTER dialogue
                );
            }
        );
    }


    private void ShowRemedyChoice()
    {
        Debug.Log("$[Shaman] Show remedy choice");
        if (remedyChoiceRoot != null)
            remedyChoiceRoot.SetActive(true);
    }

    private void HideRemedyChoice()
    {
        Debug.Log("$[Shaman] Hide remedy choice");
        if (remedyChoiceRoot != null)
            remedyChoiceRoot.SetActive(false);
    }

    private void SetupRemedyButtons()
    {
        // REMOVED: HideRemedyChoice() from here — caller controls visibility
        var items = shamanInventory != null ? shamanInventory.items : null;

        if (remedyALabel != null)
            remedyALabel.text = (items != null && items.Count >= 1) ? items[0].itemName : "Empty";
        if (remedyBLabel != null)
            remedyBLabel.text = (items != null && items.Count >= 2) ? items[1].itemName : "Empty";

        if (remedyButtonA != null)
        {
            remedyButtonA.onClick.RemoveAllListeners();
            remedyButtonA.onClick.AddListener(() => OnRemedyChosen(0));
        }
        if (remedyButtonB != null)
        {
            remedyButtonB.onClick.RemoveAllListeners();
            remedyButtonB.onClick.AddListener(() => OnRemedyChosen(1));
        }
    
        // Start hidden — ShowRemedyChoice will reveal after dialogue
        HideRemedyChoice();
    }

    private void OnRemedyChosen(int index)
    {
        Debug.Log($"[Shaman] Remedy choice: {index}");
        if (_remedyPurchasedToday) return;
        if (shamanInventory == null || shamanInventory.items.Count <= index) return;

        Item chosen = shamanInventory.items[index];
        shamanInventory.items.RemoveAt(index);

        if (playerInventory != null)
            playerInventory.AddItem(chosen);

        _remedyPurchasedToday = true;
        _panelClosedByChoice = true;
        HideRemedyChoice();

        Debug.Log($"[Shaman] Player chose: {chosen.itemName}");

        DialogueManager.Instance.PlayDialogue(
            DialogueSequence.Create(
                new DialogueLine("Shaman", $"The {chosen.itemName} will help her."),
                new DialogueLine("Shaman", "Use it wisely.")
            ),
            onComplete: () => ScenePanelManager.Instance.ClosePanel()
        );
    }

    private void OnShamanPanelClosed()
    {
        DayManager.Instance.SetFlag("shaman_visited_today");
        HideRemedyChoice();

        ScenePanelManager.Instance.LockPlayer(true);

        DialogueManager.Instance.PlayDialogue(
            DialogueSequence.Create(
                new DialogueLine("", "It's getting late... I should check on Sio.")
            ),
            onComplete: () => ScenePanelManager.Instance.LockPlayer(false)
        );
    }
}