using BoardGame.Unit;
using UnityEngine;

public class PlayerProperties : UnitBasicProperties
{
    public PlayerClassEnum playerType;
    public bool hasEstus = true;
    public bool hasLuckToken = true;
    public bool hasEmber = false;
    public bool hasAbility= true;



    protected override void StartInternal()
    {

    }

    protected override void UpdateInternal()
    {

    }
}
