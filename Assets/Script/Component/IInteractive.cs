using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractive
{
    public void OnGaze();
    public void OnSelect();
    public void OnOutofEye();
}
