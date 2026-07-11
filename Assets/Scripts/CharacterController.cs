using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float playerSpeed = 5f;

    private CharacterController controller;

    public InputActionReference moveAction;

    private Vector3 velocity;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
    }

    private void Update()
    {
        // Get the WASD input as a Vector2
        Vector2 input = moveAction.action.ReadValue<Vector2>();

        // Convert 2D input into 3D movement based on player direction
        Vector3 move = transform.right * input.x + transform.forward * input.y;

        // Move the character using speed and frame time (Time.deltaTime = time since last frame (FPS))
        controller.Move(move * playerSpeed * Time.deltaTime);

        // Apply gravity
        if (controller.isGrounded && velocity.y < 0){
            velocity.y = -2f;
        }
        velocity.y += Physics.gravity.y * Time.deltaTime;

        // Move the character vertically
        controller.Move(velocity * Time.deltaTime);
    }

    
}
