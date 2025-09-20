using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour, IInteractive
{
    public void OnGaze()
    {
        GetComponent<MeshRenderer>().material.color = Color.red;
    }

    public void OnOutofEye()
    {
        GetComponent<MeshRenderer>().material.color = Color.gray;
    }

    public void OnSelect()
    {
        GetComponent<MeshRenderer>().material.color = Color.blue;
    }
}
