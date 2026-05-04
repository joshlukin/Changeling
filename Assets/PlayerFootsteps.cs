using UnityEngine;

public class PlayerFootstepsWwise : MonoBehaviour
{
    [Header("Wwise")]
    [SerializeField] private AK.Wwise.Event footstepEvent;

    [Header("Wwise Switch Group")]
    [SerializeField] private string switchGroupName = "Footstep_Surface";

    [Header("Switch Names")]
    [SerializeField] private string woodSwitch = "Wood";
    [SerializeField] private string carpetSwitch = "Carpet";
    [SerializeField] private string tileSwitch = "Tile";
    [SerializeField] private string defaultSwitch = "Wood";

    [Header("Layer Names")]
    [SerializeField] private string woodLayer = "Floor_Wood";
    [SerializeField] private string carpetLayer = "Floor_Carp";
    [SerializeField] private string tileLayer = "Floor_Tile";

    [Header("Surface Detection")]
    [SerializeField] private Transform raycastOrigin;
    [SerializeField] private float raycastDistance = 1.5f;
    [SerializeField] private LayerMask floorMask;

    [Header("Footstep Timing")]
    [Tooltip("Seconds between footsteps while moving.")]
    [SerializeField] private float stepInterval = 0.5f;

    [Tooltip("Hard minimum time allowed between any two footstep triggers.")]
    [SerializeField] private float minimumTriggerInterval = 0.5f;

    [Tooltip("Minimum horizontal speed required before footsteps play.")]
    [SerializeField] private float minMoveSpeed = 0.1f;

    [Tooltip("Play a footstep immediately when movement starts, but still respects Minimum Trigger Interval.")]
    [SerializeField] private bool playImmediateStepOnMove = true;

    [Header("Ground Check")]
    [SerializeField] private bool requireGrounded = true;
    [SerializeField] private float groundCheckDistance = 1.2f;

    private string currentSwitch;
    private float stepTimer;
    private float globalTriggerCooldown;

    private Vector3 lastPosition;
    private bool wasMoving;

    private void Awake()
    {
        if (raycastOrigin == null)
            raycastOrigin = transform;

        lastPosition = transform.position;
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        if (globalTriggerCooldown > 0f)
            globalTriggerCooldown -= deltaTime;

        float horizontalSpeed = GetHorizontalSpeed(deltaTime);
        bool isMoving = horizontalSpeed > minMoveSpeed;
        bool isGrounded = !requireGrounded || CheckGrounded();

        if (isMoving && isGrounded)
        {
            if (!wasMoving && playImmediateStepOnMove)
            {
                TryPlayFootstep();
                stepTimer = stepInterval;
            }
            else
            {
                stepTimer -= deltaTime;

                if (stepTimer <= 0f)
                {
                    TryPlayFootstep();
                    stepTimer = stepInterval;
                }
            }
        }
        else
        {
            stepTimer = 0f;
        }

        wasMoving = isMoving && isGrounded;
        lastPosition = transform.position;
    }

    private float GetHorizontalSpeed(float deltaTime)
    {
        Vector3 movement = (transform.position - lastPosition) / Mathf.Max(deltaTime, 0.0001f);
        movement.y = 0f;
        return movement.magnitude;
    }

    private bool CheckGrounded()
    {
        Ray ray = new Ray(raycastOrigin.position, Vector3.down);
        return Physics.Raycast(ray, groundCheckDistance, floorMask);
    }

    private void TryPlayFootstep()
    {
        if (globalTriggerCooldown > 0f)
            return;

        PlayFootstep();
        globalTriggerCooldown = minimumTriggerInterval;
    }

    private void PlayFootstep()
    {
        string surfaceSwitch = DetectSurfaceSwitch();

        if (surfaceSwitch != currentSwitch)
        {
            AkUnitySoundEngine.SetSwitch(
                switchGroupName,
                surfaceSwitch,
                gameObject
            );

            currentSwitch = surfaceSwitch;
        }

        footstepEvent.Post(gameObject);
    }

    private string DetectSurfaceSwitch()
    {
        Ray ray = new Ray(raycastOrigin.position, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, floorMask))
        {
            string layerName = LayerMask.LayerToName(hit.collider.gameObject.layer);

            if (layerName == woodLayer)
                return woodSwitch;

            if (layerName == carpetLayer)
                return carpetSwitch;

            if (layerName == tileLayer)
                return tileSwitch;
        }

        return defaultSwitch;
    }

    private void OnDrawGizmosSelected()
    {
        Transform origin = raycastOrigin != null ? raycastOrigin : transform;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(
            origin.position,
            origin.position + Vector3.down * raycastDistance
        );

        Gizmos.color = Color.green;
        Gizmos.DrawLine(
            origin.position,
            origin.position + Vector3.down * groundCheckDistance
        );
    }
}