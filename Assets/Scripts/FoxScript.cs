using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoxScript : Character
{
    public override CharacterType CharacterType => CharacterType.Firsik;
    public override string CharacterName => "Фырсик";

    private static readonly float _distanceFromDarknessRespawn = 30f;

    protected override void Awake()
    {
        base.Awake();
        Health = 2;
    }

    public override void TakeDamage()
    {
        base.TakeDamage();
        if(Health > 0)
        {
            if(Runner.IsServer)
            {
                _networkRb.TeleportToPosition(FindPlaceToRespawn());
            }
        }
    }

    private Vector3 FindPlaceToRespawn()
    {
        Debug.Log("1");
        List<SpawnPoint> spawnPointsList = LevelManager.Instance.GetAllSpawnPoints();
        Vector3 spawnPointPosition = Vector3.zero;

        if (spawnPointsList.Count > 0)
        {
            float darknessAltitude = Darkness.Instance.Altitude;
            foreach (SpawnPoint spawnPoint in spawnPointsList)
            {
                spawnPointPosition = spawnPoint.Position;
                if (spawnPoint.Altitude - darknessAltitude > _distanceFromDarknessRespawn) break;               
            }
        }
        else
        {
            Debug.LogError("No spawnpoints left");
        }    

        return spawnPointPosition;
    }
}
