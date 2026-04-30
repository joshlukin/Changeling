using UnityEngine;

public class PianoInteractable : Interactable
{
    [Header("Piano")]
    public Sprite pianoArt;

    [Header("Homework")]
    public GameObject homeworkProp;
    public GameObject homeworkPlacementPrompt;
    public bool isHomeworkPlaced = false;

    private int _interactCount = 0;

    private static readonly DialogueLine[] _normalLines = new DialogueLine[]
    {
        new DialogueLine("Siofra", "Is it breakfast time?")
    };

    private static readonly DialogueLine[] _stareLines = new DialogueLine[]
    {
        new DialogueLine("Siofra", "..."),
        new DialogueLine("", "...")
    };

    private static readonly DialogueLine[] _homeworkLines = new DialogueLine[]
    {
        new DialogueLine("", "Oh, you're still practicing piano."),
        new DialogueLine("", "Don't forget to do your homework as well.")
    };

    protected override void OnInteract()
    {
        _interactCount++;
        DialogueLine[] lines = GetDialogueLines();

        ScenePanelManager.Instance.OpenPanelWithCallback(
            pianoArt,
            "Living Room",
            onClose: null,
            onPanelReady: () =>
            {
                UpdateHomeworkPromptVisibility();

                DialogueManager.Instance.PlayDialogue(
                    DialogueSequence.Create(lines)
                );
            }
        );
    }

    private DialogueLine[] GetDialogueLines()
    {
        if (_interactCount >= 4)
            return _stareLines;

        if (DayManager.Instance.GetFlag("kitchen_objective_complete") && !isHomeworkPlaced)
            return _homeworkLines;

        return _normalLines;
    }

    public void OnHomeworkPlaced()
    {
        if (isHomeworkPlaced) return;

        isHomeworkPlaced = true;
        DayManager.Instance.SetFlag("homework_placed");

        if (homeworkProp != null)
            homeworkProp.SetActive(true);

        if (homeworkPlacementPrompt != null)
            homeworkPlacementPrompt.SetActive(false);

        ScenePanelManager.Instance.ClosePanel();
    }

    private void UpdateHomeworkPromptVisibility()
    {
        if (homeworkPlacementPrompt == null) return;

        bool shouldShow = DayManager.Instance.GetFlag("kitchen_objective_complete")
                          && !isHomeworkPlaced;

        homeworkPlacementPrompt.SetActive(shouldShow);
    }
}