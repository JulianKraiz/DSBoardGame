using BoardGame.Unit;
using UnityEngine;

public class EnemyProperties : UnitBasicProperties
{
    public EnemyClassEnum enemyType;
    protected override void StartInternal()
    {

    }

    protected override void UpdateInternal()
    {

    }

    public override int StaminaLeft()
    {
        return 10;
    }
}
