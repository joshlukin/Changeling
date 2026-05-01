using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    

    private bool _isOpen = false;
    private bool _isTransitioning = false;
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

    // -------------------------------------------------------
    // Public API
    // -------------------------------------------------------

    public void OpenPanel(Sprite art, string label = "", System.Action onClose = null)
    {
        if (_isOpen || _isTransitioning) return;

        _onClose = onClose;
        _isOpen = true;

        SetupArt(art, label);
        SetContinuePromptVisible(true);
        LockPlayer(true);
        StartCoroutine(OpenRoutine(null));
    }

    public void OpenPanelWithCallback(Sprite art, string label = "", System.Action onClose = null, System.Action onPanelReady = null)
    {
        if (_isOpen || _isTransitioning) return;

        _onClose = onClose;
        _isOpen = true;

        SetupArt(art, label);
        // Hide continue prompt — dialogue system will handle advancing
        SetContinuePromptVisible(false);
        LockPlayer(true);
        StartCoroutine(OpenRoutine(onPanelReady));
    }

    public void ClosePanel()
    {
        if (!_isOpen || _isTransitioning) return;
        StartCoroutine(CloseRoutine());
    }

    /// <summary>
    /// Called by DialogueManager when dialogue starts, to hide the E prompt
    /// so it doesn't overlap with the dialogue box's own prompt.
    /// </summary>
    public void SetContinuePromptVisible(bool visible)
    {
        if (continuePromptText != null)
            continuePromptText.gameObject.SetActive(visible);
    }



    void Update()
    {
        if (!_isOpen || _isTransitioning) return;
        
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying) return;

        if (Input.GetKeyDown(KeyCode.E))
            ClosePanel();
    }
    

    private void SetupArt(Sprite art, string label)
    {
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
    }
    

    private IEnumerator OpenRoutine(System.Action onPanelReady)
    {
        _isTransitioning = true;
        panelRoot.SetActive(true);

        CanvasGroup cg = panelRoot.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            cg.interactable = false;

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                cg.alpha = Mathf.Clamp01(elapsed / fadeDuration);
                yield return null;
            }
            cg.alpha = 1f;
            cg.blocksRaycasts = true;
            cg.interactable = true;
        }

        _isTransitioning = false;
        onPanelReady?.Invoke();
    }

    private IEnumerator CloseRoutine()
    {
        _isTransitioning = true;

        CanvasGroup cg = panelRoot.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.blocksRaycasts = false;
            cg.interactable = false;

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


    private void LockPlayer(bool locked)
    {
        if (playerController != null)
            playerController.enabled = !locked;

        Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = locked;
    }
}