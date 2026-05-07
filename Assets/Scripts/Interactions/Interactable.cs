using UnityEngine;

/// <summary>
/// Base class for all interactable objects in the 3D space.
/// Attach to any object the player can walk up to and press E on.
/// INDICATOR SETUP:
/// Create a child GameObject on this object named "Indicator".
/// Add a world-space UI Canvas (or a simple quad with a material) to it.
/// The indicator will pulse and show/hide based on player proximity.
/// Alternatively, assign any GameObject to the indicatorObject field in the Inspector.
/// </summary>

public abstract class Interactable : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactRadius = 2f;
    public string interactLabel = "Interact";
    public bool repeatable = true;

    [Header("Indicator")]
    public GameObject indicatorObject;
    public float indicatorHeight = 1.5f;
    public float floatSpeed = 1.5f;
    public float floatAmount = 0.1f;
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.08f;

    private bool _hasInteracted = false;
    private bool _playerInRange = false;

    private Vector3 _indicatorBaseLocalPos;
    private Vector3 _indicatorBaseScale;
    private float _animTimer = 0f;

    [Header("References")]
    [SerializeField] public DayManager dayManager;
    protected virtual void Awake()
    {
        if (indicatorObject == null)
        {
            Transform found = transform.Find("Indicator");
            if (found)
                indicatorObject = found.gameObject;
        }

        if (indicatorObject != null)
            indicatorObject.SetActive(false);
    }

    protected virtual void Update()
    {
        if (_playerInRange)
        {
            AnimateIndicator();

            // Check if the UI is currently busy before accepting interaction input
            bool uiBusy = (DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying) || 
                          (ScenePanelManager.Instance != null && ScenePanelManager.Instance.IsOpen);

            if (Input.GetKeyDown(KeyCode.E) && !uiBusy)
                TryInteract();
        }
    }

    public void OnPlayerEnterRange()
    {
        _playerInRange = true;
        
        if (indicatorObject != null)
            indicatorObject.SetActive(CanInteract());

        OnRangeEnter();
    }

    public void OnPlayerExitRange()
    {
        _playerInRange = false;

        if (indicatorObject != null)
            indicatorObject.SetActive(false);

        OnRangeExit();
    }

    /// <summary>
    /// Called by ProximityDetector each scan while player is in range,
    /// so the indicator can update if CanInteract state changes mid-proximity.
    /// </summary>
    public void RefreshIndicator()
    {
        if (!_playerInRange) return;
        if (indicatorObject != null)
            indicatorObject.SetActive(CanInteract());
    }

    private void TryInteract()
    {
        if (!repeatable && _hasInteracted) return;
        if (!CanInteract()) return;

        _hasInteracted = true;
        OnInteract();
    }

    protected virtual bool CanInteract() => true;

    protected abstract void OnInteract();

    protected virtual void OnRangeEnter() { }
    protected virtual void OnRangeExit() { }

    private void AnimateIndicator()
    {
        if (!indicatorObject) return;

        if (_animTimer == 0f)
        {
            _indicatorBaseLocalPos = indicatorObject.transform.localPosition;
            _indicatorBaseScale = indicatorObject.transform.localScale;
        }

        _animTimer += Time.deltaTime;

        Vector3 baseWorldPos = transform.TransformPoint(_indicatorBaseLocalPos);
        float yOffset = Mathf.Sin(_animTimer * floatSpeed) * floatAmount;
        indicatorObject.transform.position = baseWorldPos + Vector3.up * yOffset;

        float scaleMod = 1f + Mathf.Sin(_animTimer * pulseSpeed) * pulseAmount;
        indicatorObject.transform.localScale = _indicatorBaseScale * scaleMod;

        if (Camera.main)
        {
            indicatorObject.transform.rotation = Quaternion.LookRotation(
                -(Camera.main.transform.position - indicatorObject.transform.position)
            );
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}