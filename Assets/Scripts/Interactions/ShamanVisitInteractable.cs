using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShamanVisitInteractable : Interactable
{
    [Header("Shaman Visit")]
    public Sprite shamanArt;
    public Inventory shamanInventory;
    public Inventory playerInventory;

    [Header("Wwise Events")]
    public AK.Wwise.Event shamanArtStartEvent;
    public AK.Wwise.Event shamanArtStopEvent;
    public AK.Wwise.Event remedyButtonClickEvent;

    [Header("Wwise States")]
    public AK.Wwise.State normalGameplayState;
    public AK.Wwise.State shamanVisitState;

    [Header("Audio Post Target")]
    public GameObject playerObject;

    [Header("Remedy Choice UI")]
    [Tooltip("Root GameObject containing the two remedy choice buttons. Child of PanelRoot.")]
    public GameObject remedyChoiceRoot;
    public Button remedyButtonA;
    public Button remedyButtonB;
    public TextMeshProUGUI remedyALabel;
    public TextMeshProUGUI remedyBLabel;

    [Header("Fallback Key Choice")]
    [Tooltip("If buttons don't work, fallback key selects remedy A (Music Box).")]
    public KeyCode remedyAKey = KeyCode.Alpha1;
    [Tooltip("Fallback key selects remedy B (Incense).")]
    public KeyCode remedyBKey = KeyCode.Alpha2;
    [Tooltip("Optional: show key hints in the label text.")]
    public bool showKeyHints = true;

    private bool _remedyPurchasedToday = false;
    private bool _panelClosedByChoice = false;
    private bool _awaitingRemedyChoice = false;

    protected override bool CanInteract()
    {
        return !DayManager.Instance.GetFlag("shaman_visited_today");
    }

    protected override void Update()
    {
        base.Update();

        // Fallback keyboard input for remedy choice
        // Works regardless of button click issues
        if (_awaitingRemedyChoice)
        {
            if (Input.GetKeyDown(remedyAKey))
                OnRemedyChosen(0);
            else if (Input.GetKeyDown(remedyBKey))
                OnRemedyChosen(1);
        }
    }

    protected override void OnInteract()
    {
        _remedyPurchasedToday = false;
        _panelClosedByChoice = false;
        _awaitingRemedyChoice = false;

        SetupRemedyButtons();
        ScenePanelManager.Instance.SetCanCloseWithKey(false);

        // Switch Wwise into the Shaman Visit mix/state.
        // This should duck or mute world room sounds without pausing/resuming 3D emitters.
        if (shamanVisitState != null)
        {
            shamanVisitState.SetValue();
        }

        // Play shaman visit start sound on the player.
        if (shamanArtStartEvent != null)
        {
            shamanArtStartEvent.Post(playerObject != null ? playerObject : gameObject);
        }

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
        _awaitingRemedyChoice = true;

        if (remedyChoiceRoot != null)
            remedyChoiceRoot.SetActive(true);
    }

    private void HideRemedyChoice()
    {
        _awaitingRemedyChoice = false;

        if (remedyChoiceRoot != null)
            remedyChoiceRoot.SetActive(false);
    }

    private void SetupRemedyButtons()
    {
        HideRemedyChoice();

        var items = shamanInventory != null ? shamanInventory.items : null;

        string nameA = (items != null && items.Count >= 1) ? items[0].itemName : "Empty";
        string nameB = (items != null && items.Count >= 2) ? items[1].itemName : "Empty";

        // Include key hints in label if enabled
        if (remedyALabel != null)
            remedyALabel.text = showKeyHints ? $"[1] {nameA}" : nameA;

        if (remedyBLabel != null)
            remedyBLabel.text = showKeyHints ? $"[2] {nameB}" : nameB;
        
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

        if (remedyButtonClickEvent != null)
        {
            remedyButtonClickEvent.Post(playerObject != null ? playerObject : gameObject);
        }

        Item chosen = shamanInventory.items[index];
        shamanInventory.items.RemoveAt(index);

        if (playerInventory != null)
            playerInventory.AddItem(chosen);

        _remedyPurchasedToday = true;
        _panelClosedByChoice = true;
        HideRemedyChoice();

        DialogueManager.Instance.PlayDialogue(
            DialogueSequence.Create(
                new DialogueLine("Shaman", $"The {chosen.itemName} will help her."),
                new DialogueLine("Shaman", "Use it wisely.")
            ),
            onComplete: () =>
            {
                ScenePanelManager.Instance.SetCanCloseWithKey(true);
                ScenePanelManager.Instance.ClosePanel();
            }
        );
    }

    private void OnShamanPanelClosed()
    {
        // Play shaman visit stop sound on the player.
        if (shamanArtStopEvent != null)
        {
            shamanArtStopEvent.Post(playerObject != null ? playerObject : gameObject);
        }

        // Return Wwise to the normal gameplay mix/state.
        // This restores the world mix without relocating 3D sounds.
        if (normalGameplayState != null)
        {
            normalGameplayState.SetValue();
        }

        ScenePanelManager.Instance.SetCanCloseWithKey(true);
        HideRemedyChoice();

        DayManager.Instance.SetFlag("shaman_visited_today");

        if (_panelClosedByChoice)
        {
            ScenePanelManager.Instance.LockPlayer(true);

            DialogueManager.Instance.PlayDialogue(
                DialogueSequence.Create(
                    new DialogueLine("", "It's getting late... I should check on Sio.")
                ),
                onComplete: () =>
                {
                    ScenePanelManager.Instance.LockPlayer(false);
                    DayManager.Instance.SetFlag("shaman_return_complete");
                }
            );
        }
        else
        {
            DayManager.Instance.SetFlag("shaman_return_complete");
        }
    }
}