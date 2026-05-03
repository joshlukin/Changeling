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
    [Tooltip("The evening version of the kitchen counter interactable (dinner).")]
    public DinnerCounterInteractable dinnerCounter;

    [Tooltip("The evening version of the table interactable (call Siofra).")]
    public DinnerTableInteractable dinnerTable;

    [Tooltip("Health monitor UI root — shown at end of day.")]
    public GameObject healthMonitorUI;

    // Poll interval — checks flag state this often
    private const float PollInterval = 0.3f;

    // -------------------------------------------------------
    // Start
    // -------------------------------------------------------

    IEnumerator Start()
    {
        // Brief wait to ensure all singletons are initialised
        yield return null;

        yield return StartCoroutine(MorningSequence());
    }

    // -------------------------------------------------------
    // Morning
    // -------------------------------------------------------

    IEnumerator MorningSequence()
    {
        // 1. Fade in — player "opens eyes"
        FadeManager.Instance.SnapToBlack();
        yield return FadeManager.Instance.FadeIn(1.5f);

        // 2. Opening monologue
        yield return PlayAndWait(DialogueSequence.Create(
            new DialogueLine("", "...Another morning."),
            new DialogueLine("", "I should check the calendar.")
        ));

        // 3. Bedroom objective
        ObjectiveManager.Instance.SetObjective("Check the calendar.");

        // 4. Wait for calendar to be read
        yield return WaitForFlag("has_read_calendar");
        ObjectiveManager.Instance.SetObjective("Find the kitchen and make brunch.");

        // 5. Wait for brunch to be made
        yield return WaitForFlag("brunch_made");
        ObjectiveManager.Instance.SetObjective("Place the food on the table.");

        // 6. Wait for food to be placed
        yield return WaitForFlag("food_placed");

        yield return PlayAndWait(DialogueSequence.Create(
            new DialogueLine("", "I should check on Sio.")
        ));

        ObjectiveManager.Instance.SetObjective("Check on Siofra in the living room.");

        // 7. Wait for first piano interaction
        yield return WaitForFlag("piano_visited_morning");

        ObjectiveManager.Instance.SetObjective("Visit the Shaman.");

        // 8. Wait for shaman visit
        yield return WaitForFlag("shaman_visited_today");

        // Shaman visit plays its own return monologue — just wait for it
        yield return WaitForFlag("shaman_return_complete");

        yield return StartCoroutine(EveningSequence());
    }

    // -------------------------------------------------------
    // Evening
    // -------------------------------------------------------

    IEnumerator EveningSequence()
    {
        ObjectiveManager.Instance.SetObjective("Check on Siofra.");

        // Evening piano interaction
        yield return WaitForFlag("piano_visited_evening");

        // Unlock dinner counter and table
        if (dinnerCounter != null) dinnerCounter.gameObject.SetActive(true);
        if (dinnerTable != null) dinnerTable.gameObject.SetActive(true);

        ObjectiveManager.Instance.SetObjective("Make dinner.");
        yield return WaitForFlag("dinner_made");

        ObjectiveManager.Instance.SetObjective("Call Siofra for dinner.");
        yield return WaitForFlag("dinner_placed");

        yield return PlayAndWait(DialogueSequence.Create(
            new DialogueLine("Siofra", "Steamed eggs, my favorite!"),
            new DialogueLine("", "She seems a little better today.")
        ));

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

        yield return PlayAndWait(DialogueSequence.Create(
            new DialogueLine("", "Hmm... the remedy must have worked. She's gotten better."),
            new DialogueLine("", "I should keep this up.")
        ));

        // Fade out to end day
        yield return FadeManager.Instance.FadeOut(1.5f);

        DayManager.Instance.AdvanceDay();

        // TODO: load Day 2 scene or reset flags for next day here
        Debug.Log("[Day1Sequence] Day 1 complete.");
    }

    // -------------------------------------------------------
    // Helpers
    // -------------------------------------------------------

    /// <summary>Plays a dialogue sequence and waits until it finishes.</summary>
    private IEnumerator PlayAndWait(DialogueSequence sequence)
    {
        bool done = false;
        DialogueManager.Instance.PlayDialogue(sequence, onComplete: () => done = true);
        yield return new WaitUntil(() => done);
    }

    /// <summary>Polls DayManager every PollInterval until the flag is set.</summary>
    private IEnumerator WaitForFlag(string flag)
    {
        while (!DayManager.Instance.GetFlag(flag))
            yield return new WaitForSeconds(PollInterval);
    }
}