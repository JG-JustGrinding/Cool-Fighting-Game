using UnityEngine;
using UnityEngine.InputSystem;

// Handles player input and actions and stuff
public class PlayerController : MonoBehaviour
{
    /// <summary>
    ///  These variables will be relative to the environment's scale to support dimension switching.
    /// </summary>
    private PlayerActionControls controls;
    private Vector2 moveInput;

    [SerializeField]
    private Character character;

    void Awake()
    {
        controls = new PlayerActionControls();

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;
    }

    private void OnEnable()
    {
        controls.Player.Enable();
    }

    private void OnDisable()
    {
        controls.Player.Disable();
    }

    void Update()
    {
        character.Move(moveInput.x, moveInput.y);
    }
}
