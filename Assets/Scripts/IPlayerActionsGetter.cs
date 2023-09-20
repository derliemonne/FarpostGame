using System;

/// <summary>
/// Make interface for classes, that give user input, not set. Use on client side only to get
/// input data, that has been sent from server
/// </summary>
public interface IPlayerActionsGetter
{
    public int HorizontalMoveDirection { get; }

    public event Action PressJump;
    public event Action PressPushPlatform;
}
