using UnityEngine;

public class SioRoomInteractable : Interactable
{
    public Sprite art;
    public DayManager DayManagerInstance;
    private void Start()
    {
        repeatable = false;
    }
    
    protected override bool CanInteract()
    {
        return DayManager.GetCurrentDay(DayManagerInstance) == 4 && !DayManager.Instance.GetFlag("daughter_room_entered");
        
    }

    protected override void OnInteract()
    {
        ScenePanelManager.Instance.OpenPanelWithCallback(
            art,
            "Sio room",
            onClose: ()=> DayManager.Instance.SetFlag("daughter_room_finished"),
            onPanelReady: () =>
            {
                DayManager.Instance.SetFlag("daughter_room_entered");
            }
        );
    }
}