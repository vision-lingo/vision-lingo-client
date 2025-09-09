using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // A public variable to control the sensitivity of the camera
    public float mouseSensitivity = 100f;

    // A reference to the player's body (the object that will rotate horizontally)
    public Transform playerBody;

    // The current rotation around the X-axis (up and down)
    float xRotation = 0f;
    [SerializeField] private bool _is_X_Mirror;
    [SerializeField] private Transform _eye;

    // The Start method is called before the first frame update
    void Start()
    {
        // Lock the cursor to the center of the screen and make it invisible
        Cursor.lockState = CursorLockMode.Locked;
    }
    float lastMouseX = 0.0f;
    float lastMouseY = 0.0f;
    // The Update method is called once per frame
    void Update()
    {
        // Get the mouse input for both X and Y axes
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        if (Mathf.Abs(lastMouseX - mouseX) > 5 || Mathf.Abs(lastMouseY - mouseY) > 5)
        {
            Debug.Log($"mouseX: {mouseX}, mouseY: {mouseX}");
            Debug.Log($"lastMouseX: {lastMouseX}, lastMouseY: {lastMouseY}");
            return;
        }
        Debug.Log($"mouseX: {mouseX}, mouseY: {mouseX}");

        // Calculate the new rotation for the camera (up and down)
        // We use 'xRotation -= mouseY' because a positive mouseY value means the mouse is moving up, 
        // which should cause the camera to look down (a negative rotation on the x-axis).
        xRotation += _is_X_Mirror ? mouseY : -mouseY;
        // Clamp the rotation so the player can't look all the way around vertically
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Apply the vertical rotation to the camera
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        //_eye.localPosition =
        // Apply the horizontal rotation to the player's body
        playerBody.Rotate(Vector3.up * mouseX);
        lastMouseX = mouseX;
        lastMouseY = mouseY;
    }
}
