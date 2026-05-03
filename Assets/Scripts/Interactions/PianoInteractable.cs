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
        new DialogueLine("", "..."),
        new DialogueLine("", "...")
    };

    private static readonly DialogueLine[] _homeworkLines = new DialogueLine[]
    {
        new DialogueLine("", "Oh, you're still practicing piano."),
        new DialogueLine("", "Don't forget to do your homework as well.")
    };

    private static readonly DialogueLine[] _day1EveningLines = new DialogueLine[]
    {
        new DialogueLine("", "Ask what piano song she's practicing."),
        new DialogueLine("Siofra", "It's a new one. Does it sound better?"),
        new DialogueLine("", "...A lot better, actually.")
    };

    private static readonly DialogueLine[] _day2EveningLines = new DialogueLine[]
    {
        new DialogueLine("", "Did you manage to finish your homework?"),
        new DialogueLine("Siofra", "It wasn't that hard."),
        new DialogueLine("", "*sigh*"),
        new DialogueLine("", "I'm sure you'll get it eventually.")
    };

    protected override void OnInteract()
    {
        _interactCount++;

        bool shamanVisitedToday = DayManager.Instance.GetFlag("shaman_visited_today");

        // Set morning or evening flag for Day1Sequence / Day2Sequence to unblock
        if (!shamanVisitedToday)
        {
            DayManager.Instance.SetFlag("piano_visited_morning");
            Debug.Log("[Piano] Set piano_visited_morning");
        }
        else
        {
            DayManager.Instance.SetFlag("piano_visited_evening");
            Debug.Log("[Piano] Set piano_visited_evening");

            // Evening visit grants relationship bonus
            DayManager.Instance.AddRelationship(10);
        }

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
        bool shamanVisitedToday = DayManager.Instance.GetFlag("shaman_visited_today");
        int day = DayManager.Instance.currentDay;

        if (shamanVisitedToday)
            return day >= 2 ? _day2EveningLines : _day1EveningLines;

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

        if (homeworkProp != null) homeworkProp.SetActive(true);
        if (homeworkPlacementPrompt != null) homeworkPlacementPrompt.SetActive(false);

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