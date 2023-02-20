using IngameDebugConsole;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraScript : MonoBehaviour
{
    private const float DefaultCameraSize = 12;
    
    [SerializeField] private float _distToMove = 3;
    [SerializeField] private float _distToStop = 3;
    [SerializeField] private float _distToTpCamera = 3;
    [SerializeField] private float _cameraAdjustSpeed = 0.3f;

    private bool _isMove;
    private Transform _player;
    private Character _character;
    private Rigidbody2D _playerRb;
    private Camera _camera;
    private float _playerLastYCoord;
    
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

    public void BindPlayer(Transform player, Rigidbody2D playerRb, Character character)
    {
        _player = player;
        _playerRb = playerRb;
        _character = character;
        _playerLastYCoord = _player.position.y;
    }

    
    public void MoveCamera()
    {
        float playerMove = Mathf.Abs(_playerLastYCoord - _player.position.y);
        bool needTp = playerMove > _distToTpCamera;

        if (needTp)
        {
            Vector3 newCameraPos = transform.position;
            newCameraPos.y = _player.position.y;
            transform.position = newCameraPos;
        }
        else
        {
            float dist = _player.position.y - transform.position.y;
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
                if(_character.GroundCheck())
                {
                    Vector3 adjustVector = new Vector3(0, Mathf.Sign(dist) * _cameraAdjustSpeed);
                    transform.Translate(adjustVector * Time.deltaTime);
                }
                else
                {
                    if (dist > 0)
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
        }

        _playerLastYCoord = _player.position.y;
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
