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
public class Day2Sequence : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Audio source that plays piano music on wake-up if relationship >= 10.")]
    public AudioSource pianoMusicSource;

    [Tooltip("Evening dinner counter — disabled until after shaman return.")]
    public DinnerCounterInteractable dinnerCounter;

    [Tooltip("Evening dinner table — disabled until dinner is made.")]
    public DinnerTableInteractable dinnerTable;

    [Tooltip("The shaman visit interactable — re-enabled for Day 2.")]
    public ShamanVisitInteractable shamanVisit;

    public Item day2NewItem;
    public GameObject shamanObj;

    private const float PollInterval = 0.3f;

    // -------------------------------------------------------
    // Entry point — called by Day1Sequence.EndOfDay()
    // -------------------------------------------------------

    public void StartDay2()
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

        // Re-enable shaman visit
        if (shamanVisit != null)
            shamanVisit.gameObject.SetActive(true);

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
        yield return FadeManager.Instance.FadeIn(1.5f);

        // 2. Wake up — piano music plays if relationship is high enough
        if (DayManager.Instance.relationshipStat >= 10)
        {
            if (pianoMusicSource != null)
                pianoMusicSource.Play();

            yield return PlayAndWait(DialogueSequence.Create(
                new DialogueLine("", "...That's Sio playing."),
                new DialogueLine("", "She's up early.")
            ));
        }
        else
        {
            yield return PlayAndWait(DialogueSequence.Create(
                new DialogueLine("", "...Another morning.")
            ));
        }

        // 3. Prompt shaman visit
        shamanObj.GetComponent<Inventory>().AddItem(day2NewItem);
        ObjectiveManager.Instance.SetObjective("Visit the Shaman.");
        yield return WaitForFlag("shaman_visited_today");
        yield return WaitForFlag("shaman_return_complete");

        yield return StartCoroutine(EveningSequence());
    }

    // -------------------------------------------------------
    // Evening
    // -------------------------------------------------------

    IEnumerator EveningSequence()
    {
        ObjectiveManager.Instance.SetObjective("Check on Siofra.");

        // Wait for evening piano visit
        yield return WaitForFlag("piano_visited_evening");

        // Unlock dinner
        if (dinnerCounter != null) dinnerCounter.gameObject.SetActive(true);

        ObjectiveManager.Instance.SetObjective("Make dinner.");
        yield return WaitForFlag("dinner_made_day2");

        if (dinnerTable != null) dinnerTable.gameObject.SetActive(true);

        ObjectiveManager.Instance.SetObjective("Call Siofra for dinner.");
        yield return WaitForFlag("dinner_placed_day2");
        

        // Health monitor
        ObjectiveManager.Instance.SetObjective("Check the health monitor.");
        yield return WaitForFlag("health_checked");

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
            new DialogueLine("", "81%..."),
            new DialogueLine("", "It's lower than before..."),
            new DialogueLine("", "I need to consult the Shaman about this tomorrow.")
        ));

        yield return FadeManager.Instance.FadeOut(1.5f);

        DayManager.Instance.AdvanceDay();

        // TODO: activate Day3Sequence here
        Debug.Log("[Day2Sequence] Day 2 complete.");
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