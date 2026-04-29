using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the full-screen 2D panel that overlays the 3D view when the player
/// interacts with a scene trigger (piano, curtains, calendar, etc.).
/// 
/// SETUP:
/// - One ScenePanelManager exists in the scene (singleton).
/// - It controls a full-screen Canvas with an Image (the art) and optional text.
/// - Each Interactable that triggers a 2D scene calls ScenePanelManager.Instance.OpenPanel(...)
/// - The player's movement is locked while a panel is open.
/// </summary>
public class ScenePanelManager : MonoBehaviour
{
    public static ScenePanelManager Instance { get; private set; }

    [Header("UI References")]
    [Tooltip("The root panel GameObject. Enable/disable to show/hide.")]
    public GameObject panelRoot;

    [Tooltip("The main art image. Swap sprite to change the scene.")]
    public Image artImage;

    [Tooltip("Optional scene label (e.g. room name). Can be left empty.")]
    public TextMeshProUGUI sceneLabelText;

    [Tooltip("Small prompt at the bottom. e.g. '[E] Continue'")]
    public TextMeshProUGUI continuePromptText;

    [Header("Transition")]
    [Tooltip("How long the fade in/out takes when opening or closing the panel.")]
    public float fadeDuration = 0.4f;

    [Header("Player Reference")]
    [Tooltip("Drag your Player_Object here so movement can be locked during scenes.")]
    public MonoBehaviour playerController;
    
    private bool _isOpen = false;
    private System.Action _onClose;

    public bool IsOpen => _isOpen;
    

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
    

    /// <summary>
    /// Opens the 2D scene panel with the given art sprite.
    /// Locks player movement. Press E to close.
    /// </summary>
    /// <param name="art">The 2D sprite to display. Pass null for a placeholder.</param>
    /// <param name="label">Optional scene label text.</param>
    /// <param name="onClose">Callback fired when the player dismisses the panel.</param>
    public void OpenPanel(Sprite art, string label = "", System.Action onClose = null)
    {
        if (_isOpen) return;

        _onClose = onClose;
        _isOpen = true;

        if (artImage != null)
        {
            // Use art if provided, otherwise tint the image dark as a placeholder
            if (art != null)
            {
                artImage.sprite = art;
                artImage.color = Color.white;
            }
            else
            {
                artImage.sprite = null;
                artImage.color = new Color(0.1f, 0.08f, 0.12f); // dark placeholder
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

    /// <summary>
    /// Closes the panel programmatically (e.g. after dialogue finishes).
    /// </summary>
    public void ClosePanel()
    {
        if (!_isOpen) return;
        StartCoroutine(CloseRoutine());
    }

    // -------------------------------------------------------
    // Input
    // -------------------------------------------------------

    void Update()
    {
        if (_isOpen && Input.GetKeyDown(KeyCode.E))
            ClosePanel();
    }

    // -------------------------------------------------------
    // Coroutines
    // -------------------------------------------------------

    private IEnumerator OpenRoutine()
    {
        panelRoot.SetActive(true);

        // Fade in via CanvasGroup if available, otherwise just show
        CanvasGroup cg = panelRoot.GetComponent<CanvasGroup>();
        if (cg)
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
    }

    private IEnumerator CloseRoutine()
    {
        CanvasGroup cg = panelRoot.GetComponent<CanvasGroup>();
        if (cg)
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
        LockPlayer(false);

        _onClose?.Invoke();
        _onClose = null;
    }
    

    private void LockPlayer(bool locked)
    {
        if (playerController)
            playerController.enabled = !locked;

        // Lock/unlock cursor
        Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = locked;
    }
}