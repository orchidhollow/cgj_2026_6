using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [Header("参数")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private LayerMask jumpableGround;
    [SerializeField] private AudioSource jumpSoundEffect;
    [SerializeField] private float falling = -5;

    [Header("组件")]
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Animator anim;
    private BoxCollider2D coll;

    /* 3 个动画状态 */
    private enum State { Idle = 0, Running = 1, Jumping = 2 }
    private State currentState;

    private float dirX;
    private bool wasGrounded;   // 用于落地瞬间切回 Idle/Running

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        coll = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        dirX = Input.GetAxisRaw("Horizontal");
        if (Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift))
        {
            dirX = dirX * 1.5f;
        }

        /* 水平移动 */
        rb.velocity = new Vector2(dirX * moveSpeed, rb.velocity.y);

        /* 跳跃 */
        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpSoundEffect.Play();
        }

        /* 动画状态机 */
        UpdateAnimationState();
        wasGrounded = IsGrounded();   // 下一帧用
    }

    private void UpdateAnimationState()
    {
        bool grounded = IsGrounded();

        /* 跳跃状态优先级最高 */
        if (!grounded)
        {
            currentState = State.Jumping;   // 编号 2
        }
        /* 落地瞬间 → 根据输入给 0 或 1 */
        //else if (!wasGrounded && grounded)
        //{
        //    currentState = (dirX != 0) ? State.Running : State.Idle;
        //}
        /* 地面持续状态 */
        else if (grounded)
        {
            currentState = (dirX != 0) ? State.Running : State.Idle;
           

        }

        /* 翻转 */
        if (dirX > 0) sprite.flipX = false;
        else if (dirX < 0) sprite.flipX = true;

        /* 送 Animator */
        anim.SetInteger("state", (int)currentState);

        if (transform.position.y < falling) // 假设玩家跳崖时 y 坐标小于 -5
            Invoke(nameof(Restart), 1f);
    }


    private bool IsGrounded()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size,
                                 0f, Vector2.down, 0.1f, jumpableGround);
    }
    private void RestarLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    void Restart() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
}
