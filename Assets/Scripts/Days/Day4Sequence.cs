using System.Collections;
using UnityEngine;

/// <summary>
/// Drives the Day 4 story sequence.
///
/// DAY 4 FLOW:
/// 1. Mother wakes up at sunset after sleeping nearly an entire day
/// 2. Investigates the living room and realizes time passed
/// 3. Searches Siofra's room
/// 4. Doorbell interrupts investigation
/// 5. Husband returns with doctor results
/// 6. Player makes dinner
/// 7. Disturbing dinner scene
/// 8. End of day
/// </summary>
public class Day4Sequence : MonoBehaviour
{
    [Header("References")]
    public DinnerCounterInteractable dinnerCounter;
    public DinnerTableInteractable dinnerTable;

    [Tooltip("Art shown during wakeup.")]
    public Sprite bedroomArt;

    public Day5Sequence day5Sequence;

    private const float PollInterval = 0.3f;

    // -------------------------------------------------------
    // Entry Point
    // -------------------------------------------------------

    public void StartDay4()
    {
        ResetDayFlags();
        StartCoroutine(MorningSequence());
    }

    private void ResetDayFlags()
    {
        DayManager.Instance.SetFlag("living_room_checked", false);
        DayManager.Instance.SetFlag("daughter_room_entered", false);
        // DayManager.Instance.SetFlag("daughter_room_finished", false);
        DayManager.Instance.SetFlag("front_door_answered", false);
        DayManager.Instance.SetFlag("dinner_made_day4", false);
        DayManager.Instance.SetFlag("dinner_placed_day4", false);
        DayManager.Instance.SetFlag("kitchen_left_day4", false);

        if (dinnerCounter != null)
            dinnerCounter.gameObject.SetActive(false);

        if (dinnerTable != null)
            dinnerTable.gameObject.SetActive(false);
    }

    // -------------------------------------------------------
    // Morning / Wakeup
    // -------------------------------------------------------

    IEnumerator MorningSequence()
    {
        FadeManager.Instance.SnapToBlack();

        ScenePanelManager.Instance.OpenPanel(
            bedroomArt,
            "Bedroom",
            onClose: null
        );

        yield return FadeManager.Instance.FadeIn(2f);

        ScenePanelManager.Instance.SetContinuePromptVisible(false);

        yield return PlayAndWait(DialogueSequence.Create(
            new DialogueLine("You", "Huh? That sound… What time is it?")
        ));

        ObjectiveManager.Instance.SetObjective("Look out the kitchen window.");

        yield return WaitForFlag("living_room_checked");

        yield return PlayAndWait(DialogueSequence.Create(
            new DialogueLine("You", "That’s strange… it’s already sunset? That can’t be…"),
            new DialogueLine("You", "Does that mean… I slept for an entire day!?"),
            new DialogueLine("You", "Sio will be back home soon and I haven’t done anything!"),
            new DialogueLine("You", "There’s no time for food right now, I have to find out what’s happening."),
            new DialogueLine("You", "Sio’s odd behavior and now this… this isn’t normal.")
        ));

        yield return StartCoroutine(DaughterRoomSequence());
    }

    // -------------------------------------------------------
    // Daughter Room Investigation
    // -------------------------------------------------------

    IEnumerator DaughterRoomSequence()
    {
        ObjectiveManager.Instance.SetObjective("Investigate Siofra's room.");

        yield return WaitForFlag("daughter_room_entered");

        yield return PlayAndWait(DialogueSequence.Create(
            new DialogueLine("You", "Her room is such a mess…"),
            new DialogueLine("You", "Her desk looks empty aside from her music sheets and homework…"),
            new DialogueLine("You", "Hmmm I should try looking under her bed"),
            new DialogueLine("You", "... nothing I’m looking for there…"),
            new DialogueLine("You", "She must be hiding her abnormalities elsewhere, I should leave for now")
        ));

        ObjectiveManager.Instance.SetObjective("Leave Siofra's room.");

        yield return WaitForFlag("daughter_room_finished");

        yield return PlayAndWait(DialogueSequence.Create(
            new DialogueLine("You", "Huh? What did I just kick?")
        ));

        yield return StartCoroutine(DoorSequence());
    }

    // -------------------------------------------------------
    // Door / Doctor Scene
    // -------------------------------------------------------

    IEnumerator DoorSequence()
    {
        ObjectiveManager.Instance.SetObjective("Answer the front door.");
        

        yield return PlayAndWait(DialogueSequence.Create(
            new DialogueLine("Your husband", "We’re back!"),
            new DialogueLine("You", "..."),
            new DialogueLine("Your husband", "You know what you were telling me about earlier?"),
            new DialogueLine("Your husband", "Well, I got worried, and I ended up taking her to the doctor"),
            new DialogueLine("Your husband", "They said she was sick… they told me to bring her back for another checkup to confirm what was going on, but either way they requested that she take it easy for the next couple of days and get plenty of water, food, and rest."),
            new DialogueLine("Your husband", "Please… take care of her…"),
            new DialogueLine("You", "...")
        ));

        yield return StartCoroutine(DinnerSequence());
    }

    // -------------------------------------------------------
    // Dinner
    // -------------------------------------------------------

    IEnumerator DinnerSequence()
    {
        if (dinnerCounter != null)
            dinnerCounter.gameObject.SetActive(true);

        ObjectiveManager.Instance.SetObjective("Make dinner.");

        yield return WaitForFlag("dinner_made_day4");

        if (dinnerTable != null)
            dinnerTable.gameObject.SetActive(true);

        ObjectiveManager.Instance.SetObjective("Place the food on the table and call over Siofra");
        
        yield return WaitForFlag("dinner_placed_day4");
        

        ObjectiveManager.Instance.SetObjective("Leave the kitchen.");

        yield return StartCoroutine(EndOfDay());
    }

    // -------------------------------------------------------
    // End Of Day
    // -------------------------------------------------------

    IEnumerator EndOfDay()
    {
        ObjectiveManager.Instance.ClearObjective();

        yield return FadeManager.Instance.FadeOut(1.5f);
        Debug.Log("[Day4Sequence] Day 4 complete.");
        DayManager.Instance.AdvanceDay();

        if (day5Sequence != null)
        {
            gameObject.SetActive(false);
            day5Sequence.StartDay5();
        }
        else
        {
            Debug.LogWarning("[Day4Sequence] No Day5Sequence assigned.");
        }
        
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