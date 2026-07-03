using UnityEngine;

public class OriginalFollowMovement : MonoBehaviour
{
    [Header("Follow Configuration (No Modification Required)")]
    [SerializeField] private float normalTolerance = 0.8f;
    private bool isOnPlatform = false;
    private Rigidbody2D rb;
    private Transform followRoot;
    private Vector3 lastRootPosition;
    private Vector3 originalLocalScale;
    private Quaternion originalLocalRotation;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component missing! Cannot follow platform", this);
            enabled = false;
            return;
        }
        originalLocalScale = transform.localScale;
        originalLocalRotation = transform.localRotation;
        transform.SetParent(null);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") && !isOnPlatform)
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y >= normalTolerance)
                {
                    followRoot = collision.gameObject.GetComponentInParent<ControllablePlatformMovement>()?.transform;
                    if (followRoot != null)
                    {
                        isOnPlatform = true;
                        lastRootPosition = followRoot.position;
                        Debug.Log("Started following moving platform, free left/right movement allowed", this);
                    }
                    break;
                }
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") && isOnPlatform)
        {
            isOnPlatform = false;
            followRoot = null;
            Debug.Log("Stopped following moving platform", this);
        }
    }

    void Update()
    {
        if (gameObject.CompareTag("Player") && Input.GetKeyDown(KeyCode.W) && isOnPlatform)
        {
            isOnPlatform = false;
            followRoot = null;
        }
    }

    void LateUpdate()
    {
        if (isOnPlatform && followRoot != null)
        {
            Vector3 rootDelta = followRoot.position - lastRootPosition;
            transform.position += rootDelta;
            lastRootPosition = followRoot.position;

            transform.localScale = originalLocalScale;
            transform.localRotation = originalLocalRotation;
        }
    }
}
