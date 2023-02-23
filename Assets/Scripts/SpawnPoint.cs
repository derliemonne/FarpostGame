using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public float Altitude => gameObject.transform.position.y;
    public Vector2 Position => gameObject.transform.position;
}
