using System;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(InputHandler))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] PlayerSettings settings;
    [SerializeField] DifficultySettings difficultySettings;
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundLayer;

    private InputHandler input;
    float gravity;
    float jumpVelocity;

    public float PlayerVelocityX => rb.linearVelocity.x;
    public float MaxJumpDistance => settings.baseSpeed * settings.timeToApex * 2;
    public PlayerSettings Settings => settings;
    Rigidbody2D rb;
    private LevelGenerator levelGenerator;
    bool grounded;
    bool canMove = true;

    void Awake()
    {
        input = GetComponent<InputHandler>();
        rb = GetComponent<Rigidbody2D>();
        levelGenerator = FindFirstObjectByType<LevelGenerator>();
    }

    void Start()
    {
        gravity = 2 * settings.jumpHeight / Mathf.Pow(settings.timeToApex, 2);
        jumpVelocity = gravity * settings.timeToApex;

        rb.gravityScale = gravity / -Physics2D.gravity.y;
    }

    void Update()
    {
        if (!canMove) return;

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
#if UNITY_EDITOR
    if (Keyboard.current.rKey.wasPressedThisFrame)
    {
        transform.position = new Vector3(transform.position.x, 2, transform.position.z);
        rb.linearVelocity = Vector2.zero;
    }
#endif
    }

    void FixedUpdate()
    {
        if (!canMove) return;

        float targetVelocity;// = input.MoveInput.x *settings.baseSpeed;
        targetVelocity = Mathf.Lerp(
            settings.baseSpeed,
            settings.maxSpeed,
            levelGenerator.Difficulty * difficultySettings.maxSpeedMultiplier
        ) * input.MoveInput.x;
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

    public void OnReachedFinish()
    {
        Debug.Log(name + " finished!");
        // if(finished)
        //     return;

        // finished = true; TODO: add a finished variable to prevent multiple calls, or mark 1st and 2nd place

        StopMovement();
    }


    public void StopMovement()
    {
        canMove = false;

        rb.linearVelocity = Vector2.zero;
    }

    public void OnHitObstacle()
    {
        Debug.Log(name + " hit an obstacle!");
        StopMovement();
    }
}