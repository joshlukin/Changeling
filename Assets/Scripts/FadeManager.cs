using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages full-screen black fade transitions.
/// Requires a Canvas with a full-screen black Image child — see setup notes.
/// </summary>
public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance { get; private set; }

    [Tooltip("The full-screen black Image used for fading.")]
    public Image fadeImage;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Start fully transparent
        _SetAlpha(0f);
    }

    // -------------------------------------------------------
    // Public API
    // -------------------------------------------------------

    /// <summary>Fades the screen to black over <paramref name="duration"/> seconds.</summary>
    public Coroutine FadeOut(float duration = 1f)
    {
        return StartCoroutine(FadeRoutine(0f, 1f, duration));
    }

    /// <summary>Fades the screen from black to clear over <paramref name="duration"/> seconds.</summary>
    public Coroutine FadeIn(float duration = 1f)
    {
        return StartCoroutine(FadeRoutine(1f, 0f, duration));
    }

    /// <summary>
    /// Fades out, waits, then fades back in. 
    /// Optionally runs an action in the black gap (e.g. moving the player).
    /// </summary>
    public Coroutine FadeOutAndIn(float fadeOutDuration = 0.5f, float holdDuration = 0f, float fadeInDuration = 0.5f, System.Action onBlack = null)
    {
        return StartCoroutine(FadeOutAndInRoutine(fadeOutDuration, holdDuration, fadeInDuration, onBlack));
    }

    /// <summary>Instantly snaps to black with no animation.</summary>
    public void SnapToBlack()
    {
        StopAllCoroutines();
        _SetAlpha(1f);
    }

    /// <summary>Instantly clears the fade with no animation.</summary>
    public void SnapToClear()
    {
        StopAllCoroutines();
        _SetAlpha(0f);
    }

    // -------------------------------------------------------
    // Coroutines

    private IEnumerator FadeRoutine(float from, float to, float duration)
    {
        float elapsed = 0f;
        _SetAlpha(from);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _SetAlpha(Mathf.Lerp(from, to, elapsed / duration));
            yield return null;
        }

        _SetAlpha(to);
    }

    private IEnumerator FadeOutAndInRoutine(float fadeOutDuration, float holdDuration, float fadeInDuration, System.Action onBlack)
    {
        yield return FadeRoutine(0f, 1f, fadeOutDuration);

        onBlack?.Invoke();

        if (holdDuration > 0f)
            yield return new WaitForSeconds(holdDuration);

        yield return FadeRoutine(1f, 0f, fadeInDuration);
    }
    

    private void _SetAlpha(float alpha)
    {
        if (fadeImage) return;
        Color c = fadeImage.color;
        c.a = alpha;
        fadeImage.color = c;
    }
}