using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;

    [Header("Settings")]
    public KeyCode advanceKey = KeyCode.E;

    private Queue<DialogueLine> _lineQueue = new Queue<DialogueLine>();
    private System.Action _onComplete;
    private bool _isPlaying = false;
    private bool _awaitingInput = false;

    // If PlayDialogue is called while already playing, store it here
    // and play it immediately after the current sequence ends.
    private DialogueSequence _pendingSequence;
    private System.Action _pendingOnComplete;

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

    public void PlayDialogue(DialogueSequence sequence, System.Action onComplete = null)
    {
        if (_isPlaying)
        {
            // Queue it instead of dropping it — fixes shaman dialogue being skipped
            Debug.Log("[DialogueManager] Queuing sequence — already playing.");
            _pendingSequence = sequence;
            _pendingOnComplete = onComplete;
            return;
        }

        StartSequence(sequence, onComplete);
    }

    public void ForceStop()
    {
        _lineQueue.Clear();
        _pendingSequence = null;
        _pendingOnComplete = null;
        _isPlaying = false;
        _awaitingInput = false;
        HidePanel();
    }

    // -------------------------------------------------------
    // Internal
    // -------------------------------------------------------

    private void StartSequence(DialogueSequence sequence, System.Action onComplete)
    {
        _lineQueue.Clear();
        foreach (var line in sequence.lines)
            _lineQueue.Enqueue(line);

        _onComplete = onComplete;
        _isPlaying = true;

        ShowPanel();
        DisplayNextLine();
    }

    private void DisplayNextLine()
    {
        if (_lineQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = _lineQueue.Dequeue();

        bool hasSpeaker = !string.IsNullOrEmpty(line.speakerName);
        if (speakerNameText != null)
        {
            speakerNameText.gameObject.SetActive(hasSpeaker);
            if (hasSpeaker)
                speakerNameText.text = line.speakerName;
        }

        if (dialogueText != null)
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

        var callback = _onComplete;
        _onComplete = null;
        callback?.Invoke();

        // Play pending sequence if one was queued during this sequence
        if (_pendingSequence != null)
        {
            var next = _pendingSequence;
            var nextCallback = _pendingOnComplete;
            _pendingSequence = null;
            _pendingOnComplete = null;
            StartSequence(next, nextCallback);
        }
    }

    private void ShowPanel()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);
    }

    private void HidePanel()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }
}