using UnityEngine;

public class CameraController : MonoBehaviour{
  public float mouseSensitivity = 7f;
  private float xRotation = 0f;

void Start(){

    // Locks the cursor to the center of the screen and makes it invisible
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
}

  void Update(){

        // Gets the amount the mouse moved horizontally
        float mouseX = Input.GetAxis("Mouse X");

        // Gets the amount the mouse moved vertically
        float mouseY = Input.GetAxis("Mouse Y");

        // Adds mouse movement to the camera's rotation values
        // xRotation controls looking up/down
        xRotation -= mouseY * mouseSensitivity;

        // Limits the camera's vertical rotation so it cannot flip upside down (only look 90 degrees up or down)
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Applies the stored X and Y rotation values to the camera
        // Keeps Z rotation at 0 to prevent the camera from tilting sideways
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

    }
}
