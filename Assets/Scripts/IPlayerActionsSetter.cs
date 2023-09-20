/// <summary>
/// Make interface for classes, that set input data for sent it to the server, not give access for it.
/// Use only to get input data from user and then sent it to the server.
/// </summary>
public interface IPlayerActionsSetter
{
    public int HorizontalMoveDirection { get; }
    public bool IsPressingJump { get; }
    public bool IsPressingPushPlatform { get; }
}
