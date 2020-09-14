﻿using BoardGame.Script.Events;
using System;
using UnityEngine;

public class UnitBasicProperties : MonoBehaviour
{
    public bool isActive { get; private set; }
    public int initiative;
    public int hitPoints;
    public int injuries;
    public int stamina { get; protected set; }
    public int physicalArmor;
    public int magicalArmor;
    public int physicalDamage;
    public int magicalDamage;

    public bool hasActivationToken;
    public bool hasAggroToken;

    public GameObject leftEquipement;
    public GameObject rightEquipement;
    public GameObject armourEquipement;
    public GameObject sideEquipement;

    private HoverBrillanceBehavior hoverBrillanceBehavior;

    // Start is called before the first frame update
    void Start()
    {
        var brillanceCapsule = transform.Find("HoverBrillance");
        if(brillanceCapsule != null)
        {
            hoverBrillanceBehavior = brillanceCapsule.GetComponent<HoverBrillanceBehavior>();
            hoverBrillanceBehavior.PositionSelected += BrillanceCapsuleClicked;
        }
        StartInternal();
    }

    protected virtual void StartInternal()
    {

    }

    // Update is called once per frame
    void Update()
    {
        UpdateInternal();
    }

    protected virtual void UpdateInternal()
    {

    }

    public void Activate()
    {
        isActive = true;
        hasActivationToken = false;
        ActivateInternal();
        EventManager.RaiseEventGameObject(EventTypes.UnitIsActivated, gameObject);
    }

    public void Deactivate()
    {
        isActive = false;
    }

    protected virtual void ActivateInternal()
    {

    }

    public void ResetStaminaAndInjuries()
    {
        stamina = 0;
        injuries = 0;
    }

    public virtual int HasEnoughStaminaToMove()
    {
        return hitPoints - injuries - stamina;
    }

    public virtual int StaminaLeft()
    {
        return hitPoints - injuries - stamina;
    }

    public virtual void ClearEquipement()
    {
        if (leftEquipement != null)
        {
            Destroy(leftEquipement);
        }
        if (rightEquipement != null)
        {
            Destroy(rightEquipement);
        }
        if (armourEquipement != null)
        {
            Destroy(armourEquipement);
        }
        if (sideEquipement != null)
        {
            Destroy(sideEquipement);
        }
    }

    private void OnMouseEnter()
    {
        EventManager.RaiseEventGameObject(EventTypes.UnitHoverEntered, gameObject);
    }

    private void OnMouseExit()
    {
        EventManager.RaiseEventGameObject(EventTypes.UnitHoverExited, gameObject);
    }

    private void OnMouseDown()
    {
        if(isActive)
        {
            EventManager.RaiseEventGameObject(EventTypes.ActiveUnitSelected, gameObject);
        }
    }

    public virtual void ConsumeStamina(int amount)
    {
        stamina += amount;
    }

    public void ShowHoverBrillance()
    {
        if(hoverBrillanceBehavior != null)
        {
            hoverBrillanceBehavior.Activate();
        }
    }

    public void HideHoverBrillance()
    {
        if (hoverBrillanceBehavior != null)
        {
            hoverBrillanceBehavior.Deactivate();
        }
    }

    public void RecieveInjuries(int injuriesRecieved)
    {
        injuries += injuriesRecieved;
    }

    private void BrillanceCapsuleClicked(GameObject position)
    {
        EventManager.RaiseEventGameObject(EventTypes.UnitSelected, gameObject);
    }

    
}