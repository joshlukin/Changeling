using UnityEngine;

public class KitchenWindowInteractable : Interactable
{
    public Sprite art;
    public DayManager DayManagerInstance;
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
            onClose: null,
            onPanelReady: () =>
            {
                DayManager.Instance.SetFlag("living_room_checked");
            }
        );
    }
}