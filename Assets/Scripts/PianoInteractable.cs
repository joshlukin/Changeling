using UnityEngine;

public class PianoInteractable : Interactable
{
    public Sprite pianoArtPlaceholder; // drag a sprite in, or leave null
    private int _clickCount = 0;

    protected override void OnInteract()
    {
        _clickCount++;
        Debug.Log("Piano interacted with " + _clickCount + " clicks");
        if (_clickCount < 4)
        {
            ScenePanelManager.Instance.OpenPanel(pianoArtPlaceholder, "Living Room", onClose: () =>
            {
                DialogueManager.Instance.PlayDialogue(DialogueSequence.Create(
                    new DialogueLine("Daughter", "Is it breakfast time?")
                ));
            });
        }
        else
        {
            ScenePanelManager.Instance.OpenPanel(pianoArtPlaceholder, "Living Room", onClose: () =>
            {
                DialogueManager.Instance.PlayDialogue(DialogueSequence.Create(
                    new DialogueLine("Daughter", "..."),
                    new DialogueLine("Mother", "...")
                ));
            });
        }
    }
}