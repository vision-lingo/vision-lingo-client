using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicboxController : MonoBehaviour
{
    [SerializeField] private GameObject _musicBox;
    [SerializeField] private float _radius;
    [SerializeField] private float _speed;

    private bool _isHorizontal = true;


    void Start()
    {
        
    }
    
    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(0))
            _isHorizontal = !_isHorizontal;

        if (_isHorizontal)
            _musicBox.transform.position = new Vector3(0, 0, -10) + new Vector3(Mathf.Sin(Mathf.PI * Time.time * _speed) * _radius, 0, Mathf.Cos(Mathf.PI  * Time.time * _speed) * _radius);
        else
            _musicBox.transform.position = new Vector3(0, 0, -10) + new Vector3(0, Mathf.Sin(Mathf.PI * Time.time * _speed) * _radius, Mathf.Cos(Mathf.PI * Time.time * _speed) * _radius);
    }
}
