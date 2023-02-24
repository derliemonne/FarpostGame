using IngameDebugConsole;
using System.Net.NetworkInformation;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraScript : MonoBehaviour
{
    private const float DefaultCameraSize = 9.6f;

    public Character Character => _character;

    private Vector3 Pivot 
    {
        get
        {
            if(_character == null)
            {
                Debug.Log("No binded player");
            }
            return new Vector3(transform.position.x, _player.transform.position.y + _verticalShift, transform.position.z);
        }
            
    }
        
    [SerializeField] private float _verticalShift = 2;
    [SerializeField] private float _distToMove = 3;
    [SerializeField] private float _distToStop = 3;
    [SerializeField] private float _distToTpCamera = 3;
    [SerializeField] private float _cameraAdjustSpeed = 0.3f;
    [SerializeField] private float _chunkWidth = 10.8f;

    private bool _isMove;
    private Transform _player;
    private Character _character;
    private Rigidbody2D _playerRb;
    private Camera _camera;
    private float _pivotLastYCoord;
    
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
        //float ratio = (float)Screen.width / Screen.height;
        //Camera.orthographicSize = _chunkWidth / (ratio * 2);
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
        _pivotLastYCoord = Pivot.y;
        TpCameraToPivot();
    }
    
    public void MoveCamera()
    {
        float pivotMove = Mathf.Abs(_pivotLastYCoord - Pivot.y);
        bool needTp = pivotMove > _distToTpCamera;

        if (needTp)
        {
            TpCameraToPivot();
        }
        else
        {
            float dist = Pivot.y - transform.position.y;
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
                if(_character.GroundChecker.IsGrounded)
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

        _pivotLastYCoord = Pivot.y;
    }
    
    private void TpCameraToPivot()
    {
        transform.position = Pivot;
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
