using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundBox : MonoBehaviour, IInteractive
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private KeyCode _playKey = KeyCode.Space;
    [SerializeField] private MeshRenderer _meshRenderer;

    private void Start()
    {
        if (!TryGetComponent(out _meshRenderer))
            Debug.LogError("_meshRenderer is not found.");
    }

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

    public void OnGaze()
    {
        if(_meshRenderer != null)
            _meshRenderer.material.color = Color.red;
    }

    public void OnOutofEye()
    {
        if (_meshRenderer != null)
            _meshRenderer.material.color = Color.gray;
    }

    public void OnSelect()
    {
        if (_meshRenderer != null)
            _meshRenderer.material.color = Color.blue;
    }
}
