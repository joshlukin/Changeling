using UnityEngine;

[System.Serializable]
public struct DinnerTableDayData
{
    public Sprite art;
    public string panelLabel;
    public DialogueSequence callDialogue;  // plays after panel closes (calling Siofra)
    public string dinnerMadeFlag;          // must be true to interact (e.g. "dinner_made")
    public string dinnerPlacedFlag;        // set on interaction (e.g. "dinner_placed")
    public GameObject plateProp;           // food prop to enable after placement
}

public class DinnerTableInteractable : Interactable
{
    [Header("Day Data")]
    [Tooltip("One entry per day. Index 0 = Day 1, Index 1 = Day 2, etc.")]
    public DinnerTableDayData[] dayData;

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
        DinnerTableDayData data = dayData[GetDayIndex()];
        return DayManager.Instance.GetFlag(data.dinnerMadeFlag)
               && !DayManager.Instance.GetFlag(data.dinnerPlacedFlag);
    }

    protected override void OnInteract()
    {
        if (dayData == null || dayData.Length == 0) return;

        DinnerTableDayData data = dayData[GetDayIndex()];

        ScenePanelManager.Instance.OpenPanel(
            data.art,
            data.panelLabel,
            onClose: () =>
            {
                DayManager.Instance.SetFlag(data.dinnerPlacedFlag);

                if (data.plateProp != null)
                    data.plateProp.SetActive(true);

                if (data.callDialogue != null)
                {
                    ScenePanelManager.Instance.LockPlayer(true);
                    DialogueManager.Instance.PlayDialogue(
                        data.callDialogue,
                        onComplete: () => ScenePanelManager.Instance.LockPlayer(false)
                    );
                }
            }
        );
    }
}