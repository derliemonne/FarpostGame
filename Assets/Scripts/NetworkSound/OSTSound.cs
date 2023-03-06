using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class OSTSound : MonoBehaviour
{
    private AudioSource _ostAudioSource;

    private void Awake()
    {
        _ostAudioSource = GetComponent<AudioSource>();  
    }

    private void Start()
    {
        GameStateHandler gameStateHandler = GameStateHandler.Instance;

        gameStateHandler.RaceStartedChanged += (raceStarted) =>
        {
            if(raceStarted)
            {
                _ostAudioSource.Play();
            }
        };

        gameStateHandler.RaceFinished += (recordsList) =>
        {
            _ostAudioSource.Stop();
        };
    }
}
