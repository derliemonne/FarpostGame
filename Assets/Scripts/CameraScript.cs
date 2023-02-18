using IngameDebugConsole;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraScript : MonoBehaviour
{
    private const float DefaultCameraSize = 12;
    
    [SerializeField] private float _distToMove = 3;
    [SerializeField] private float _distToStop = 3;
    
    private bool _isMove;
    private Transform _player;
    private Rigidbody2D _playerRb;
    private Camera _camera;

    private Camera Camera
    {
        get
        {
            if (_camera == null) _camera = GetComponent<Camera>();
            return _camera;
        }
    }

    private void Awake()
    {
        ResetCameraSize();
    }
    
    public void ResetCameraSize()
    {
        Camera.orthographicSize = DefaultCameraSize;
    }
    
    
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
        float dist = transform.position.y - _player.position.y;
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
                transform.Translate(velocity * Time.deltaTime);
            }
            else
            {
                Vector3 velocity = new Vector3(0,
                    Mathf.Min(_playerRb.velocity.y, 0), 0);
                transform.Translate(velocity * Time.deltaTime);
            }
        }
    }
    
    private void OnValidate()
    {
        ResetCameraSize();
    }
    
    [ConsoleMethod("setcam", "Resets camera size to fit screen.")]
    public static void SetCameraSizeCommand()
    {
        Camera.main.GetComponent<CameraScript>().ResetCameraSize();
    }
}
