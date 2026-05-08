using System.Collections;
using UnityEngine;

/// <summary>
/// Debug tool for skipping to specific days and story beats.
/// Attach to any persistent GameObject (e.g. DayManager).
/// REMOVE or disable before shipping.
///
/// CONTROLS (only active in editor or development builds):
///   F1 — Skip to start of Day 1 evening (post-shaman)
///   F2 — Skip to start of Day 2
///   F3 — Skip to start of Day 2 evening
/// </summary>
public class DebugDaySkip : MonoBehaviour
{
    [Header("References")]
    public Day1Sequence day1Sequence;
    public Day2Sequence day2Sequence;

    [Header("Evening Skip References")]
    [Tooltip("Dinner counter — enabled when skipping to evening.")]
    public GameObject dinnerCounter;
    [Tooltip("Dinner table — enabled when skipping to evening.")]
    public GameObject dinnerTable;
    [Tooltip("Brunch table plate prop — disabled when skipping to evening.")]
    public GameObject brunchPlateProp;

    [Header("Settings")]
    [Tooltip("Only active in editor or development builds.")]
    public bool onlyInDevelopment = true;

    private void Update()
    {
#if !UNITY_EDITOR
        if (onlyInDevelopment && !Debug.isDebugBuild) return;
#endif

        if (Input.GetKeyDown(KeyCode.F1)) StartCoroutine(SkipToDay1Evening());
        if (Input.GetKeyDown(KeyCode.F2)) StartCoroutine(SkipToDay2Morning());
        if (Input.GetKeyDown(KeyCode.F3)) StartCoroutine(SkipToDay2Evening());
    }

    // -------------------------------------------------------
    // Skip targets
    // -------------------------------------------------------

    /// <summary>
    /// Skips to right after the shaman visit on Day 1.
    /// All morning flags are set, shaman visited, evening begins.
    /// </summary>
    private IEnumerator SkipToDay1Evening()
    {
        Debug.Log("[DebugDaySkip] Skipping to Day 1 Evening...");

        SetDay1MorningFlags();
        DayManager.Instance.SetFlag("shaman_visited_today");
        DayManager.Instance.SetFlag("shaman_return_complete");

        if (brunchPlateProp != null) brunchPlateProp.SetActive(false);

        // Stop Day1's current coroutine and jump to evening
        if (day1Sequence != null)
        {
            day1Sequence.StopAllCoroutines();
            yield return null; // one frame gap
            day1Sequence.StartCoroutine("EveningSequence");
        }

        FadeManager.Instance.SnapToClear();
        Debug.Log("[DebugDaySkip] Now at Day 1 Evening.");
    }

    /// <summary>
    /// Skips to the start of Day 2 morning.
    /// All Day 1 flags set, day advanced to 2.
    /// </summary>
    private IEnumerator SkipToDay2Morning()
    {
        Debug.Log("[DebugDaySkip] Skipping to Day 2 Morning...");

        SetDay1MorningFlags();
        SetDay1EveningFlags();

        DayManager.Instance.currentDay = 2;
        DayManager.Instance.daughterHealth = 90f;
        DayManager.Instance.relationshipStat = 10; // assume relationship built

        if (day1Sequence != null) day1Sequence.gameObject.SetActive(false);

        if (day2Sequence != null)
        {
            day2Sequence.gameObject.SetActive(true);
            yield return null;
            day2Sequence.StartDay2();
        }

        FadeManager.Instance.SnapToClear();
        Debug.Log("[DebugDaySkip] Now at Day 2 Morning.");
    }

    /// <summary>
    /// Skips to Day 2 evening — post shaman, ready for dinner.
    /// </summary>
    private IEnumerator SkipToDay2Evening()
    {
        Debug.Log("[DebugDaySkip] Skipping to Day 2 Evening...");

        SetDay1MorningFlags();
        SetDay1EveningFlags();

        DayManager.Instance.currentDay = 2;
        DayManager.Instance.daughterHealth = 90f;
        DayManager.Instance.relationshipStat = 10;
        DayManager.Instance.SetFlag("shaman_visited_today");
        DayManager.Instance.SetFlag("shaman_return_complete");

        if (day1Sequence != null) day1Sequence.gameObject.SetActive(false);

        if (day2Sequence != null)
        {
            day2Sequence.gameObject.SetActive(true);
            day2Sequence.StopAllCoroutines();
            yield return null;
            day2Sequence.StartCoroutine("EveningSequence");
        }

        FadeManager.Instance.SnapToClear();
        Debug.Log("[DebugDaySkip] Now at Day 2 Evening.");
    }

    // -------------------------------------------------------
    // Flag helpers
    // -------------------------------------------------------

    private void SetDay1MorningFlags()
    {
        DayManager.Instance.SetFlag("has_read_calendar");
        DayManager.Instance.SetFlag("brunch_made");
        DayManager.Instance.SetFlag("kitchen_objective_complete");
        DayManager.Instance.SetFlag("food_placed");
        DayManager.Instance.SetFlag("piano_visited_morning");
        DayManager.Instance.SetFlag("homework_placed");
    }

    private void SetDay1EveningFlags()
    {
        DayManager.Instance.SetFlag("piano_visited_evening");
        DayManager.Instance.SetFlag("dinner_made");
        DayManager.Instance.SetFlag("dinner_placed");
        DayManager.Instance.SetFlag("health_checked");
    }
}