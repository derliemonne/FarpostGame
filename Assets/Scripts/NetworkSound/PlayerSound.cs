using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerSound : MonoBehaviour
{
    public static PlayerSound Instance { get; private set; }
    
    [SerializeField] private AudioClip _jumpSound;
    [SerializeField] private AudioClip _deathSound;
    [SerializeField] private AudioClip _buffSound;

    private Character _character;
    private AudioSource _audioSource;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Trying to create another instance of singleton class.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        _audioSource = GetComponent<AudioSource>();
        
        CameraScript cameraScript = CameraScript.Instance;
        if (CameraScript.Instance == null)
        {
            Debug.LogError($"{nameof(cameraScript)} is null.");
        }
        else
        {
            transform.parent = cameraScript.transform;
        }
    }

    public void BindPlayer(Character character)
    {
        _character = character;
        _character.BindPlayerSound(this);
    }

    public void Play_Death()
    {
        Play(_deathSound);
    }

    public void Play_Jump()
    {
        Play(_jumpSound);
    }

    public void Play_Buff()
    {
        Play(_buffSound);
    }

    private void Play(AudioClip clip)
    {
        _audioSource.clip = clip;
        _audioSource.Play();
    }
}
