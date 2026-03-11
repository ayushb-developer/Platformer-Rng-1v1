using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(InputHandler))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] PlayerSettings settings;
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundLayer;

    private InputHandler input;
    float gravity;
    float jumpVelocity;

    public float PlayerVelocityX => rb.linearVelocity.x;
    public float MaxJumpDistance => settings.baseSpeed * settings.timeToApex * 2;
    public PlayerSettings Settings => settings;
    Rigidbody2D rb;
    bool grounded;

    void Awake()
    {
        input = GetComponent<InputHandler>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        gravity = 2 * settings.jumpHeight / Mathf.Pow(settings.timeToApex, 2);
        jumpVelocity = gravity * settings.timeToApex;

        rb.gravityScale = gravity / -Physics2D.gravity.y;
    }

    void Update()
    {
        grounded = Physics2D.OverlapCircle(
            groundCheck.position,
            0.2f,
            groundLayer
        );

        if (input.JumpPressed && grounded)
        {
            rb.linearVelocity = new Vector2(PlayerVelocityX, jumpVelocity);
            input.ResetJump();
            // Debug.Log("Max Jump Distance: " + MaxJumpDistance(PlayerVelocityX));
        }
    }

    void FixedUpdate()
    {
        float targetVelocity = input.MoveInput.x *settings.baseSpeed;
        float newX = Mathf.Lerp(PlayerVelocityX, targetVelocity, 10f * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
    }

    // public float MaxJumpDistance(float horizontalSpeed)
    // {
    //     return horizontalSpeed * (timeToApex * 2);
    // }


    void OnDrawGizmos()
    {
        if(!Application.isPlaying) return;
        if(rb == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position,
        transform.position + Vector3.right * MaxJumpDistance);
    }
}