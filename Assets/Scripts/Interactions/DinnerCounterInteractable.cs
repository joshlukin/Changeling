using UnityEngine;

[System.Serializable]
public struct DinnerCounterDayData
{
    public Sprite art;
    public string panelLabel;
    public DialogueSequence dinnerDialogue;
    public string dinnerMadeFlag; // e.g. "dinner_made_day1"

    [Header("Wwise Events")]
    public AK.Wwise.Event dinnerStartEvent;
    public AK.Wwise.Event dinnerStopEvent;
}

public class DinnerCounterInteractable : Interactable
{
    [Header("Day Data")]
    [Tooltip("One entry per day. Index 0 = Day 1, Index 1 = Day 2, etc.")]
    public DinnerCounterDayData[] dayData;

    [Header("Audio Post Target")]
    public GameObject playerObject;

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
            onClose: () =>
            {
                if (data.dinnerStopEvent != null)
                {
                    data.dinnerStopEvent.Post(playerObject != null ? playerObject : gameObject);
                }
            },
            onPanelReady: () =>
            {
                if (data.dinnerStartEvent != null)
                {
                    data.dinnerStartEvent.Post(playerObject != null ? playerObject : gameObject);
                }

                DayManager.Instance.SetFlag(data.dinnerMadeFlag);

                if (data.dinnerDialogue != null)
                    DialogueManager.Instance.PlayDialogue(data.dinnerDialogue);
            }
        );
    }
}