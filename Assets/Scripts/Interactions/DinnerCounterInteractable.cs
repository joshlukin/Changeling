using UnityEngine;

[System.Serializable]
public struct DinnerCounterDayData
{
    public Sprite art;
    public string panelLabel;
    public DialogueSequence dinnerDialogue;
    public string dinnerMadeFlag; // e.g. "dinner_made"
}

public class DinnerCounterInteractable : Interactable
{
    [Header("Day Data")]
    [Tooltip("One entry per day. Index 0 = Day 1, Index 1 = Day 2, etc.")]
    public DinnerCounterDayData[] dayData;

    private void Start()
    {
        repeatable = false;
    }

    private int GetDayIndex()
    {
        return Mathf.Clamp(DayManager.Instance.currentDay - 1, 0, dayData.Length - 1);
    }

    protected override bool CanInteract()
    {
        if (dayData == null || dayData.Length == 0) return false;
        return !DayManager.Instance.GetFlag(dayData[GetDayIndex()].dinnerMadeFlag);
        
    }

    protected override void OnInteract()
    {
        if (dayData == null || dayData.Length == 0) return;

        DinnerCounterDayData data = dayData[GetDayIndex()];

        ScenePanelManager.Instance.OpenPanelWithCallback(
            data.art,
            data.panelLabel,
            onClose: null,
            onPanelReady: () =>
            {
                DayManager.Instance.SetFlag(data.dinnerMadeFlag);

                if (data.dinnerDialogue != null)
                    DialogueManager.Instance.PlayDialogue(data.dinnerDialogue);
            }
        );
    }
}