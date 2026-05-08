using UnityEngine;

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

    // -------------------------------------------------------
    // State
    // -------------------------------------------------------

    protected bool _hasInteracted = false;
    private bool _playerInRange = false;
    private Transform _playerTransform;

    private Vector3 _indicatorBaseLocalPos;
    private Vector3 _indicatorBaseScale;
    private float _animTimer = 0f;

    // -------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------

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

    protected virtual void Start()
    {
        CachePlayer();
    }

    protected virtual void OnEnable()
    {
        // Reset interaction state every time this object is enabled.
        // This is critical for dinner objects that get toggled mid-game by
        // DaySequence scripts — without this, _hasInteracted stays true
        // from a previous enable cycle and blocks all interaction.
        _hasInteracted = false;
        _playerInRange = false;
        _animTimer = 0f;

        CachePlayer();
    }

    private void CachePlayer()
    {
        if (_playerTransform != null) return;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _playerTransform = playerObj.transform;
        else
            Debug.LogWarning($"[Interactable] '{name}' could not find a GameObject tagged 'Player'. " +
                             "Make sure your Player GameObject has the 'Player' tag.");
    }

    protected virtual void Update()
    {
        if (_playerTransform == null)
        {
            CachePlayer();
            return;
        }

        float dist = Vector3.Distance(transform.position, _playerTransform.position);
        bool inRange = dist <= interactRadius;

        // Transition into range
        if (inRange && !_playerInRange)
        {
            _playerInRange = true;
            OnRangeEnter();
        }
        // Transition out of range
        else if (!inRange && _playerInRange)
        {
            _playerInRange = false;
            if (indicatorObject != null)
                indicatorObject.SetActive(false);
            OnRangeExit();
        }

        // While in range — keep indicator and input live
        if (_playerInRange)
        {
            // Refresh indicator every frame — CanInteract may change (flags)
            if (indicatorObject != null)
                indicatorObject.SetActive(CanInteract());

            AnimateIndicator();

            bool uiBusy = (DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying) ||
                          (ScenePanelManager.Instance != null && ScenePanelManager.Instance.IsOpen);

            if (Input.GetKeyDown(KeyCode.E) && !uiBusy)
                TryInteract();
        }
    }

    // -------------------------------------------------------
    // Range — kept for ProximityDetector compatibility
    // but self-managed distance check is the source of truth
    // -------------------------------------------------------

    public void OnPlayerEnterRange() { }
    public void OnPlayerExitRange() { }
    public void RefreshIndicator() { }

    // -------------------------------------------------------
    // Interaction
    // -------------------------------------------------------

    private void TryInteract()
    {
        if (!repeatable && _hasInteracted) return;
        if (!CanInteract()) return;

        _hasInteracted = true;
        OnInteract();
    }

    /// <summary>Reset interaction state — useful for day resets.</summary>
    public void ResetInteraction()
    {
        _hasInteracted = false;
    }

    protected virtual bool CanInteract() => true;
    protected abstract void OnInteract();
    protected virtual void OnRangeEnter() { }
    protected virtual void OnRangeExit() { }

    // -------------------------------------------------------
    // Indicator animation
    // -------------------------------------------------------

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

    // -------------------------------------------------------
    // Editor
    // -------------------------------------------------------

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}