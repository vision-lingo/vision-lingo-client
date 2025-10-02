using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxController : MonoBehaviour
{
    [SerializeField] private Material _skyboxMat;
    [SerializeField] private float _rotSpeed;
    // Update is called once per frame
    void Update()
    {
        _skyboxMat.SetFloat("_Rotation", Time.time * _rotSpeed);
    }
}
