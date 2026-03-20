using System;
using UnityEditor;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.VFX;
using Unity.VisualScripting;


#if UNITY_EDITOR
using UnityEngine.InputSystem;
#endif

public enum PlayerState
{
    None,
    Alive,
    Dead,
    Finished
}

[RequireComponent(typeof(InputHandler))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : NetworkBehaviour
{
    public static PlayerController LocalPlayer {get ; private set;}

    [SerializeField] PlayerSettings settings;
    [SerializeField] DifficultySettings difficultySettings;
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] PlayerVisual playerVisual;
    
    private InputHandler input;
    float gravity;
    float jumpVelocity;

    public float PlayerVelocityX => rb.linearVelocity.x;
    public float MaxJumpDistance => PlayerVelocityX * settings.timeToApex * 2;

    public PlayerSettings Settings => settings;
    public bool IsReady => IsSpawned;
    public PlayerState MyPlayerState {get; private set;} = PlayerState.None;
    // public Vector3 Position => transform.position;

    Rigidbody2D rb;

    bool grounded;
    bool canMove = false;
    private Vector3 startingPosition = new Vector3(0, 3, 0);

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
        playerVisual.DisableVisual();
        transform.position = startingPosition;
        rb.gravityScale = 0;
        // playerVisual.Initialize(rb, IsOwner);
    }

    private void StartPlayer()
    {
        Debug.Log("Starting Player");
        MyPlayerState = PlayerState.Alive;
        // transform.position = startingPosition;
        canMove = true;
        // input.InitInput(); //temporary comment out initial input for testing

        gravity = 2 * settings.jumpHeight / Mathf.Pow(settings.timeToApex, 2);
        jumpVelocity = gravity * settings.timeToApex;

        rb.gravityScale = gravity / -Physics2D.gravity.y;
        playerVisual.Initialize(rb, IsOwner);
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
        if(!IsSpawned) return;
        if(MyPlayerState == PlayerState.Dead || MyPlayerState == PlayerState.Finished) return;
        grounded = Physics2D.OverlapCircle(
            groundCheck.position,
            0.2f,
            groundLayer
        );
        playerVisual.UpdateVisual(grounded);
            
        if (!IsOwner) return;
        // Debug.Log($"Input: {input.MoveInput}");
        if(transform.position.y < settings.deathY)
        {
            Debug.Log("Fell to death! Game Over for this player.", this);
            OnDeath();
        }
        if (!canMove) return;

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

        MyPlayerState = PlayerState.Finished;
        SubmitFinishServerRpc();
        GameUI.Instance?.ShowLocalResult();
    }

    [ServerRpc]
    void SubmitFinishServerRpc()
    {
        RaceManager.Instance.RegisterFinish(this);
    }


    // [ServerRpc]
    // void SubmitFinishServerRpc()
    // {
    //     GameFlow.Instance.FinishGame();
    // }

    public void StopMovement()
    {
        canMove = false;

        rb.linearVelocity = Vector2.zero;
    }

    public void OnHitObstacle()
    {
        // GameFlow.Instance.FinishGame();
        if(!IsOwner) return;
        Debug.Log("Hit Obstacle! Game Over for this player.");
        // RaceManager.Instance.RegisterDeath(OwnerClientId);
        OnDeath();
        StopMovement();
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

        if (!IsOwner) return;
        
        LocalPlayer = this;
        RaceManager.Instance.RegisterPlayer(OwnerClientId);
        CameraFollow cam = FindFirstObjectByType<CameraFollow>();
        cam.SetTarget(transform);
    }
    
    float GetLocalDifficulty() //so that each player can have their own difficulty based on how far they are in the level, not just one global difficulty
    {
        float distance = transform.position.x;

        float normalized = distance / difficultySettings.difficultyRampDistance;

        return difficultySettings.difficultyCurve.Evaluate(normalized);
    }

    public void OnDeath()
    {
        if (!IsOwner) return;
        if(MyPlayerState == PlayerState.Dead) return;

        MyPlayerState = PlayerState.Dead;
        StopMovement();
        SubmitDeathServerRpc();

        GameUI.Instance?.ShowLocalResult();
        
        CameraFollow cam = FindFirstObjectByType<CameraFollow>();
        if(RaceManager.Instance.LeadingPlayer != null)
        {
            cam.SetTarget(RaceManager.Instance.LeadingPlayer.transform);
        }
    }

    [ServerRpc]
    void SubmitDeathServerRpc(ServerRpcParams rpcParams = default)
    {   
        ulong clientId = OwnerClientId;
        RaceManager.Instance.RegisterDeath(clientId);
    }
}