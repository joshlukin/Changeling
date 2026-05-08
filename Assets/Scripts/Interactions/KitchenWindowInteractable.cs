using UnityEngine;

public class KitchenWindowInteractable : Interactable
{
    public Sprite art;
    public DayManager DayManagerInstance;

    [Header("Wwise Events")]
    public AK.Wwise.Event windowLookStartEvent;
    public AK.Wwise.Event windowLookStopEvent;

    [Header("Audio Post Target")]
    public GameObject playerObject;

    private void Start()
    {
        repeatable = false;
    }
    
    protected override bool CanInteract()
    {
        return DayManager.GetCurrentDay(DayManagerInstance) == 4 && !DayManager.Instance.GetFlag("living_room_checked");
        
    }

    protected override void OnInteract()
    {
        ScenePanelManager.Instance.OpenPanelWithCallback(
            art,
            "Kitchen Window",
            onClose: () =>
            {
                if (windowLookStopEvent != null)
                {
                    windowLookStopEvent.Post(playerObject != null ? playerObject : gameObject);
                }
            },
            onPanelReady: () =>
            {
                if (windowLookStartEvent != null)
                {
                    windowLookStartEvent.Post(playerObject != null ? playerObject : gameObject);
                }

                DayManager.Instance.SetFlag("living_room_checked");
            }
        );
    }
}