using Assets.Scripts.Unit;
using System.Collections.Generic;
using UnityEngine;
using BoardGame.Unit;

public class EnemyGenerator : MonoBehaviour
{
    public GameObject CreateHollowSoldier(Transform parent)
    {
        var soldier = Instantiate(Resources.Load("Asset/SmallHollowSoldier"), parent) as GameObject;
        var prop = soldier.GetComponent<EnemyProperties>();

        prop.enemyType = EnemyClassEnum.ArbalestHollowSoldier;
        prop.hitPoints = 1;

        prop.leftEquipement = Instantiate(Resources.Load("Asset/Cards/EquipementCard")) as GameObject;
        var equipProp = prop.leftEquipement.GetComponent<EquipementProperties>();
        equipProp.SetMaterial((Material)Resources.Load("Material/Cards/StartingEquipement/warrior_shield_material", typeof(Material)));
        equipProp.equipementName = "Hollow Shield";
        equipProp.flatArmorResistence = 1;
        equipProp.flatMagicArmorResistence = 1;

        prop.rightEquipement = Instantiate(Resources.Load("Asset/Cards/EquipementCard")) as GameObject;
        equipProp = prop.rightEquipement.GetComponent<EquipementProperties>();
        equipProp.SetMaterial((Material)Resources.Load("Material/Cards/StartingEquipement/warrior_armour_material", typeof(Material)));
        equipProp.equipementName = "Hollow Sword";
        equipProp.attackList = new List<AttackDetail>()
        {
            new AttackDetail()
            {
                flatModifier = 4,
                dodgeLevel = 1,
            }
        };

        return soldier;
    }

    public GameObject CreateCrossbowHollowSoldier(Transform parent)
    {
        var soldier = Instantiate(Resources.Load("Asset/ArcherHoolowSoldier"), parent) as GameObject;
        var prop = soldier.GetComponent<EnemyProperties>();
        prop.enemyType = EnemyClassEnum.HollowSoldier;

        prop.hitPoints = 1;

        prop.leftEquipement = Instantiate(Resources.Load("Asset/Cards/EquipementCard")) as GameObject;
        var equipProp = prop.leftEquipement.GetComponent<EquipementProperties>();
        equipProp.SetMaterial((Material)Resources.Load("Material/Cards/StartingEquipement/warrior_shield_material", typeof(Material)));
        equipProp.equipementName = "Hollow Shield";
        equipProp.flatArmorResistence = 1;

        prop.rightEquipement = Instantiate(Resources.Load("Asset/Cards/EquipementCard")) as GameObject;
        equipProp = prop.rightEquipement.GetComponent<EquipementProperties>();
        equipProp.SetMaterial((Material)Resources.Load("Material/Cards/StartingEquipement/warrior_armour_material", typeof(Material)));
        equipProp.equipementName = "Hollow Crossbow";
        equipProp.attackList = new List<AttackDetail>()
        {
            new AttackDetail()
            {
                flatMagicModifier = 3,
                dodgeLevel = 1,
                range = 10,
            }
        };

        return soldier;
    }
}
