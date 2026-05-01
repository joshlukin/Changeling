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

    protected override bool CanInteract()
    {
        return !DayManager.Instance.GetFlag("shaman_visited_today");
    }

    protected override void OnInteract()
    {
        SetupRemedyButtons();

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
                    onComplete: ShowRemedyChoice
                );
            }
        );
    }

    private void ShowRemedyChoice()
    {
        if (remedyChoiceRoot != null)
            remedyChoiceRoot.SetActive(true);
    }

    private void HideRemedyChoice()
    {
        if (remedyChoiceRoot != null)
            remedyChoiceRoot.SetActive(false);
    }

    private void SetupRemedyButtons()
    {
        HideRemedyChoice();

        var items = shamanInventory != null ? shamanInventory.items : null;
        
        //offers just the first two items for day 1 TODO: make applicable to all days, potential fix is to be controlled by DayManager
        if (remedyALabel != null)
            remedyALabel.text = (items != null && items.Count >= 1)
                ? items[0].itemName
                : "Empty";

        if (remedyBLabel != null)
            remedyBLabel.text = (items != null && items.Count >= 2)
                ? items[1].itemName
                : "Empty";

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
    }

    private void OnRemedyChosen(int index)
    {
        if (_remedyPurchasedToday) return;
        if (shamanInventory == null || shamanInventory.items.Count <= index) return;

        Item chosen = shamanInventory.items[index];
        shamanInventory.items.RemoveAt(index);

        if (playerInventory != null)
            playerInventory.AddItem(chosen);

        _remedyPurchasedToday = true;
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

        DialogueManager.Instance.PlayDialogue(
            DialogueSequence.Create(
                new DialogueLine("", "It's getting late... I should check on Sio.")
            )
        );
    }
}