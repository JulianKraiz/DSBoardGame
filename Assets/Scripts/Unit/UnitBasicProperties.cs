using Assets.Scripts.Unit;
using BoardGame.Script.Events;
using System;
using UnityEngine;

public class UnitBasicProperties : MonoBehaviour
{
    public Material portrait;
    public Material tile;

    public bool isActive { get; protected set; }
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

    public bool hasEmber = false;
    public bool isBleeding;
    public bool isPoisoned;
    public bool isStaggered;
    public bool isFrozen;

    public GameObject leftEquipement;
    public GameObject rightEquipement;
    public GameObject armourEquipement;
    public GameObject sideEquipement;

    public UnitSide side;

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
        ActivateInternal();
        EventManager.RaiseEvent(GameObjectEventType.UnitIsActivated, gameObject);
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
    }

    private void OnMouseExit()
    {
    }

    private void OnMouseDown()
    {
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
        if (injuriesRecieved >= 3 && hasEmber)
        {
            injuriesRecieved -= 1;
        }

        injuries += injuriesRecieved;

        if (injuriesRecieved > 0 && isBleeding)
        {
            injuries += 2;
            isBleeding = false;
        }
    }

    private void BrillanceCapsuleClicked(GameObject position)
    {
        EventManager.RaiseEvent(GameObjectEventType.UnitSelected, gameObject);
    }

    public DefenseDices GetDefenseDices(bool magicAttack)
    {
        var result = new DefenseDices();
        if (armourEquipement != null)
        {
            armourEquipement.GetComponent<EquipementProperties>().ContributeDiceToDefenseRolls(result, magicAttack);
        }
        if (leftEquipement != null)
        {
            leftEquipement.GetComponent<EquipementProperties>().ContributeDiceToDefenseRolls(result, magicAttack);
        }
        if(rightEquipement != null)
        {
            rightEquipement.GetComponent<EquipementProperties>().ContributeDiceToDefenseRolls(result, magicAttack);
        }
        
        return result;
    }

    public void EndOfTurn()
    {
        Deactivate();
        ApplyStatusEffect();
        RemoveStatusEffect();

    }

    private void ApplyStatusEffect()
    {
        if(isPoisoned)
        {
            RecieveInjuries(1);
        }
    }

    internal void RemoveStatusEffect(bool endOfActivationCleanse = true)
    {
        isPoisoned = false;
        isStaggered = false;
        isFrozen = false;
        isBleeding = endOfActivationCleanse ? isBleeding : false;
    }
}