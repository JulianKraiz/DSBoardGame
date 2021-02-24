using Assets.Scripts.Unit;
using System.Collections.Generic;
using UnityEngine;
using BoardGame.Unit;
using Assets.Scripts.Unit.Model.Attacks;

namespace Assets.Scripts.Tile
{
    public class EnemyGenerator : MonoBehaviour
    {

        public GameObject CreateHollowSoldier(Transform parent)
        {
            var soldier = Instantiate(Resources.Load("Asset/Units/SmallHollowSoldier"), parent) as GameObject;
            var prop = soldier.GetComponent<EnemyProperties>();

            prop.portrait = (Material)Resources.Load("Material/UnitBackground/hollow_soldier_portrait", typeof(Material));
            prop.tile = (Material)Resources.Load("Material/UnitBackground/hollow_soldier_tile_material", typeof(Material));
            prop.enemyType = EnemyClassEnum.ArbalestHollowSoldier;
            prop.hitPoints = 1;
            prop.side = UnitSide.Hollow;

            prop.leftEquipement = Instantiate(Resources.Load("Asset/Cards/EquipementCard"), GameStateManager.Instance.generatedCardHolder) as GameObject;
            var equipProp = prop.leftEquipement.GetComponent<EquipementProperties>();
            equipProp.SetMaterial((Material)Resources.Load("Material/Cards/StartingEquipement/warrior_shield_material", typeof(Material)));
            equipProp.equipementName = "Hollow Shield";
            equipProp.flatArmorResistence = 1;
            equipProp.flatMagicArmorResistence = 1;

            prop.rightEquipement = Instantiate(Resources.Load("Asset/Cards/EquipementCard"), GameStateManager.Instance.generatedCardHolder) as GameObject;
            equipProp = prop.rightEquipement.GetComponent<EquipementProperties>();
            equipProp.SetMaterial((Material)Resources.Load("Material/Cards/StartingEquipement/warrior_armour_material", typeof(Material)));
            equipProp.equipementName = "Hollow Sword";
            equipProp.attackList = new List<BehaviorAction>()
        {
                 new MovementAction()
            {
                Direction = MovementDirection.Forward,
                MoveDistance = 1,
                TargetPreference = PreferedTarget.Closest,
            },
            new AttackAction()
            {
                FlatModifier = 4,
                DodgeLevel = 1,
                TargetPreference = PreferedTarget.Closest,
            }
        };

            return soldier;
        }

        public GameObject CreateCrossbowHollowSoldier(Transform parent)
        {
            var soldier = Instantiate(Resources.Load("Asset/Units/ArcherHoolowSoldier"), parent) as GameObject;
            var prop = soldier.GetComponent<EnemyProperties>();

            prop.portrait = (Material)Resources.Load("Material/UnitBackground/hollow_arbalest_soldier_portrait", typeof(Material));
            prop.tile = (Material)Resources.Load("Material/UnitBackground/hollow_arbalest_soldier_tile_material", typeof(Material));
            prop.enemyType = EnemyClassEnum.HollowSoldier;
            prop.side = UnitSide.Hollow;
            prop.hitPoints = 1;

            prop.leftEquipement = Instantiate(Resources.Load("Asset/Cards/EquipementCard"), GameStateManager.Instance.generatedCardHolder) as GameObject;
            var equipProp = prop.leftEquipement.GetComponent<EquipementProperties>();
            equipProp.SetMaterial((Material)Resources.Load("Material/Cards/StartingEquipement/warrior_shield_material", typeof(Material)));
            equipProp.equipementName = "Hollow Shield";
            equipProp.flatArmorResistence = 1;

            prop.rightEquipement = Instantiate(Resources.Load("Asset/Cards/EquipementCard"), GameStateManager.Instance.generatedCardHolder) as GameObject;
            equipProp = prop.rightEquipement.GetComponent<EquipementProperties>();
            equipProp.SetMaterial((Material)Resources.Load("Material/Cards/StartingEquipement/warrior_armour_material", typeof(Material)));
            equipProp.equipementName = "Hollow Crossbow";
            equipProp.attackList = new List<BehaviorAction>()
        {
            new MovementAction()
            {
                Direction = MovementDirection.Backward,
                MoveDistance = 1,
                TargetPreference = PreferedTarget.Aggro,
            },
            new AttackAction()
            {
                FlatModifier = 3,
                MagicAttack = true,
                InfiniteRange = true,
                DodgeLevel = 1,
                TargetPreference = PreferedTarget.Aggro,
            }
        };

            return soldier;
        }
    }
}