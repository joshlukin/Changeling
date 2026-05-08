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
    //public Day7Sequence day7Sequence;

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
        // Wait for evening piano visit
        yield return WaitForFlag("piano_visited_evening");

        // Unlock dinner
        if (dinnerCounter != null) dinnerCounter.gameObject.SetActive(true);

        ObjectiveManager.Instance.SetObjective("Make dinner.");
        yield return WaitForFlag("dinner_made_day6");

        if (dinnerTable != null) dinnerTable.gameObject.SetActive(true);
        
        ObjectiveManager.Instance.SetObjective("Call Siofra for dinner.");
        yield return WaitForFlag("dinner_placed_day6");
        
        
        
        ObjectiveManager.Instance.ClearObjective();

        yield return FadeManager.Instance.FadeOut(1.5f);

        DayManager.Instance.AdvanceDay();
        // if (day7Sequence != null)
        // {
        //     Debug.Log("[Day6Sequence] Day 6 complete.");
        //     gameObject.SetActive(false);
        //     day7Sequence.StartDay7();
        // }
        // else
        // {
        //     Debug.LogWarning("[Day6Sequence] No Day7Sequence assigned.");
        // } 
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