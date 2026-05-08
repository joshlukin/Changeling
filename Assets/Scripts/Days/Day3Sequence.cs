using System.Collections;
using UnityEngine;

/// <summary>
/// Drives the Day 2 story sequence.
/// Attach to an empty GameObject. Activated by Day1Sequence at end of Day 1.
/// 
/// DAY 2 FLOW:
/// 1. Wake up — piano music plays if relationship >= 10
/// 2. Shaman visit 2
/// 3. Return home — interact with daughter (evening)
/// 4. Make dinner, place food, call Siofra
/// 5. Health monitor — health dropped to 81%, player alarmed
/// 6. End of day
/// </summary>
public class Day3Sequence : MonoBehaviour
{
    [Tooltip("Evening dinner counter — disabled until after shaman return.")]
    public DinnerCounterInteractable dinnerCounter;

    [Tooltip("Evening dinner table — disabled until dinner is made.")]
    public DinnerTableInteractable dinnerTable;
    
    private const float PollInterval = 0.3f;
    public Sprite bedroomArt;
    public Day4Sequence day4Sequence;

    [Header("Wwise Events")]
    [Tooltip("Plays at the very start of Day 3 when the first dialogue box appears.")]
    public AK.Wwise.Event dayStartFirstDialogueEvent;

    [Tooltip("Plays right after the line 'Must be dad'.")]
    public AK.Wwise.Event afterMustBeDadEvent;

    [Tooltip("Pauses or ducks audio during the Day 3 to Day 4 transition.")]
    public AK.Wwise.Event endOfDayPauseEvent;

    [Tooltip("Optional: stops a specific SFX that should not continue into Day 4.")]
    public AK.Wwise.Event stopSfxBeforeNextDayEvent;

    [Header("Audio Post Target")]
    public GameObject audioPostTarget;

    [Tooltip("The GameObject that originally posted/plays the SFX you want to stop.")]
    public GameObject sfxToStopAudioObject;

    // -------------------------------------------------------
    // Entry point — called by Day2Sequence.EndOfDay()
    // -------------------------------------------------------

    public void StartDay3()
    {
        // Reset day-specific flags while preserving cross-day state
        ResetDayFlags();
        StartCoroutine(MorningSequence());
    }

    private void ResetDayFlags()
    {
        // Clear day-specific flags so interactables work fresh
        DayManager.Instance.SetFlag("shaman_visited_today", false);
        DayManager.Instance.SetFlag("shaman_return_complete", false);
        DayManager.Instance.SetFlag("dinner_made", false);
        DayManager.Instance.SetFlag("dinner_placed", false);
        DayManager.Instance.SetFlag("health_checked", false);
        DayManager.Instance.SetFlag("piano_visited_evening", false);
        
        // Keep dinner objects disabled until evening
        if (dinnerCounter != null) dinnerCounter.gameObject.SetActive(false);
        if (dinnerTable != null) dinnerTable.gameObject.SetActive(false);
    }

    // -------------------------------------------------------
    // Morning
    // -------------------------------------------------------

