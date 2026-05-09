using UnityEngine;

public class SioRoomInteractable : Interactable
{
    public Sprite art;
    public DayManager DayManagerInstance;

    [Header("Wwise Events")]
    public AK.Wwise.Event doorOpenEvent;
    public AK.Wwise.Event doorCloseEvent;

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
        doorOpenEvent?.Post(gameObject);

        ScenePanelManager.Instance.OpenPanelWithCallback(
            art,
            "Sio room",
            onClose: ()=> 
            {
                doorCloseEvent?.Post(gameObject);
                DayManager.Instance.SetFlag("daughter_room_finished", true);
            },
            onPanelReady: () =>
            {
                DayManager.Instance.SetFlag("daughter_room_entered");
            }
        );
    }
}