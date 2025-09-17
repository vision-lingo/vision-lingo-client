using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour, IInteractive
{
    public void Hover()
    {
        GetComponent<MeshRenderer>().material.color = Color.red;
    }

    public void OutOfHand()
    {
        GetComponent<MeshRenderer>().material.color = Color.gray;
    }

    public void Pick()
    {
        GetComponent<MeshRenderer>().material.color = Color.blue;
    }
}
