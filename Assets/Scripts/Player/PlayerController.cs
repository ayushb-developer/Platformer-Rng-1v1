using System;
using UnityEditor;
using UnityEngine;
using Unity.Netcode;
#if UNITY_EDITOR
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(InputHandler))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : NetworkBehaviour
{
    [SerializeField] SpriteRenderer playerSprite;
    [SerializeField] PlayerSettings settings;
    [SerializeField] DifficultySettings difficultySettings;
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundLayer;

    private InputHandler input;
    float gravity;
    float jumpVelocity;

    public float PlayerVelocityX => rb.linearVelocity.x;
    public float MaxJumpDistance => PlayerVelocityX * settings.timeToApex * 2;

    public PlayerSettings Settings => settings;

    // public Vector3 Position => transform.position;

    Rigidbody2D rb;

    bool grounded;
    bool canMove = false;

    void Awake()
    {
        input = GetComponent<InputHandler>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        if(GameFlow.Instance.State == GameState.WaitingForPlayers || GameFlow.Instance.State == GameState.WaitingToStart)
        {
            Initialize();
        }     
    }

    private void Initialize()
    {
        Debug.Log("Initializing Player");
        canMove = false;
        playerSprite.enabled = false;
        transform.position = new (0, 3, 0);
        rb.gravityScale = 0;

        if(IsOwner)
        {
            playerSprite.color = Color.blue;
        }
        else
        {
            playerSprite.color = Color.orange;
        }
    }

    private void StartPlayer()
    {
        Debug.Log("Starting Player");
        canMove = true;
        playerSprite.enabled = true;

        gravity = 2 * settings.jumpHeight / Mathf.Pow(settings.timeToApex, 2);
        jumpVelocity = gravity * settings.timeToApex;

        rb.gravityScale = gravity / -Physics2D.gravity.y;
    }

    void OnEnable()
    {
        GameFlow.Instance.OnStateChanged += HandleGameState;
    }

    void OnDisable()
    {
        GameFlow.Instance.OnStateChanged -= HandleGameState;
    }

    void Update()
    {
        if(!IsOwner) return;
        Debug.Log($"Input: {input.MoveInput}");

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
        if(!IsOwner) return;
        if (!canMove) return;

        float targetVelocity;// = input.MoveInput.x *settings.baseSpeed;
        targetVelocity = Mathf.Lerp(
            settings.baseSpeed,
            settings.maxSpeed,
            GetLocalDifficulty() * difficultySettings.maxSpeedMultiplier
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
        if (!IsOwner) return;

        SubmitFinishServerRpc();
    }

    [ServerRpc]
    void SubmitFinishServerRpc()
    {
        GameFlow.Instance.FinishGame();
    }

    public void StopMovement()
    {
        canMove = false;

        rb.linearVelocity = Vector2.zero;
    }

    public void OnHitObstacle()
    {
        Debug.Log("Hit Obstacle! Game Over.");
        GameFlow.Instance.FinishGame();
    }

    void HandleGameState(GameState state)
    {
        switch (state)
        {
            case GameState.WaitingToStart:
                Initialize();
                break;

            case GameState.Playing:
                StartPlayer();
                break;

            case GameState.Finished:
                StopMovement();
                break;
        }
    }

    public override void OnNetworkSpawn()
    {
        rb.simulated = IsOwner;

        if (IsOwner)
        {
            CameraFollow cam = FindFirstObjectByType<CameraFollow>();
            cam.SetTarget(transform);
        }
    }
    
    float GetLocalDifficulty() //so that each player can have their own difficulty based on how far they are in the level, not just one global difficulty
    {
        float distance = transform.position.x;

        float normalized = distance / difficultySettings.difficultyRampDistance;

        return difficultySettings.difficultyCurve.Evaluate(normalized);
    }
}