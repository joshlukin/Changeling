using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the full-screen 2D panel that overlays the 3D view.
/// </summary>
public class ScenePanelManager : MonoBehaviour
{
    public static ScenePanelManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject panelRoot;
    public Image artImage;
    public TextMeshProUGUI sceneLabelText;
    public TextMeshProUGUI continuePromptText;

    [Header("Transition")]
    public float fadeDuration = 0.4f;

    [Header("Player Reference")]
    public MonoBehaviour playerController;

    // -------------------------------------------------------
    // State
    // -------------------------------------------------------

    private bool _isOpen = false;
    private bool _isTransitioning = false;  // true while fading in OR out
    private System.Action _onClose;

    public bool IsOpen => _isOpen;

    // -------------------------------------------------------
    // Lifecycle
    // -------------------------------------------------------

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    // -------------------------------------------------------
    // Public API
    // -------------------------------------------------------

    public void OpenPanel(Sprite art, string label = "", System.Action onClose = null)
    {
        if (_isOpen || _isTransitioning) return;

        _onClose = onClose;
        _isOpen = true;

        if (artImage != null)
        {
            if (art != null)
            {
                artImage.sprite = art;
                artImage.color = Color.white;
            }
            else
            {
                artImage.sprite = null;
                artImage.color = new Color(0.1f, 0.08f, 0.12f);
            }
        }

        if (sceneLabelText != null)
        {
            sceneLabelText.text = label;
            sceneLabelText.gameObject.SetActive(!string.IsNullOrEmpty(label));
        }

        if (continuePromptText != null)
            continuePromptText.text = "[E] Continue";

        LockPlayer(true);
        StartCoroutine(OpenRoutine());
    }

    public void ClosePanel()
    {
        if (!_isOpen || _isTransitioning) return;
        StartCoroutine(CloseRoutine());
    }

    // -------------------------------------------------------
    // Input — only close if fully open and not transitioning
    // -------------------------------------------------------

    void Update()
    {
        if (_isOpen && !_isTransitioning && Input.GetKeyDown(KeyCode.E))
            ClosePanel();
    }

    // -------------------------------------------------------
    // Coroutines
    // -------------------------------------------------------

    private IEnumerator OpenRoutine()
    {
        _isTransitioning = true;
        panelRoot.SetActive(true);

        CanvasGroup cg = panelRoot.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 0f;
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                cg.alpha = Mathf.Clamp01(elapsed / fadeDuration);
                yield return null;
            }
            cg.alpha = 1f;
        }

        _isTransitioning = false;
        // Panel is now fully open — safe to show buttons, play dialogue, etc.
        // Callers that need to run logic AFTER the panel is visible
        // should use the onPanelReady callback via OpenPanelWithCallback.
    }

    private IEnumerator CloseRoutine()
    {
        _isTransitioning = true;

        CanvasGroup cg = panelRoot.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                cg.alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration);
                yield return null;
            }
            cg.alpha = 0f;
        }

        panelRoot.SetActive(false);
        _isOpen = false;
        _isTransitioning = false;
        LockPlayer(false);

        _onClose?.Invoke();
        _onClose = null;
    }

    // -------------------------------------------------------
    // Extended open with ready callback
    // -------------------------------------------------------

    /// <summary>
    /// Opens the panel and fires onPanelReady once the fade-in is complete.
    /// Use this when you need to show buttons or start dialogue AFTER the panel is visible.
    /// </summary>
    public void OpenPanelWithCallback(Sprite art, string label = "", System.Action onClose = null, System.Action onPanelReady = null)
    {
        if (_isOpen || _isTransitioning) return;

        _onClose = onClose;
        _isOpen = true;

        if (artImage != null)
        {
            if (art != null)
            {
                artImage.sprite = art;
                artImage.color = Color.white;
            }
            else
            {
                artImage.sprite = null;
                artImage.color = new Color(0.1f, 0.08f, 0.12f);
            }
        }

        if (sceneLabelText != null)
        {
            sceneLabelText.text = label;
            sceneLabelText.gameObject.SetActive(!string.IsNullOrEmpty(label));
        }

        if (continuePromptText != null)
            continuePromptText.text = "[E] Continue";

        LockPlayer(true);
        StartCoroutine(OpenRoutineWithCallback(onPanelReady));
    }

    private IEnumerator OpenRoutineWithCallback(System.Action onPanelReady)
    {
        _isTransitioning = true;
        panelRoot.SetActive(true);

        CanvasGroup cg = panelRoot.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 0f;
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                cg.alpha = Mathf.Clamp01(elapsed / fadeDuration);
                yield return null;
            }
            cg.alpha = 1f;
        }

        _isTransitioning = false;
        onPanelReady?.Invoke();
    }

    // -------------------------------------------------------
    // Player lock
    // -------------------------------------------------------

    private void LockPlayer(bool locked)
    {
        if (playerController != null)
            playerController.enabled = !locked;

        Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = locked;
    }
}