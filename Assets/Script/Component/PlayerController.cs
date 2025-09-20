using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Transform _playerBody;
    [SerializeField] private float _mouseSensitivity = 100f;
    [SerializeField] private bool _is_X_Mirror;

    private float _xRotation = 0f;
    private float _lastMouseX = 0.0f;
    private float _lastMouseY = 0.0f;

    private IInteractive _interObj = null;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }


    void Update()
    {
        Ray ray = new Ray(_playerBody.transform.position, _playerBody.transform.forward * 100);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit))
        {

            if (hit.transform.TryGetComponent(out _interObj))
            {
                _interObj.OnGaze();
                if (Input.GetMouseButton(0))
                {
                    _interObj.OnSelect();
                }
            }
        }
        else
        {
            if (_interObj != null)
            {
                _interObj.OnOutofEye();
                _interObj = null;
            }
        }
        Debug.DrawRay(_playerBody.transform.position, _playerBody.transform.forward * 100);


        float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity * Time.deltaTime;
        if (Mathf.Abs(_lastMouseX - mouseX) > 5 || Mathf.Abs(_lastMouseY - mouseY) > 5)
        {
            Debug.Log($"mouseX: {mouseX}, mouseY: {mouseX}");
            Debug.Log($"lastMouseX: {_lastMouseX}, lastMouseY: {_lastMouseY}");
            return;
        }
        Debug.Log($"mouseX: {mouseX}, mouseY: {mouseX}");

        _xRotation += _is_X_Mirror ? mouseY : -mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        _playerBody.Rotate(Vector3.up * mouseX);
        _lastMouseX = mouseX;
        _lastMouseY = mouseY;
    }
}
