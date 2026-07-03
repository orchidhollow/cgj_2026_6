using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Unity 6 compatible - controllable reciprocating moving platform (fixes path vs. Gizmos mismatch)
/// Core fixes: wrong Lerp start point, forced initial position sync, added debug logs
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class ControllablePlatformMovement : MonoBehaviour
{
    [Header("Reciprocating movement parameters (configure in editor)")]
    [Tooltip("Start point A: platform initial position (green Gizmos sphere)")]
    [SerializeField] private Vector3 startPointA = new Vector3(0, 2, 0);

    [Tooltip("End point B: target position (red Gizmos sphere)")]
    [SerializeField] private Vector3 endPointB = new Vector3(5, 2, 0);

    [Tooltip("Single leg duration (A→B / B→A) in seconds, min 0.1s")]
    [SerializeField] private float moveDuration = 2f;

    [Tooltip("Stop time at endpoints in seconds, 0 means no stop")]
    [SerializeField] private float stopDuration = 1f;

    [Header("Physics config (no need to change)")]
    [SerializeField] private bool usePhysicsMovement = true;

    // core components & state
    private Rigidbody2D platformRigidbody;
    private float phaseTimer;          // timer for move/stop phases
    private MovementState currentState;

    /// <summary>
    /// platform movement state machine
    /// </summary>
    private enum MovementState
    {
        MoveToB,       // moving to end B
        StopAtB,       // stopping at B
        MoveBackToA,   // returning to start A
        StopAtA        // stopping at A
    }

    #region Unity life-cycle (Unity 6 compatible)
    private void Start()
    {
        InitRigidbody();

        // core fix 1: force initial position to configured startPointA (eliminates offset)
        transform.position = startPointA;
        Debug.Log($"[PlatformMove] Initial position synced to configured start A: {startPointA}, current Transform: {transform.position}", this);

        currentState = MovementState.MoveToB;
        phaseTimer = 0f;

        // parameter validation (avoid bad values)
        moveDuration = Mathf.Max(0.1f, moveDuration);
        stopDuration = Mathf.Max(0f, stopDuration);
    }

    /// <summary>
    /// physics update (smooth interaction with player)
    /// </summary>
    private void FixedUpdate()
    {
        UpdateMovementState();
    }
    #endregion

    #region Core logic (fixes wrong movement path)
    /// <summary>
    /// init rigidbody (Unity 6 kinematic setup)
    /// </summary>
    private void InitRigidbody()
    {
        platformRigidbody = GetComponent<Rigidbody2D>();
        if (platformRigidbody == null)
        {
            platformRigidbody = gameObject.AddComponent<Rigidbody2D>();
            Debug.LogWarning("[PlatformMove] Added missing Rigidbody2D", this);
        }

        // Unity 6 rigidbody settings
        platformRigidbody.bodyType = RigidbodyType2D.Kinematic;
        platformRigidbody.gravityScale = 0f;
        platformRigidbody.freezeRotation = true;
        platformRigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    /// <summary>
    /// state machine for reciprocating motion
    /// </summary>
    private void UpdateMovementState()
    {
        switch (currentState)
        {
            case MovementState.MoveToB:
                // core fix 2: pass correct start (A) and end (B)
                SmoothMoveToTarget(startPointA, endPointB, MovementState.StopAtB);
                break;
            case MovementState.StopAtB:
                StopAtTarget(endPointB, MovementState.MoveBackToA);
                break;
            case MovementState.MoveBackToA:
                // core fix 2: pass correct start (B) and end (A)
                SmoothMoveToTarget(endPointB, startPointA, MovementState.StopAtA);
                break;
            case MovementState.StopAtA:
                StopAtTarget(startPointA, MovementState.MoveToB);
                break;
        }
    }

    /// <summary>
    /// smooth move to target (fixes wrong Lerp start)
    /// </summary>
    /// <param name="currentStart">start of current leg (A when A→B, B when B→A)</param>
    /// <param name="targetPos">destination</param>
    /// <param name="nextState">state after arrival</param>
    private void SmoothMoveToTarget(Vector3 currentStart, Vector3 targetPos, MovementState nextState)
    {
        phaseTimer += Time.fixedDeltaTime;
        float t = Mathf.Clamp01(phaseTimer / moveDuration);

        // core fix 3: Lerp from current start to target (matches Gizmos path)
        Vector3 newPos = Vector3.Lerp(currentStart, targetPos, t);
        newPos = new Vector3(newPos.x, Mathf.Round(newPos.y * 100f) / 100f, newPos.z);

        // Unity 6 kinematic movement (no jitter)
        if (usePhysicsMovement && platformRigidbody.bodyType == RigidbodyType2D.Kinematic)
            platformRigidbody.MovePosition(newPos);
        else
            transform.position = newPos;

        if (t >= 1f)
        {
            Debug.Log($"[PlatformMove] Reached {targetPos}, switching to stop", this);
            currentState = nextState;
            phaseTimer = 0f;
        }
    }

    /// <summary>
    /// stop at target for specified duration (locks position to avoid drift)
    /// </summary>
    private void StopAtTarget(Vector3 targetPos, MovementState nextState)
    {
        // lock position to eliminate Unity 6 physics micro-drift
        if (usePhysicsMovement && platformRigidbody.bodyType == RigidbodyType2D.Kinematic)
            platformRigidbody.MovePosition(targetPos);
        else
            transform.position = targetPos;

        phaseTimer += Time.fixedDeltaTime;
        if (phaseTimer >= stopDuration)
        {
            Debug.Log($"[PlatformMove] Stop finished, moving to {nextState}", this);
            currentState = nextState;
            phaseTimer = 0f;
        }
    }
    #endregion

    #region Editor visualization (Gizmos match actual path)
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // log to verify Gizmos coords match config
        Debug.Log($"[Gizmos] Drawing A: {startPointA}  B: {endPointB}", this);

        // start A: green sphere
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(startPointA, 0.15f);
        Handles.Label(startPointA, $"Start A: {startPointA:F2}");

        // end B: red sphere
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(endPointB, 0.15f);
        Handles.Label(endPointB, $"End B: {endPointB:F2}");

        // path line
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(startPointA, endPointB);
    }

    private void OnDrawGizmosSelected()
    {
        Handles.Label(startPointA, $"Start A: {startPointA:F2}");
        Handles.Label(endPointB, $"End B: {endPointB:F2}");
    }
#endif
    #endregion
}
