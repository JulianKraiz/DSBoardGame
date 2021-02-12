﻿using Assets.Scripts.Unit;
using Assets.Scripts.Unit.Model.Attacks;
using BoardGame.Script.Events;
using System.Collections.Generic;
using UnityEngine;

public class AttacksRadialDisplayContainer : MonoBehaviour
{
    private Vector3 positionOffset;
    private Vector3 displayScale;

    private GameObject currentUnit;

    private AttackRadialDisplayBehavior RightAttack1;
    private AttackRadialDisplayBehavior RightAttack2;
    private AttackRadialDisplayBehavior RightAttack3;
    private AttackRadialDisplayBehavior LeftAttack1;
    private AttackRadialDisplayBehavior LeftAttack2;
    private AttackRadialDisplayBehavior LeftAttack3;

    private List<AttackRadialDisplayBehavior> existing;

    void Start()
    {
        positionOffset = new Vector3(0, 1, 0);
        displayScale = new Vector3(0.2f, 0.2f, 0.2f);
        EventManager.StartListening(GameObjectEventType.UnitIsActivated, SetupUnitAttackPanels);
        EventManager.StartListening(ObjectEventType.AttackApplied, DisableSide);
        EventManager.StartListening(GameObjectEventType.ResetAndHideAttackDial, ClearAllFromEvent);
        EventManager.StartListening(ObjectEventType.EncounterToResolve, HideTemporaily);
        EventManager.StartListening(ObjectEventType.EncountersResolved, ShowExisting);

        LeftAttack1 = transform.Find("LeftAttack1").GetComponent<AttackRadialDisplayBehavior>(); 
        LeftAttack2 = transform.Find("LeftAttack2").GetComponent<AttackRadialDisplayBehavior>(); 
        LeftAttack3 = transform.Find("LeftAttack3").GetComponent<AttackRadialDisplayBehavior>(); 
        RightAttack1 = transform.Find("RightAttack1").GetComponent<AttackRadialDisplayBehavior>();
        RightAttack2 = transform.Find("RightAttack2").GetComponent<AttackRadialDisplayBehavior>();
        RightAttack3 = transform.Find("RightAttack3").GetComponent<AttackRadialDisplayBehavior>();

        existing = new List<AttackRadialDisplayBehavior>();

        HideAllDisplay();
    }

    private void ShowExisting(object arg0)
    {
       foreach(var attack in existing)
        {
            attack.gameObject.SetActive(true);
        }
    }

    private void HideTemporaily(object _)
    {
        HideAllDisplay();
    }

    void Update()
    {
    }

    private void ClearAllFromEvent(object _)
    {
        currentUnit = null;
        HideAllDisplay();
    }

    private void SetupUnitAttackPanels(GameObject unit)
    {
        HideAllDisplay();

        existing.Clear();
        currentUnit = unit;

        var basicProperties = currentUnit.GetComponent<UnitBasicProperties>();
        if (basicProperties.leftEquipement != null)
        {
            var index = 0;
            var equipementProperties = basicProperties.leftEquipement.GetComponent<EquipementProperties>();
            foreach (var attack in equipementProperties.attackList)
            {
                SetLeftAttack(attack, basicProperties, index);
                index++;
            }

            index = 0;
            equipementProperties = basicProperties.rightEquipement.GetComponent<EquipementProperties>();
            foreach (var attack in equipementProperties.attackList)
            {
                SetRightAttack(attack, basicProperties, index);
                index++;
            }
        }
    }

    private void SetLeftAttack(AttackAction attack, UnitBasicProperties unitProperties, int index)
    {
        if (index == 0)
        {
            LeftAttack1.gameObject.SetActive(true);
            LeftAttack1.Reset();
            LeftAttack1.SetupTile(attack, unitProperties, AttackSide.Left);
            existing.Add(LeftAttack1);
        }
        else if (index == 1)
        {
            LeftAttack2.gameObject.SetActive(true);
            LeftAttack2.Reset();
            LeftAttack2.SetupTile(attack, unitProperties, AttackSide.Left);
            existing.Add(LeftAttack2);
        }
        else if (index == 2)
        {
            LeftAttack3.gameObject.SetActive(true);
            LeftAttack3.Reset();
            LeftAttack3.SetupTile(attack, unitProperties, AttackSide.Left);
            existing.Add(LeftAttack3);
        }
    }

    private void SetRightAttack(AttackAction attack, UnitBasicProperties unitProperties, int index)
    {
        if (index == 0)
        {
            RightAttack1.gameObject.SetActive(true);
            RightAttack1.Reset();
            RightAttack1.SetupTile(attack, unitProperties, AttackSide.Right);
            existing.Add(RightAttack1);
        }
        else if (index == 1)
        {
            RightAttack2.gameObject.SetActive(true);
            RightAttack2.Reset();
            RightAttack2.SetupTile(attack, unitProperties, AttackSide.Right);
            existing.Add(RightAttack2);
        }
        else if (index == 2)
        {
            RightAttack3.gameObject.SetActive(true);
            RightAttack3.Reset();
            RightAttack3.SetupTile(attack, unitProperties, AttackSide.Right);
            existing.Add(RightAttack3);
        }
    }

    private void HideAllDisplay()
    {
        LeftAttack1.gameObject.SetActive(false);
        LeftAttack2.gameObject.SetActive(false);
        LeftAttack3.gameObject.SetActive(false);
        RightAttack1.gameObject.SetActive(false);
        RightAttack2.gameObject.SetActive(false);
        RightAttack3.gameObject.SetActive(false);
    }

    private void DisableSide(object attack)
    {
        var detail = (AttackAction)attack;
        if(detail.Side == AttackSide.Left || detail.Side == AttackSide.Both)
        {
            DisableLeftDisplay();
        }
        else if (detail.Side == AttackSide.Right || detail.Side == AttackSide.Both)
        {
            DisableRightDisplay();
        }
    }

    private void DisableLeftDisplay()
    {
        LeftAttack1.Disable();
        LeftAttack2.Disable();
        LeftAttack3.Disable();
    }

    private void DisableRightDisplay()
    {
        RightAttack1.Disable();
        RightAttack2.Disable();
        RightAttack3.Disable();
    }

}
