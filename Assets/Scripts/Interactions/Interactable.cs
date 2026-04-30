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
    [Tooltip("How close the player must be to see the prompt and interact.")]
    public float interactRadius = 2f;

    [Tooltip("Label shown in the interaction prompt. e.g. 'Piano', 'Calendar'")]
    public string interactLabel = "Interact";

    [Tooltip("If false, the player can only interact once ever.")]
    public bool repeatable = true;
        
    [Header("Indicator")]
    [Tooltip("Assign a GameObject to use as the hover indicator (e.g. a pulsing circle quad). ")]
    public GameObject indicatorObject;
 
    [Tooltip("How high above the object's pivot the indicator floats.")]
    public float indicatorHeight = 1.5f;
 
    [Tooltip("Speed of the indicator's up/down float animation.")]
    public float floatSpeed = 1.5f;
 
    [Tooltip("How far the indicator bobs up and down.")]
    public float floatAmount = 0.1f;
 
    [Tooltip("Speed of the indicator's pulse (scale) animation.")]
    public float pulseSpeed = 2f;
 
    [Tooltip("How much the indicator scales up and down while pulsing.")]
    public float pulseAmount = 0.08f;


    private bool _hasInteracted = false;
    private bool _playerInRange = false;
    
    private Vector3 _indicatorBaseLocalPos;
    private Vector3 _indicatorBaseScale;
    private float _animTimer = 0f;

    protected virtual void Awake()
    {
        if (indicatorObject == null)
        {
            Transform found = transform.Find("Indicator");
            if (found)
                indicatorObject = found.gameObject;
        }
 
        if (indicatorObject != null)
        {
            indicatorObject.SetActive(false);
        }
    }


    protected virtual void Update()
    {
        if (_playerInRange)
        {
            AnimateIndicator();
 
            if (Input.GetKeyDown(KeyCode.E))
                TryInteract();
        }

    }
    
    public void OnPlayerEnterRange()
    {
        _playerInRange = true;
 
        if (indicatorObject)
            indicatorObject.SetActive(true);
 
        OnRangeEnter();
    }
 
    public void OnPlayerExitRange()
    {
        _playerInRange = false;
 
        if (indicatorObject)
            indicatorObject.SetActive(false);
 
        OnRangeExit();
    }

    private void TryInteract()
    {
        if (!repeatable && _hasInteracted) return;
        if (!CanInteract()) return;

        _hasInteracted = true;
        OnInteract();
    }

    /// <summary>
    /// Override to add conditions that must be true before interaction is allowed.
    /// Return false to silently block. Default is always true.
    /// </summary>
    protected virtual bool CanInteract() => true;

    /// <summary>
    /// Override this to define what the interactable does.
    /// Typically: open a 2D panel, play dialogue, set a flag.
    /// </summary>
    protected abstract void OnInteract();

    /// <summary>Called when the player enters range. Override to show a custom prompt.</summary>
    protected virtual void OnRangeEnter() { }

    /// <summary>Called when the player exits range. Override to hide a custom prompt.</summary>
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
        Gizmos.DrawWireSphere(this.gameObject.transform.position, interactRadius);
    }
}