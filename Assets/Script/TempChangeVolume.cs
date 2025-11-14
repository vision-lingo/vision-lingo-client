using System.Collections;
using UnityEngine;

public class TempChangeVolume : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        MainSystem.Instance.SoundController.PlayMusic("WhirlwindOfJoy");
    }
}
