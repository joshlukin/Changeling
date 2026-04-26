using UnityEngine;

/// <summary>
/// Attach this to the Shaman (or placeholder cube).
/// Also requires an Inventory component on the same GameObject.
/// </summary>
[RequireComponent(typeof(Inventory))]
public class ShamanInteractable : MonoBehaviour
{
    [Header("Interaction")]
    [Tooltip("How close the player must be to interact.")]
    public float interactRange = 3f;

    [Tooltip("Key the player presses to interact.")]
    public KeyCode interactKey = KeyCode.E;

    private Inventory _shamanInventory;
    private Inventory _playerInventory; // optional: give player an Inventory too
    private bool _playerInRange = false;

    // Simple GUI prompt — replace with your UI system later
    private bool _showPrompt = false;
    private string _promptMessage = "";

    void Awake()
    {
        _shamanInventory = GetComponent<Inventory>();
    }
    

    void OnGUI()
    {
        if (_showPrompt)
        {
            // Centered screen prompt — swap this out for a proper UI later
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.fontSize = 20;
            style.alignment = TextAnchor.MiddleCenter;

            float w = 400, h = 60;
            GUI.Box(new Rect(Screen.width / 2f - w / 2f, Screen.height - 120, w, h),
                    _promptMessage, style);
        }
    }

    // Called by PlayerInteraction when the player enters/exits range
    public void SetPlayerInRange(bool inRange, Inventory pInventory = null)
    {
        _playerInRange = inRange;
        _playerInventory = pInventory;

        if (inRange)
        {
            UpdatePrompt();
        }
        else
        {
            _showPrompt = false;
        }
    }

    public void TryGiveItemToPlayer()
    {
        if (!_shamanInventory.HasItems())
        {
            _promptMessage = "The Shaman has nothing left to offer.";
            return;
        }

        Item given = _shamanInventory.TakeFirstItem();

        if (_playerInventory)
            _playerInventory.AddItem(given);

        Debug.Log($"[Shaman] Gave '{given.itemName}' to the player.");
        _promptMessage = $"Received: {given.itemName}";

        // Update prompt after taking
        if (!_shamanInventory.HasItems())
            _promptMessage += "  (Nothing left)";
    }

    private void UpdatePrompt()
    {
        _showPrompt = true;
        if (_shamanInventory.HasItems())
            _promptMessage = $"[{interactKey}] Take item from Shaman";
        else
            _promptMessage = "The Shaman has nothing to offer.";
    }

    //visualized interact range
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}