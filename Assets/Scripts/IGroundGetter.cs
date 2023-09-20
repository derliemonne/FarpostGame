using System.Collections.Generic;

/// <summary>
/// Gets info of the ground that is standable by entity
/// </summary>
public interface IGroundGetter
{
    public List<GroundInfo> GetGround();
}
