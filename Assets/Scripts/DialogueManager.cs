using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages sequential dialogue display.
/// Feed it a DialogueSequence and it handles the rest.
/// Press key to advance lines.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI References")]
    [Tooltip("The root panel that shows/hides the dialogue box.")]
    public GameObject dialoguePanel;

    [Tooltip("Displays the speaker's name. Hide this object for narration/monologue lines.")]
    public TextMeshProUGUI speakerNameText;

    [Tooltip("Displays the dialogue line.")]
    public TextMeshProUGUI dialogueText;

    [Header("Settings")]
    public KeyCode advanceKey = KeyCode.E;


    private Queue<DialogueLine> _lineQueue = new Queue<DialogueLine>();
    private System.Action _onComplete;
    private bool _isPlaying = false;
    private bool _awaitingInput = false;

    public bool IsPlaying => _isPlaying;
    

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        HidePanel();
    }

    void Update()
    {
        if (_awaitingInput && Input.GetKeyDown(advanceKey))
            AdvanceLine();
    }

    // -------------------------------------------------------
    // Public API
    // -------------------------------------------------------

    /// <summary>
    /// Starts playing a dialogue sequence.
    /// </summary>
    /// <param name="sequence">The lines to display.</param>
    /// <param name="onComplete">Optional callback fired when the last line is dismissed.</param>
    public void PlayDialogue(DialogueSequence sequence, System.Action onComplete = null)
    {
        if (_isPlaying)
        {
            Debug.LogWarning("[DialogueManager] Already playing dialogue. Ignoring new request.");
            return;
        }

        _lineQueue.Clear();
        foreach (var line in sequence.lines)
            _lineQueue.Enqueue(line);

        _onComplete = onComplete;
        _isPlaying = true;

        ShowPanel();
        DisplayNextLine();
    }

    /// <summary>
    /// Immediately stops dialogue and hides the panel.
    /// </summary>
    public void ForceStop()
    {
        StopAllCoroutines();
        _lineQueue.Clear();
        _isPlaying = false;
        _awaitingInput = false;
        HidePanel();
    }


    private void DisplayNextLine()
    {
        if (_lineQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = _lineQueue.Dequeue();

        // Speaker name — hide the label for narration/monologue
        bool hasSpeaker = !string.IsNullOrEmpty(line.speakerName);
        if (speakerNameText)
        {
            speakerNameText.gameObject.SetActive(hasSpeaker);
            if (hasSpeaker)
                speakerNameText.text = line.speakerName;
        }

        if (dialogueText)
            dialogueText.text = line.text;

        _awaitingInput = true;
    }

    private void AdvanceLine()
    {
        _awaitingInput = false;
        DisplayNextLine();
    }

    private void EndDialogue()
    {
        _isPlaying = false;
        _awaitingInput = false;
        HidePanel();
        _onComplete?.Invoke();
        _onComplete = null;
    }

    private void ShowPanel()
    {
        if (dialoguePanel)
            dialoguePanel.SetActive(true);
    }

    private void HidePanel()
    {
        if (dialoguePanel)
            dialoguePanel.SetActive(false);
    }
}