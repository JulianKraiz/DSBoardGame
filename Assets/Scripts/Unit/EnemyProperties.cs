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

    public override void ConsumeStamina(int amount)
    {
    }

    public override int HasEnoughStaminaToMove()
    {
        return 10;
    }
}
