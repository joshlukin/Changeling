using System.Collections;
using UnityEngine;

public class Day7Sequence : MonoBehaviour
{
    [Header("Ending Cutscenes")]
    [Tooltip("Images to play in sequence if the player chose to kill.")]
    public Sprite[] killCutsceneSprites;
    
    [Tooltip("Images to play in sequence if the player chose to let go.")]
    public Sprite[] letGoCutsceneSprites;

    [Header("Audio Settings")]
    public AK.Wwise.Event killEndingMusic;
    public AK.Wwise.Event letGoEndingMusic;
    public GameObject audioPostTarget;

    /// <summary>
    /// Call this when Day 7 begins (after the fade from Day 6).
    /// </summary>
    public void StartDay7()
    {
        StartCoroutine(Day7Routine());
    }

    private IEnumerator Day7Routine()
    {
        // Fade in from the black screen at the end of Day 6
        yield return FadeManager.Instance.FadeIn(2.0f);

        // Determine which sequence to play based on a flag set during Day 6
        if (DayManager.Instance.GetFlag("chose_kill"))
        {
            yield return StartCoroutine(PlayKillCutscene());
        }
        else
        {
            yield return StartCoroutine(PlayLetGoCutscene());
        }

        // Wrap up the game (fade to black, load credits, etc.)
        yield return FadeManager.Instance.FadeOut(3.0f);
        Debug.Log("[Day7Sequence] Game Complete. Transition to Credits or Main Menu.");
    }

    // -------------------------------------------------------
    // Distinct Cutscene Functions
    // -------------------------------------------------------

    public IEnumerator PlayKillCutscene()
    {
        Debug.Log("[Day7Sequence] Playing Kill Cutscene...");
        
        if (killEndingMusic != null)
            killEndingMusic.Post(audioPostTarget != null ? audioPostTarget : gameObject);

        yield return StartCoroutine(PlaySpriteSequence(killCutsceneSprites));
    }

    public IEnumerator PlayLetGoCutscene()
    {
        Debug.Log("[Day7Sequence] Playing Let Go Cutscene...");

        if (letGoEndingMusic != null)
            letGoEndingMusic.Post(audioPostTarget != null ? audioPostTarget : gameObject);

        yield return StartCoroutine(PlaySpriteSequence(letGoCutsceneSprites));
    }

    // -------------------------------------------------------
    // Slideshow Logic
    // -------------------------------------------------------

    /// <summary>
    /// Loops through an array of sprites using the ScenePanelManager.
    /// The player must dismiss the current panel to see the next one.
    /// </summary>
    private IEnumerator PlaySpriteSequence(Sprite[] frames)
    {
        if (frames == null || frames.Length == 0)
        {
            Debug.LogWarning("[Day7Sequence] No sprites assigned for this cutscene!");
            yield break;
        }

        // Lock player movement and prevent manual skipping
        ScenePanelManager.Instance.LockPlayer(true);
        ScenePanelManager.Instance.SetCanCloseWithKey(false);

        foreach (Sprite frame in frames)
        {
            // Open the panel with no onClose callback needed
            ScenePanelManager.Instance.OpenPanel(
                frame,
                "Ending Sequence",
                onClose: null
            );

            // Wait exactly 3 seconds per frame (so 2 frames = 6 seconds)
            yield return new WaitForSeconds(3.0f);
            
            // Programmatically close the panel so the next one can open
            ScenePanelManager.Instance.ClosePanel();

            // Wait 0.5 seconds to let the ScenePanelManager's fade-out animation finish
            // (Assuming your fadeDuration is around 0.4 seconds)
            yield return new WaitForSeconds(0.5f);
        }

        // Restore control
        ScenePanelManager.Instance.SetCanCloseWithKey(true);
        ScenePanelManager.Instance.LockPlayer(false);
    }
}