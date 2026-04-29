using UnityEngine;

public class ProximityDetector : MonoBehaviour
{
    [Tooltip("How often (in seconds) to scan for nearby interactables. 0.1-0.2 is ideal.")]
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

        // Iterate all interactables and find the closest one in range.
        // FindObjectsByType is called on a timer (not every frame) so the
        // performance cost is acceptable for a small scene like this.
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
        if (_currentInteractable && _currentInteractable != closest)
        {
            _currentInteractable.OnPlayerExitRange();
            _currentInteractable = null;
        }

        // Entered range of new
        if (closest && closest != _currentInteractable)
        {
            _currentInteractable = closest;
            _currentInteractable.OnPlayerEnterRange();
        }
    }
}