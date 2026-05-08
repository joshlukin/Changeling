using UnityEngine;

public class PianoInteractable : Interactable
{
    [Header("Piano")]
    public Sprite pianoArt;

    [Header("Wwise Events")]
    public AK.Wwise.Event pianoInterruptEvent;
    public AK.Wwise.Event pianoResumeEvent;

    [Header("Audio Post Target")]
    [Tooltip("The GameObject that is actually playing the piano loop. Usually the piano object with the AkAmbient / Wwise emitter.")]
    public GameObject pianoAudioObject;

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
        new DialogueLine("You", "..."),
        new DialogueLine("Siofra", "...")
    };

    private static readonly DialogueLine[] _homeworkLines = new DialogueLine[]
    {
        new DialogueLine("You", "Oh, you're still practicing piano."),
        new DialogueLine("You", "Don't forget to do your homework as well.")
    };

    private static readonly DialogueLine[] _day1EveningLines = new DialogueLine[]
    {
        new DialogueLine("You", "Ask what piano song she's practicing."),
        new DialogueLine("Siofra", "It's a new one. Does it sound better?"),
        new DialogueLine("You", "...A lot better, actually.")
    };

    private static readonly DialogueLine[] _day2EveningLines = new DialogueLine[]
    {
        new DialogueLine("You", "Did you manage to finish your homework?"),
        new DialogueLine("Siofra", "It wasn't that hard."),
        new DialogueLine("You", "*sigh*"),
        new DialogueLine("You", "I'm sure you'll get it eventually.")
    };

    private static readonly DialogueLine[] _day5EveningLines = new DialogueLine[]
    {
        new DialogueLine("Siofra", "Mom?"),
        new DialogueLine("You", "..."),
        new DialogueLine("Siofra", "... Is everything alright?"),
        new DialogueLine("You", "..."),
        new DialogueLine("Siofra", "Is dinner ready?"),
        new DialogueLine("You", "... I don’t have time to prepare dinner tonight."),
        new DialogueLine("Siofra", "Huh!? Did something happen? What are we going to eat? I can-"),
        new DialogueLine("You", "Go to your room."),
        new DialogueLine("Siofra", "Mom!? What’s going on? Did I do something wrong? Please, just-"),
        new DialogueLine("You", "Now."),
        new DialogueLine("Siofra", "..."),
        new DialogueLine("You", "..."),
        new DialogueLine("Siofra", "... yes, mom.")
    };

    protected override void OnInteract()
    {
        if (pianoInterruptEvent != null && pianoAudioObject != null)
        {
            pianoInterruptEvent.Post(pianoAudioObject);
        }

        _interactCount++;

        bool shamanVisitedToday = DayManager.Instance.GetFlag("shaman_visited_today");
        int day = DayManager.Instance.currentDay;

        // Days 3, 5, 6 have no shaman visit — piano always counts as evening
        bool isEveningDay = day == 3 || day == 5 || day == 6;

        if (!shamanVisitedToday && !isEveningDay)
        {
            DayManager.Instance.SetFlag("piano_visited_morning");
        }
        else
        {
            DayManager.Instance.SetFlag("piano_visited_evening");

            // Only grant relationship on days with a real evening interaction
            if (shamanVisitedToday)
                DayManager.Instance.AddRelationship(10);
        }

        DialogueLine[] lines = GetDialogueLines();

        ScenePanelManager.Instance.OpenPanelWithCallback(
            pianoArt,
            "Living Room",
            onClose: () =>
            {
                if (pianoResumeEvent != null && pianoAudioObject != null)
                {
                    pianoResumeEvent.Post(pianoAudioObject);
                }
            },
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
        int day = DayManager.Instance.currentDay;
        bool shamanVisitedToday = DayManager.Instance.GetFlag("shaman_visited_today");

        // Day-specific evening lines take priority
        if (day == 5) return _day5EveningLines;

        if (shamanVisitedToday)
            return day >= 2 ? _day2EveningLines : _day1EveningLines;

        if (_interactCount >= 4) return _stareLines;

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