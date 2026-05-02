using UnityEngine;

public class ProximityDetector : MonoBehaviour
{
    [Tooltip("How often (in seconds) to scan for nearby interactables.")]
    public float scanInterval = 0.15f;

    private Interactable _currentInteractable;
    private float _scanTimer;

    void Update()
    {
        _scanTimer -= Time.deltaTime;
        if (_scanTimer <= 0f)
        {
            _scanTimer = scanInterval;
            ScanForInteractables();
        }
    }

    private void ScanForInteractables()
    {
        Interactable closest = null;
        float closestDist = float.MaxValue;

        foreach (var interactable in FindObjectsByType<Interactable>(FindObjectsSortMode.None))
        {
            float dist = Vector3.Distance(transform.position, interactable.transform.position);
            if (dist <= interactable.interactRadius && dist < closestDist)
            {
                closestDist = dist;
                closest = interactable;
            }
        }

        // Exited range of previous
        if (_currentInteractable != null && _currentInteractable != closest)
        {
            _currentInteractable.OnPlayerExitRange();
            _currentInteractable = null;
        }

        // Entered range of new
        if (closest != null && closest != _currentInteractable)
        {
            _currentInteractable = closest;
            _currentInteractable.OnPlayerEnterRange();
        }

        // Already in range — refresh indicator in case CanInteract changed
        // (e.g. kitchen counter after brunch is made)
        if (_currentInteractable != null && _currentInteractable == closest)
        {
            _currentInteractable.RefreshIndicator();
        }
    }
}