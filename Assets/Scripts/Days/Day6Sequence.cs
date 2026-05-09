using System.Collections;
using UnityEngine;

public class Day6Sequence : MonoBehaviour
{
    [Tooltip("Evening dinner counter — disabled until after shaman return.")]
    public DinnerCounterInteractable dinnerCounter;

    [Tooltip("Evening dinner table — disabled until dinner is made.")]
    public DinnerTableInteractable dinnerTable;
    
    private const float PollInterval = 0.3f;
    public Sprite bedroomArt;

    public Sprite killArt;
    [Header("Kill Choice")]
    public KillChoicePanel killChoicePanel;
    
    public Day7Sequence day7Sequence;
    
    

    [Header("Wwise Start Of Day Events")]
    [Tooltip("Re-engages / resumes audio after the Day 5 to Day 6 transition.")]
    public AK.Wwise.Event startOfDayResumeEvent;

    [Header("Wwise Relationship Events")]
    [Tooltip("Plays when relationshipStat >= 10.")]
    public AK.Wwise.Event relationshipBasedEvent;

    [Tooltip("The GameObject the relationship-based sound should emit from.")]
    public GameObject relationshipAudioObject;

    [Header("Wwise End Of Day Events")]
    [Tooltip("Pauses or ducks audio during the Day 6 to Day 7 transition/end transition.")]
    public AK.Wwise.Event endOfDayPauseEvent;

    [Tooltip("Optional: stops a specific SFX that should not continue after Day 6.")]
    public AK.Wwise.Event stopSfxBeforeNextDayEvent;

    [Tooltip("The GameObject that originally posted/plays the SFX you want to stop.")]
    public GameObject sfxToStopAudioObject;

    [Header("Audio Post Target")]
    public GameObject audioPostTarget;

    
    public void StartDay6()
    {
        // Reset day-specific flags while preserving cross-day state
        ResetDayFlags();
        StartCoroutine(EndOfDay());
    }

    private void ResetDayFlags()
    {
        DayManager.Instance.SetFlag("piano_visited_evening", false);
        
        // Keep dinner objects disabled until evening
        if (dinnerCounter != null) dinnerCounter.gameObject.SetActive(false);
        if (dinnerTable != null) dinnerTable.gameObject.SetActive(false);
    }

    // -------------------------------------------------------
    // End of day
    // -------------------------------------------------------

    IEnumerator EndOfDay()
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

        yield return FadeManager.Instance.FadeIn(1.5f);

        // Unlock dinner
        if (dinnerCounter != null)
            dinnerCounter.gameObject.SetActive(true);

        ObjectiveManager.Instance.SetObjective("Make dinner.");
        yield return WaitForFlag("dinner_made_day6");

        if (dinnerTable != null)
            dinnerTable.gameObject.SetActive(true);
        
        ObjectiveManager.Instance.SetObjective("Call Siofra for dinner.");
        yield return WaitForFlag("dinner_placed_day6");
        
        yield return StartCoroutine(KillingMoment());
    
        ObjectiveManager.Instance.ClearObjective();

        if (endOfDayPauseEvent != null)
        {
            endOfDayPauseEvent.Post(audioPostTarget != null ? audioPostTarget : gameObject);
        }

        if (stopSfxBeforeNextDayEvent != null && sfxToStopAudioObject != null)
        {
            stopSfxBeforeNextDayEvent.Post(sfxToStopAudioObject);
        }
        DayManager.Instance.ModifyDaughterHealth(-9f);
        yield return FadeManager.Instance.FadeOut(2.5f);
        
        // Stay black briefly so the pause/stop has time to be felt
        yield return new WaitForSeconds(2.0f);

        DayManager.Instance.AdvanceDay();

        if (day7Sequence != null)
        {
            Debug.Log("[Day6Sequence] Day 6 complete.");
            gameObject.SetActive(false);
            day7Sequence.StartDay7();
        }
        else
        {
            Debug.LogWarning("[Day6Sequence] No Day7Sequence assigned.");
        } 
    }
    private bool _killChoiceMade = false;
    private IEnumerator KillingMoment()
    {
        _killChoiceMade = false;
        yield return new WaitForSeconds(0.5f);

        killChoicePanel.Show();

        // The coroutine will freeze here until a button is clicked
        yield return new WaitUntil(() => _killChoiceMade);
    }
    public void OnKillChosen()
    {
        Debug.Log("[Day6] Kill chosen");
        _killChoiceMade = true;
        DayManager.Instance.SetFlag("chose_kill");
    }

    public void OnLetGoChosen()
    {
        Debug.Log("[Day6] Let Go chosen");
        _killChoiceMade = true;
    }
    
    // -------------------------------------------------------
    // Helpers
    // -------------------------------------------------------

    private IEnumerator PlayAndWait(DialogueSequence sequence)
    {
        bool done = false;
        yield return new WaitUntil(() => done);
    }

    private IEnumerator WaitForFlag(string flag)
    {
        while (!DayManager.Instance.GetFlag(flag))
            yield return new WaitForSeconds(PollInterval);
    }
}