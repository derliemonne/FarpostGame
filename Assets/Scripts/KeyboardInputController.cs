using UnityEngine;
using Fusion;

public class KeyboardInputController : NetworkBehaviour, IPlayerActionsSetter
{
    public int HorizontalMoveDirection => (int)Input.GetAxis("Horizontal");
    public bool IsPressingJump => Input.GetAxis("Vertical") > 0.1;
    public bool IsPressingPushPlatform => Input.GetAxis("PushPlatform") > 0.1;
}
