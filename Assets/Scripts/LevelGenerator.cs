using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    //������ ������� ���������
    public GameObject platform;

    //������ ������ ���������
    public GameObject weak_platform;

    //���������� ����� �����������
    public float dist_between_platforms = 5;

    //���� ��������� ������ ���������
    public float weak_platform_chance = 1;

    //���������� �� x ��� �������� �����
    public float left_coord_platform = -3;

    //���������� �� x ��� �������� ������
    public float right_coord_platform = 3;

    //���������� �� y ��������� ������� ���������
    private float next_platform_pos = 1;

    private enum platform_orientation { LEFT, RIGHT };

    //������������ ��������� ������� ���������(������ ��� �����)
    private platform_orientation next_platform_orientation = platform_orientation.LEFT;

    //������ � �����������
    private List<GameObject> platforms_list = new List<GameObject>();

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
