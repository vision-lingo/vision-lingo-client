using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundBox : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private KeyCode _playKey = KeyCode.Space;

    void Update()
    {
        if (Input.GetKeyDown(_playKey))
        {
            Debug.Log("Space 눌림!");
            ToggleSound();
        }
    }

    void ToggleSound()
    {
        if (_audioSource != null)
        {
            if (_audioSource.isPlaying)
            {
                _audioSource.Pause();
            }
            else
            {
                _audioSource.Play();
            }
        }
    }
}
