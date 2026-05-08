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

        yield return FadeManager.Instance.FadeOut(1.5f);

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