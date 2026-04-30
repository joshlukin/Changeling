using UnityEngine;

/// <summary>
/// The calendar/medicine reminder note in the bedroom.
/// One-time interaction: opens a 2D art panel, player presses E to dismiss.
/// </summary>
public class CalendarInteractable : Interactable
{
    [Header("Calendar")]
    [Tooltip("The 2D art to display. Leave null for a solid placeholder color.")]
    public Sprite calendarArt;

    [Tooltip("Label shown on the panel. Can be left empty.")]
    public string panelLabel = "Medicine Reminders";

    private void Start()
    {
        repeatable = false;
    }

    protected override void OnInteract()
    {
        ScenePanelManager.Instance.OpenPanel(
            calendarArt,
            panelLabel,
            onClose: () =>
            {
                DayManager.Instance.SetFlag("has_read_calendar");
            }
        );
    }
}