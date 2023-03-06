using System;
using IngameDebugConsole;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraScript : MonoBehaviour
{
    private const float DefaultCameraSize = 9.6f;
    private const float LerpDuration = 1f;

    public static CameraScript Instance { get; private set; }
    
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
        if (Instance != null)
        {
            Destroy(gameObject);
            Debug.LogError("Trying to create another instance of singleton class.");
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        ResetCameraSize();
    }

    /// <summary>
    /// Should be called only in runtime and not in editor.
    /// Otherwise it can use the size of editor window instead of the size of the game window.
    /// </summary>
    public void ResetCameraSize()
    {
        Debug.Log($"Set camera size. Screen is {Screen.width}x{Screen.height}");
        float ratio = (float)Screen.width / Screen.height;
        Camera.orthographicSize = _chunkWidth / (ratio * 2);
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
            LerpCameraToPivot();
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

    private void LerpCameraToPivot()
    {
        StartCoroutine(LerpCameraToPoint(transform.position, Pivot));
    }

    private IEnumerator LerpCameraToPoint(Vector3 startPoint, Vector3 endPoint)
    {
        float elapsedTime = 0;
        while (elapsedTime < LerpDuration)
        {
            float delta = elapsedTime / LerpDuration;
            transform.position = Vector3.Lerp(startPoint, endPoint, delta);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = endPoint;
    }

    [ConsoleMethod("setcam", "Resets camera size to fit screen.")]
    public static void SetCameraSizeCommand()
    {
        Camera.main.GetComponent<CameraScript>().ResetCameraSize();
    }

    private void OnApplicationQuit()
    {
        Instance = null;
        Destroy(gameObject);
    }
}
