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
    [SerializeField] private bool _is_X_Mirror;
    [SerializeField] private Transform _eye;

    private float xRotation = 0f;
    private float lastMouseX = 0.0f;
    private float lastMouseY = 0.0f;

    private IInteractive _interObj = null;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }


    void Update()
    {
        Ray ray = new Ray(playerBody.transform.position, playerBody.transform.forward * 100);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit))
        {

            if (hit.transform.TryGetComponent(out _interObj))
            {
                _interObj.Hover();
                if (Input.GetMouseButton(0))
                {
                    _interObj.Pick();
                }
            }
        }
        else
        {
            if (_interObj != null)
            {
                _interObj.OutOfHand();
                _interObj = null;
            }
        }
        Debug.DrawRay(playerBody.transform.position, playerBody.transform.forward * 100);


        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        if (Mathf.Abs(lastMouseX - mouseX) > 5 || Mathf.Abs(lastMouseY - mouseY) > 5)
        {
            Debug.Log($"mouseX: {mouseX}, mouseY: {mouseX}");
            Debug.Log($"lastMouseX: {lastMouseX}, lastMouseY: {lastMouseY}");
            return;
        }
        Debug.Log($"mouseX: {mouseX}, mouseY: {mouseX}");

        xRotation += _is_X_Mirror ? mouseY : -mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
        lastMouseX = mouseX;
        lastMouseY = mouseY;
    }
}
