﻿using Assets.Scripts.Unit;
using Assets.Scripts.Unit.Model.Attacks;
using BoardGame.Script.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyAutoTurnResolver : MonoBehaviour
{

    private EnemyAttackResolver attackResolver;

    private EnemyProperties enemy;
    private List<AttackDetail> attackstoExecute;

    void Start()
    {
        attackstoExecute = new List<AttackDetail>();
        attackResolver = FindObjectOfType<EnemyAttackResolver>();
    }

    void Update()
    {
        
    }

    public void ResolveEnemyTurn(GameObject enemyObject)
    {
        enemy = enemyObject.GetComponent<EnemyProperties>();

        if(enemy.leftEquipement != null)
        {
            var t = enemy.leftEquipement.GetComponent<EquipementProperties>();
            attackstoExecute.AddRange(enemy.leftEquipement.GetComponent<EquipementProperties>().attackList);
        }
        if (enemy.rightEquipement != null)
        {
            attackstoExecute.AddRange(enemy.rightEquipement.GetComponent<EquipementProperties>().attackList);
        }
        if (enemy.armourEquipement != null)
        {
            attackstoExecute.AddRange(enemy.armourEquipement.GetComponent<EquipementProperties>().attackList);
        }

        EventManager.StartListening(ObjectEventType.AttackApplied, AttackApplied);

        LaunchAttack();
    }

    private void LaunchAttack()
    {
        if(!attackstoExecute.Any())
        {
            EventManager.RaiseEvent(GameObjectEventType.EndUnitTurn);
            return;
        }

        var nextAttack = attackstoExecute.First();
        attackResolver.ExecuteAttack(enemy, nextAttack);
    }

    // After is applied and units checked out. ready for next round.
    private void AttackApplied(object _)
    {
        attackstoExecute.RemoveAt(0);
        LaunchAttack();
    }
}
