using UnityEngine;

/// <summary>
/// Controll the movement of entity, giving this direction and speed
/// </summary>
public interface IMoveController
{
    /// <summary>
    /// Must be one-vector, only assign direction
    /// </summary>
    Vector2 Direction { get; set; }
    float Speed { get; set; }
}
