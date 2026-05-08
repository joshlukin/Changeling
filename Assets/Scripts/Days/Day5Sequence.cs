using System.Collections;
using UnityEngine;

/// <summary>
/// Drives the Day 5 story sequence.
///
/// DAY 5 FLOW:
/// 1. Siofra attempts to talk to mother
/// 2. Mother becomes increasingly distant and obsessive
/// 3. Siofra is sent away without dinner
/// 4. Mother reassures herself she is "saving" her daughter
/// 5. End of day
/// </summary>
public class Day5Sequence : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Art shown during opening scene.")]
    public Sprite kitchenArt;

    public Day6Sequence day6Sequence;

    private const float PollInterval = 0.3f;

    [Header("Wwise Start Of Day Events")]
    [Tooltip("Re-engages / resumes audio after the Day 4 to Day 5 transition.")]
    public AK.Wwise.Event startOfDayResumeEvent;

    [Header("Wwise Relationship Events")]
    [Tooltip("Plays when relationshipStat >= 10.")]
    public AK.Wwise.Event relationshipBasedEvent;

    [Tooltip("The GameObject the relationship-based sound should emit from.")]
    public GameObject relationshipAudioObject;

    [Header("Wwise End Of Day Events")]
    [Tooltip("Pauses or ducks audio during the Day 5 to Day 6 transition.")]
    public AK.Wwise.Event endOfDayPauseEvent;

    [Tooltip("Optional: stops a specific SFX that should not continue into Day 6.")]
    public AK.Wwise.Event stopSfxBeforeNextDayEvent;

    [Tooltip("The GameObject that originally posted/plays the SFX you want to stop.")]
    public GameObject sfxToStopAudioObject;

    [Header("Audio Post Target")]
    public GameObject audioPostTarget;

    // -------------------------------------------------------
    // Entry Point
    // -------------------------------------------------------

    public void StartDay5()
    {
        ResetDayFlags();
        StartCoroutine(Day5Opening());
    }

    private void ResetDayFlags()
    {
        DayManager.Instance.SetFlag("talked_to_siofra_day5", false);
        DayManager.Instance.SetFlag("siofra_left_kitchen_day5", false);
        DayManager.Instance.SetFlag("piano_visited_evening", false);
    }

    // -------------------------------------------------------
    // Opening Scene
    // -------------------------------------------------------

    IEnumerator Day5Opening()
    {
        FadeManager.Instance.SnapToBlack();

        if (startOfDayResumeEvent != null)
        {
            startOfDayResumeEvent.Post(audioPostTarget != null ? audioPostTarget : gameObject);
        }

        if (DayManager.Instance.relationshipStat >= 10)
        {
            if (relationshipBasedEvent != null)
            {
                relationshipBasedEvent.Post(
                    relationshipAudioObject != null
                        ? relationshipAudioObject
                        : audioPostTarget != null
                            ? audioPostTarget
                            : gameObject
                );
            }
        }

        ScenePanelManager.Instance.OpenPanel(
            kitchenArt,
            "Kitchen",
            onClose: null
        );

        yield return FadeManager.Instance.FadeIn(1.5f);

        ScenePanelManager.Instance.SetContinuePromptVisible(false);

        ObjectiveManager.Instance.SetObjective("See what Siofra wants to talk about.");
    
        yield return WaitForFlag("piano_visited_evening");

        ObjectiveManager.Instance.SetObjective("Wait for Siofra to leave.");
        
        yield return PlayAndWait(DialogueSequence.Create(
            new DialogueLine("You", "I will save you, don’t worry."),
            new DialogueLine("You", "I will definitely save you.")
        ));

        yield return StartCoroutine(EndOfDay());
    }

    // -------------------------------------------------------
    // End Of Day
    // -------------------------------------------------------

    IEnumerator EndOfDay()
    {
        ObjectiveManager.Instance.ClearObjective();

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

        if (day6Sequence != null)
        {
            gameObject.SetActive(false);
            day6Sequence.StartDay6();
        }
        else
        {
            Debug.LogWarning("[Day5Sequence] No Day6Sequence assigned.");
        }

        Debug.Log("[Day5Sequence] Day 5 complete.");
    }

    // -------------------------------------------------------
    // Helpers
    // -------------------------------------------------------

    private IEnumerator PlayAndWait(DialogueSequence sequence)
    {
        bool done = false;

        DialogueManager.Instance.PlayDialogue(
            sequence,
            onComplete: () => done = true
        );

        yield return new WaitUntil(() => done);
    }

    private IEnumerator WaitForFlag(string flag)
    {
        while (!DayManager.Instance.GetFlag(flag))
            yield return new WaitForSeconds(PollInterval);
    }
}