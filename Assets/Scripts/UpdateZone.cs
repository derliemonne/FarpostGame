using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateZone : MonoBehaviour
{
    [SerializeField]
    private Transform top_edge;
    [SerializeField]
    private Transform bottom_edge;
    [SerializeField]
    private float top_shift;
    [SerializeField]
    private float bottom_shift;

    public float GetTop()
    {
        return top_edge.position.y + top_shift;
    }
    public float GetBottom()
    {
        return bottom_edge.position.y + bottom_shift;
    }
}