    IEnumerator MorningSequence()
    {
        // 1. Fade in
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

        // 3. Fade in — player "opens eyes" into the bedroom art
        yield return FadeManager.Instance.FadeIn(1.5f);

        // 4. Hide the [E] prompt — dialogue advances lines instead
        ScenePanelManager.Instance.SetContinuePromptVisible(false);
        
        ScenePanelManager.Instance.ClosePanel();
        ObjectiveManager.Instance.SetObjective("Talk to your husband");

        if (dayStartFirstDialogueEvent != null)
        {
            dayStartFirstDialogueEvent.Post(audioPostTarget != null ? audioPostTarget : gameObject);
        }

        // First father interaction — part 1
        yield return PlayAndWait(DialogueSequence.Create(
            new DialogueLine("", "*There's knocking at the door*"),
            new DialogueLine("Siofra", "Must be dad")
        ));

        if (afterMustBeDadEvent != null)
        {
            afterMustBeDadEvent.Post(audioPostTarget != null ? audioPostTarget : gameObject);
        }

        // First father interaction — part 2
        yield return PlayAndWait(DialogueSequence.Create(
            new DialogueLine("Your husband", "It’s really about time you give me a key…"),
            new DialogueLine("You", "…"),
            new DialogueLine("You", "I don’t need you visiting Siofra, you’re a terrible influence in her life!”"),
            new DialogueLine("Your husband", "*sigh* ... We talked about th-"),
            new DialogueLine("You", "I don’t need you plaguing her mind with your dangerous ideas, can’t you see—you’re harming her!"),
            new DialogueLine("Your husband", "ook, I don’t have time for this right now. Whether you like it or not, I have mandated visitation rights and it’s my weekend. Hand her over"),
            new DialogueLine("You", "… fine. Goodbye, Sio.")
        ));

        yield return PlayAndWait(DialogueSequence.Create(
            new DialogueLine("You", "Practice piano for a bit, I need to talk to your dad"),
            new DialogueLine("Siofra", "Yes mother"),
            new DialogueLine("You", "Listen, the shaman is saying it's getting really bad."),
            new DialogueLine("Your husband", "Who now? What’s getting bad, do you even hear yourself!?"),
            new DialogueLine("You", "Our daughter! She's sick! We need to DO something! YOU need to do something! Do you know anything about your own daughter?"),
            new DialogueLine("Your husband", "Wow, hey! I think you should calm down, alright, let’s just-"),
            new DialogueLine("You", "You don't care about her at all, do you?"),
            new DialogueLine("Your husband", "Just tell me what's going on!"),
            new DialogueLine("You", "She's slowly turning into a..a horrible monster! She’s going to die, she's going to die-"),
            new DialogueLine("Your husband", "Hey… hey! Look at me and just breathe! Breathe…"),
            new DialogueLine("Your husband", "I’ll take care of her, okay?"),
            new DialogueLine("Your husband", "You need to get some rest, I’ll make sure nothing happens to her."),
            new DialogueLine("You", "Okay… okay…"),
            new DialogueLine("You", "Please, just promise me you’ll keep an eye on her at all times… and be on the lookout for any strange signs or abnormal changes in her behavior."),
            new DialogueLine("Your husband", "I will, I promise. I’ll let you know if anything happens."),
            new DialogueLine("You", "Okay………. okay…."),
            new DialogueLine("Your husband", "Hey, just wait here, okay? I’ll go get Sio, you should lay down and get some rest, you need it."),
            new DialogueLine("You", "….. alright")
        ));
        
        ObjectiveManager.Instance.SetObjective("Go to bed for the day");
        yield return StartCoroutine(EndOfDay());
    }

    // -------------------------------------------------------
    // End of day
    // -------------------------------------------------------

    IEnumerator EndOfDay()
    {
        ObjectiveManager.Instance.ClearObjective();

        // Health dropped — player alarmed
        // DayManager.daughterHealth should be set to 81 by remedy logic or
        // automatically decremented here if no remedy was used effectively
        DayManager.Instance.SetDaughterHealth(81f);

        yield return PlayAndWait(DialogueSequence.Create(
            new DialogueLine("You (to yourself)", "I hope she doesn't get worse when she's with her dad...")
        ));

        if (endOfDayPauseEvent != null)
        {
            endOfDayPauseEvent.Post(audioPostTarget != null ? audioPostTarget : gameObject);
        }

        if (stopSfxBeforeNextDayEvent != null && sfxToStopAudioObject != null)
        {
            stopSfxBeforeNextDayEvent.Post(sfxToStopAudioObject);
        }

        yield return FadeManager.Instance.FadeOut(2.5f);

        // Stay black briefly so the pause/stop has time to be felt
        yield return new WaitForSeconds(2.0f);

        DayManager.Instance.AdvanceDay();

        if (day4Sequence != null)
        {
            Debug.Log("[Day3Sequence] Day 3 complete.");
            gameObject.SetActive(false);
            day4Sequence.StartDay4();
        }
        else
        {
            Debug.LogWarning("[Day3Sequence] No Day4Sequence assigned.");
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