using UnityEngine;

[RequireComponent(typeof(InputHandler))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 8f;
    [SerializeField] float jumpForce = 14f;

    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundLayer;

    private InputHandler input;
    
    Rigidbody2D rb;
    bool grounded;

    void Awake()
    {
        input = GetComponent<InputHandler>();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
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
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            input.ResetJump();
        }
    }

    void FixedUpdate()
    {
        float targetVelocity = input.MoveInput.x *moveSpeed;
        float newX = Mathf.Lerp(rb.linearVelocity.x, targetVelocity, 10f * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
    }
}