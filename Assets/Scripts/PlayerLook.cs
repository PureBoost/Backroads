using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public float mouseSensitivity = 7f;

    private float yRotation = 0f;

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X");

        yRotation += mouseX * mouseSensitivity;

        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }
}