﻿using BoardGame.Unit;
using System;

public class PlayerProperties : UnitBasicProperties
{
    public PlayerClassEnum playerType;
    public bool hasEstus = true;
    public bool hasLuckToken = true;
    public bool hasAbility = true;

    protected override void StartInternal()
    {

    }

    protected override void UpdateInternal()
    {

    }

    protected override void ActivateInternal()
    {
        hasAggroToken = true;
        stamina = Math.Max(0, stamina - 2);
    }
}
