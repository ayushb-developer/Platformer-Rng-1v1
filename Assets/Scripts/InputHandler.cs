using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : NetworkBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public bool JumpPressed { get; private set; }

    System.Action<InputAction.CallbackContext> moveHandler;
    System.Action<InputAction.CallbackContext> jumpHandler;
    System.Action<InputAction.CallbackContext> touchStartHandler;

    PlayerInputActions inputActions;

    Vector2 startTouch;
    float swipeThreshold = 50f;

    void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        inputActions.Enable();

        moveHandler = ctx => MoveInput = ctx.ReadValue<Vector2>();
        jumpHandler = ctx => JumpPressed = true;
        touchStartHandler = ctx => startTouch = inputActions.Player.TouchPosition.ReadValue<Vector2>();

        inputActions.Player.Move.performed += moveHandler;
        inputActions.Player.Jump.performed += jumpHandler;
        inputActions.Player.TouchPress.started += touchStartHandler;
        inputActions.Player.TouchPress.canceled += OnTouchReleased;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;

        inputActions.Player.Move.performed -= moveHandler;
        inputActions.Player.Jump.performed -= jumpHandler;
        inputActions.Player.TouchPress.started -= touchStartHandler;
        inputActions.Player.TouchPress.canceled -= OnTouchReleased;

        inputActions.Disable();
    }

    void OnTouchReleased(InputAction.CallbackContext ctx)
    {
        Vector2 endTouch = inputActions.Player.TouchPosition.ReadValue<Vector2>();
        Vector2 delta = endTouch - startTouch;

        Debug.Log(delta);
        if (Mathf.Abs(delta.x) > swipeThreshold)
        {
            MoveInput = delta.x > 0 ? Vector2.right : Vector2.left;
        }
        else
        {
            JumpPressed = true;
        }
    }

    public void ResetJump()
    {
        JumpPressed = false;
    }
}