using System.Collections;
using UnityEngine;
/// <summary>
/// Drives the Day 1 story sequence.
/// Attach to an empty GameObject in your scene.
/// Watches DayManager flags and advances objectives accordingly.
/// Does not force the player — it just updates prompts and unlocks beats.
///
/// DAY 1 FLOW:
/// 1. Wake up (fade in, opening monologue)
/// 2. Bedroom free roam (calendar available)
/// 3. Prompt: find the kitchen
/// 4. Kitchen: make brunch → sets kitchen_objective_complete
/// 5. Table: place food → sets food_placed
/// 6. Living room: check on Siofra (piano interaction)
/// 7. Prompt: visit the Shaman
/// 8. Shaman visit → sets shaman_visited_today
/// 9. Return home monologue → evening begins
/// 10. Evening: interact with Siofra again
/// 11. Dinner: make dinner → sets dinner_made
/// 12. Table: call Siofra → sets dinner_placed
/// 13. Health monitor → day ends
/// </summary>

public class Day1Sequence : MonoBehaviour
{
    [Header("References")] 
    public GameObject brunchTable;
    public GameObject dinnerCounter;
    public GameObject dinnerTable;
    public GameObject healthMonitorUI;
    public Day2Sequence day2Sequence;

    [Header("Opening")]
    [Tooltip("Background art shown during the opening bedroom sequence.")]
    public Sprite bedroomArt;

    private const float PollInterval = 0.3f;

    [Header("Wwise Events")]
    public AK.Wwise.Event bedroomWakeUpEvent;
    public AK.Wwise.Event bedroomLeaveEvent;
    [Header("Wwise End Of Day Events")]
    public AK.Wwise.Event endOfDayPauseEvent;
    public AK.Wwise.Event stopSfxBeforeDay2Event;

    [Header("Audio Post Targets")]
    public GameObject audioPostTarget;
    public GameObject sfxAudioObject;
        

    IEnumerator Start()
    {
        // Wait one frame for all singletons to initialise
        yield return null;
        yield return StartCoroutine(MorningSequence());
    }

    IEnumerator MorningSequence()
{
    // 1. Start fully black
    FadeManager.Instance.SnapToBlack();

    // 2. Open the bedroom art panel BEFORE fading in
    // This means the player never sees the 3D hallway during the intro.
    // OpenPanel is used (not WithCallback) so the [E] prompt shows —
    // but we immediately hide it since dialogue controls advancing here.
    ScenePanelManager.Instance.OpenPanel(
        bedroomArt,
        "Bedroom",
        onClose: null
    );

    bedroomWakeUpEvent?.Post(gameObject);

    // 3. Fade in — player "opens eyes" into the bedroom art
    yield return FadeManager.Instance.FadeIn(1.5f);

    // 4. Hide the [E] prompt — dialogue advances lines instead
    ScenePanelManager.Instance.SetContinuePromptVisible(false);

    // 5. Opening monologue plays over the bedroom art
    yield return PlayAndWait(DialogueSequence.Create(
        new DialogueLine("You (to yourself)", "...Another morning."),
        new DialogueLine("You (to yourself)", "I should check the calendar.")
    ));

    // 6. Close bedroom art — player now sees the 3D hallway
    ScenePanelManager.Instance.ClosePanel();

    bedroomLeaveEvent?.Post(gameObject);

    // Brief pause before objective appears
    yield return new WaitForSeconds(0.3f);

    // 7. Calendar objective
    ObjectiveManager.Instance.SetObjective("Check the calendar.");
    yield return WaitForFlag("has_read_calendar");

        ObjectiveManager.Instance.SetObjective("Find the kitchen and make brunch.");
        yield return WaitForFlag("brunch_made");

        ObjectiveManager.Instance.SetObjective("Place the food on the table.");
        yield return WaitForFlag("food_placed");

        yield return PlayAndWait(DialogueSequence.Create(
            new DialogueLine("", "I should check on Sio.")
        ));

        ObjectiveManager.Instance.SetObjective("Check on Siofra in the living room.");
        yield return WaitForFlag("piano_visited_morning");

        ObjectiveManager.Instance.SetObjective("Visit the Shaman.");
        yield return WaitForFlag("shaman_visited_today");
        yield return WaitForFlag("shaman_return_complete");

        brunchTable.GetComponent<TableInteractable>().plateProp.SetActive(false);
        Debug.Log("Starting evening sequence");
        yield return StartCoroutine(EveningSequence());
        
    }

    IEnumerator EveningSequence()
    {
        ObjectiveManager.Instance.SetObjective("Check on Siofra again.");
        yield return WaitForFlag("piano_visited_evening");
        
        dinnerCounter.SetActive(true);
        
        ObjectiveManager.Instance.SetObjective("Make dinner.");
        yield return WaitForFlag("dinner_made_day1");
        dinnerTable.SetActive(true);

        ObjectiveManager.Instance.SetObjective("Call Siofra for dinner.");
        yield return WaitForFlag("dinner_placed_day1");
        
        ObjectiveManager.Instance.SetObjective("Check the health monitor.");
        //yield return WaitForFlag("health_checked");

        yield return StartCoroutine(EndOfDay());
    }

    IEnumerator EndOfDay()
{
    ObjectiveManager.Instance.ClearObjective();

    DayManager.Instance.AdvanceDay();

    if (endOfDayPauseEvent != null)
    {
        endOfDayPauseEvent.Post(audioPostTarget != null ? audioPostTarget : gameObject);
    }

    if (stopSfxBeforeDay2Event != null && sfxAudioObject != null)
    {
        stopSfxBeforeDay2Event.Post(sfxAudioObject);
    }
    
    yield return FadeManager.Instance.FadeOut(2.5f);

    yield return new WaitForSeconds(2.0f);
    
    if (day2Sequence != null)
    {
        gameObject.SetActive(false);
        day2Sequence.StartDay2();
    }
    else
    {
        Debug.LogWarning("[Day1Sequence] No Day2Sequence assigned.");
    } 
}

    // -------------------------------------------------------
    // Helpers
    // -------------------------------------------------------

    private IEnumerator PlayAndWait(DialogueSequence sequence)
    {
        bool done = false;
        DialogueManager.Instance.PlayDialogue(sequence, onComplete: () => done = true);
        yield return new WaitUntil(() => done);
    }

    private IEnumerator WaitForFlag(string flag)
    {
        while (!DayManager.Instance.GetFlag(flag))
            yield return new WaitForSeconds(PollInterval);
    }
}