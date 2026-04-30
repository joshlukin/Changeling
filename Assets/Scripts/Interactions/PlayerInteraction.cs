using UnityEngine;

/// <summary>
/// Attach this to the Player GameObject.
/// Detects nearby ShamanInteractable objects and notifies them.
/// Optionally, also give the Player an Inventory component.
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Tooltip("Should match or exceed the Shaman's interactRange.")]
    public float detectionRadius = 3f;

    private ShamanInteractable _currentShaman = null;
    private Inventory _playerInventory;

    void Awake()
    {
        _playerInventory = GetComponent<Inventory>();
    }

    void Update()
    {
        DetectShaman();
    }

    private void DetectShaman()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius);

        ShamanInteractable found = null;
        foreach (var hit in hits)
        {
            ShamanInteractable s = hit.GetComponent<ShamanInteractable>();
            if (s)
            {
                found = s;
                break;
            }
        }

        // Entered range
        if (found && found != _currentShaman)
        {
            if (_currentShaman)
                _currentShaman.SetPlayerInRange(false);

            _currentShaman = found;
            _currentShaman.SetPlayerInRange(true, _playerInventory);
        }

        // Left range
        if (!found && _currentShaman)
        {
            _currentShaman.SetPlayerInRange(false);
            _currentShaman = null;
        }
        
        if (_currentShaman && Input.GetKeyDown(KeyCode.E))
        {
            _currentShaman.TryGiveItemToPlayer();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}