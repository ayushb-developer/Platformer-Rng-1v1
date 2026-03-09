using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public bool JumpPressed { get; private set; }

    PlayerInputActions inputActions;

    Vector2 startTouch;
    float swipeThreshold = 50f;

    void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    void OnEnable()
    {
        inputActions.Enable();

        inputActions.Player.Move.performed += ctx =>
            MoveInput = ctx.ReadValue<Vector2>();

        // inputActions.Player.Move.canceled += ctx =>
        //     MoveInput = Vector2.zero;

        inputActions.Player.Jump.performed += ctx =>
            JumpPressed = true;

        inputActions.Player.TouchPress.started += ctx =>
            startTouch = inputActions.Player.TouchPosition.ReadValue<Vector2>();

        inputActions.Player.TouchPress.canceled += OnTouchReleased;
    }

    void OnDisable()
    {
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