using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A single line of dialogue. Leave speakerName empty for narration or internal monologue.
/// </summary>
[Serializable]
public class DialogueLine
{
    [Tooltip("Who is speaking. Leave blank for narration or internal monologue.")]
    public string speakerName;

    [TextArea(2, 5)]
    public string text;
    
    public DialogueLine(string text)
    {
        this.speakerName = "";
        this.text = text;
    }

    public DialogueLine(string speakerName, string text)
    {
        this.speakerName = speakerName;
        this.text = text;
    }
}

/// <summary>
/// An ordered list of DialogueLines played as a sequence.
/// </summary>
[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Sequence")]
public class DialogueSequence : ScriptableObject
{
    public List<DialogueLine> lines = new List<DialogueLine>();

    /// <summary>
    /// Builds a DialogueSequence at runtime without needing a ScriptableObject asset.
    /// Useful for simple one-off dialogue triggered by interactables.
    /// </summary>
    public static DialogueSequence Create(params DialogueLine[] lines)
    {
        DialogueSequence seq = CreateInstance<DialogueSequence>();
        seq.lines = new List<DialogueLine>(lines);
        return seq;
    }
}