using Newtonsoft.Json.Bson;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [SerializeField] private Transform _camera;
    [SerializeField] private float _distToMove;
    [SerializeField] private float _distToStop;
    private bool _isMove = false;
    private Transform _player = null;
    private Rigidbody2D _playerRb = null;

    private void Update()
    {
        if(_player && _playerRb)
        {
            MoveCamera();
        }    
    }

    public void BindPlayer(Transform player, Rigidbody2D playerRb)
    {
        _player = player;
        _playerRb = playerRb;   
    }

    public void MoveCamera()
    {
        float dist = _camera.position.y - _player.position.y;
        if(Mathf.Abs(dist) > _distToMove)
        {
            _isMove = true;
        }
        if(Mathf.Abs(dist) < _distToStop)
        {
            _isMove = false;
        }

        if(_isMove)
        {
            if(dist < 0)
            {
                Vector3 velocity = new Vector3(0,
                    Mathf.Max(_playerRb.velocity.y, 0), 0);
                _camera.Translate(velocity * Time.deltaTime);
            }
            else
            {
                Vector3 velocity = new Vector3(0,
                    Mathf.Min(_playerRb.velocity.y, 0), 0);
                _camera.Translate(velocity * Time.deltaTime);
            }
        }
    }
}
