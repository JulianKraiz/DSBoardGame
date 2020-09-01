using System;
using UnityEngine;

public class UnitBasicProperties : MonoBehaviour
{
    public bool isActive;
    public int initiative;
    public int hitPoints;
    public int injuries;
    public int stamina;
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

    // Start is called before the first frame update
    void Start()
    {
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
}