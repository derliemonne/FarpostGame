using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayersList : MonoBehaviour
{
    public FoxScript Fox { get; private set; }
    public CatScript Cat { get; private set; }
    public RaccoonScript Raccoon { get; private set; }
    public GullScript Gull { get; private set; }

    public void Init(FoxScript foxScript, CatScript catScript, RaccoonScript raccoonScript, GullScript gullScript)
    {
        Fox = foxScript;
        Cat = catScript;
        Raccoon = raccoonScript; 
        Gull = gullScript;
    }

    public float GetHighest()
    {
        float foxHeight = 0, catHeight = 0, raccoonHeight = 0, gullHeight = 0;
        if (Fox) foxHeight = Fox.PlayerGameObject.transform.position.y;
        if (Cat) catHeight = Cat.PlayerGameObject.transform.position.y;
        if (Raccoon) raccoonHeight = Raccoon.PlayerGameObject.transform.position.y;
        if (Gull) gullHeight = Gull.PlayerGameObject.transform.position.y;
        return Mathf.Max(foxHeight, catHeight, raccoonHeight, gullHeight);
    }
}
