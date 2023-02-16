using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateScript : MonoBehaviour
{
    public LevelGenerator level_generator;
    public UpdateZone update_zone;
    public PlayersList player_list;
    public CameraScript camera;
    public DeathZoneScript death_zone_script;

    private void Start()
    {
        while (level_generator.UpdateLevel(update_zone));
        death_zone_script.Change(true);
    }

    private void Update()
    {
        level_generator.UpdateLevel(update_zone);
        camera.MoveCamera();
        death_zone_script.Move();
    }
}
