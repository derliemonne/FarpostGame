using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    //префаб обычной платформы
    public GameObject platform;

    //префаб шаткой платформы
    public GameObject weak_platform;

    //расстояние между платформами
    public float dist_between_platforms = 5;

    //шанс генерации шаткой платформы
    public float weak_platform_chance = 1;

    //координата по x для платформ слева
    public float left_coord_platform = -3;

    //координата по x для платформ справа
    public float right_coord_platform = 3;

    //координаты по y следующей обычной платформы
    private float next_platform_pos = 1;

    private enum platform_orientation { LEFT, RIGHT };

    //расположение следующей обычной платформы(справа или слева)
    private platform_orientation next_platform_orientation = platform_orientation.LEFT;

    //список с платформами
    private List<GameObject> platforms_list = new List<GameObject>();

    public void AddNewChunk()
    {

    }

    public void DestroyLastChunk()
    {

    }

    public bool UpdateLevel(UpdateZone update_zone)
    {
        bool some_changes = false;
        if (platforms_list.Count == 0 || platforms_list[platforms_list.Count - 1].transform.position.y <= update_zone.GetTop())
        {
            GenerateNextPlatform();
            some_changes = true;
        }
        if (platforms_list[0].transform.position.y <= update_zone.GetBottom())
        {
            DestroyLastPlatform();
            some_changes = true;
        }
        return some_changes;
    }

    public Transform GetPlatformToPlace()
    {
        return platforms_list[1].transform.GetChild(0);
    }

    private void GenerateNextPlatform()
    {
        GeneratePlatform();
    }

    private void DestroyLastPlatform()
    {
        Destroy(platforms_list[0]);
        platforms_list.RemoveAt(0);
    }

    private void GeneratePlatform()
    {
        Vector3 new_platform_coords = new Vector3(
            next_platform_orientation == platform_orientation.LEFT ? left_coord_platform : right_coord_platform,
            next_platform_pos,
            0);
        platforms_list.Add(Instantiate(platform, new_platform_coords, new Quaternion()));

        next_platform_orientation = next_platform_orientation == platform_orientation.LEFT ? platform_orientation.RIGHT : platform_orientation.LEFT;
        next_platform_pos += dist_between_platforms;
    }

    private void GenerateWeakPlatform()
    {

    }
}
