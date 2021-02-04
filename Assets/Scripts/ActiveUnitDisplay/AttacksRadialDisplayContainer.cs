using Assets.Scripts.Unit;
using Assets.Scripts.Unit.Model.Attacks;
using BoardGame.Script.Events;
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

    void Start()
    {
        positionOffset = new Vector3(0, 1, 0);
        displayScale = new Vector3(0.2f, 0.2f, 0.2f);
        EventManager.StartListeningGameObject(EventTypes.UnitIsActivated, SetupUnitAttackPanels);
        EventManager.StartListeningObject(EventTypes.AttackApplied, DisableSide);
        EventManager.StartListeningObject(EventTypes.ResetAndHideAttackDial, ClearAllFromEvent);

        LeftAttack1 = transform.Find("LeftAttack1").GetComponent<AttackRadialDisplayBehavior>(); 
        LeftAttack2 = transform.Find("LeftAttack2").GetComponent<AttackRadialDisplayBehavior>(); 
        LeftAttack3 = transform.Find("LeftAttack3").GetComponent<AttackRadialDisplayBehavior>(); 
        RightAttack1 = transform.Find("RightAttack1").GetComponent<AttackRadialDisplayBehavior>();
        RightAttack2 = transform.Find("RightAttack2").GetComponent<AttackRadialDisplayBehavior>();
        RightAttack3 = transform.Find("RightAttack3").GetComponent<AttackRadialDisplayBehavior>();

        ClearAllDisplay();
    }

    void Update()
    {
    }

    private void ClearAllFromEvent(object _)
    {
        currentUnit = null;
        ClearAllDisplay();
    }

    private void SetupUnitAttackPanels(GameObject unit)
    {
        ClearAllDisplay();

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

    private void SetLeftAttack(AttackDetail attack, UnitBasicProperties unitProperties, int index)
    {
        if (index == 0)
        {
            LeftAttack1.gameObject.SetActive(true);
            LeftAttack1.Reset();
            LeftAttack1.SetupTile(attack, unitProperties, AttackSide.Left);
        }
        else if (index == 1)
        {
            LeftAttack2.gameObject.SetActive(true);
            LeftAttack2.Reset();
            LeftAttack2.SetupTile(attack, unitProperties, AttackSide.Left);
        }
        else if (index == 2)
        {
            LeftAttack3.gameObject.SetActive(true);
            LeftAttack3.Reset();
            LeftAttack3.SetupTile(attack, unitProperties, AttackSide.Left);
        }
    }

    private void SetRightAttack(AttackDetail attack, UnitBasicProperties unitProperties, int index)
    {
        if (index == 0)
        {
            RightAttack1.gameObject.SetActive(true);
            RightAttack1.Reset();
            RightAttack1.SetupTile(attack, unitProperties, AttackSide.Right);
        }
        else if (index == 1)
        {
            RightAttack2.gameObject.SetActive(true);
            RightAttack2.Reset();
            RightAttack2.SetupTile(attack, unitProperties, AttackSide.Right);
        }
        else if (index == 2)
        {
            RightAttack3.gameObject.SetActive(true);
            RightAttack3.Reset();
            RightAttack3.SetupTile(attack, unitProperties, AttackSide.Right);
        }
    }

    private void ClearAllDisplay()
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
        var detail = (AttackDetail)attack;
        if(detail.side == AttackSide.Left || detail.side == AttackSide.Both)
        {
            DisableLeftDisplay();
        }
        else if (detail.side == AttackSide.Right || detail.side == AttackSide.Both)
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
