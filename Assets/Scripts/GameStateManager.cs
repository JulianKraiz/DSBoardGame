using BoardGame.Script.Events;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BoardGame.Unit;
using Assets.Scripts.Unit;

public class GameStateManager : MonoBehaviour
{
    public List<GameObject> tiles = new List<GameObject>();
    public List<GameObject> players = new List<GameObject>();

    private PlayerDisplayContainer characterDisplayContainer;

    void Start()
    {
        characterDisplayContainer = GameObject.Find("UnitDisplays").GetComponent<PlayerDisplayContainer>();
        tiles = GameObject.FindGameObjectsWithTag("Tile").ToList();
        players = GameObject.FindGameObjectsWithTag("Player").ToList();
        EventManager.StartListening(EventTypes.TileIsEntered, IntializeFocusedTileHandler);

        // DEBUG
        foreach (var tile in tiles)
        {
            tile.GetComponent<TileManager>().monsterSettings = new TileMonsterSettings()
            {
                arbalestHollowSoldierCount = 1,
                swordhollowSoldierCount = 2,
            };
        }
        SetPlayerStartingEquipement();

        // do after each player added to the game.
        InitializeCharacterDisplays();

     
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void IntializeFocusedTileHandler(GameObject _)
    {
        var focusedTile = tiles.Select(t => t.GetComponent<TileManager>()).Where(p => p.isFocused).FirstOrDefault();
        if (focusedTile != null)
        {
            focusedTile.PrepareTileEntered();
        }
    }

    private void InitializeCharacterDisplays()
    {
        characterDisplayContainer.Initialize();
    }

    private void SetPlayerStartingEquipement()
    {
        var pProps = players.Select(p => p.GetComponent<PlayerProperties>());
        
        var prop = pProps.FirstOrDefault(p => p.playerType == PlayerClassEnum.Warrior);
        if (prop != null)
        {
            prop.leftEquipement = Instantiate(Resources.Load("Asset/Cards/EquipementCard")) as GameObject;
            prop.leftEquipement.transform.Rotate(45, 180, 0);
            var equipProp = prop.leftEquipement.GetComponent<EquipementProperties>();
            equipProp.SetMaterial((Material)Resources.Load("Material/Cards/StartingEquipement/warrior_shield_material", typeof(Material)));
            equipProp.equipementName = "Round Shield";
            equipProp.blackArmorDices = 1;

            prop.armourEquipement = Instantiate(Resources.Load("Asset/Cards/EquipementCard")) as GameObject;
            prop.armourEquipement.transform.Rotate(45, 180, 0);
            equipProp = prop.armourEquipement.GetComponent<EquipementProperties>();
            equipProp.SetMaterial((Material)Resources.Load("Material/Cards/StartingEquipement/warrior_armour_material", typeof(Material)));
            equipProp.equipementName = "Northern Armour";
            equipProp.strenghRequirement = 16;
            equipProp.dextirityRequirement = 8;
            equipProp.blackArmorDices = 1;
            equipProp.blackMagicArmorDices = 1;
            equipProp.dodgeRollsDices = 1;

            prop.rightEquipement = Instantiate(Resources.Load("Asset/Cards/EquipementCard")) as GameObject;
            prop.rightEquipement.transform.Rotate(45, 180, 0);
            equipProp = prop.rightEquipement.GetComponent<EquipementProperties>();
            equipProp.SetMaterial((Material)Resources.Load("Material/Cards/StartingEquipement/warrior_axe_material", typeof(Material)));
            equipProp.equipementName = "Battle Axe";
            equipProp.strenghRequirement = 14;
            equipProp.attackList = new List<AttackDetail>()
            {
                new AttackDetail()
                {
                    blackAttackDices = 2,
                    staminaCost = 0,
                },
                new AttackDetail()
                {
                    blackAttackDices = 2,
                    staminaCost = 2,
                    nodeSplash = true,
                }
            };
        }

        prop = pProps.FirstOrDefault(p => p.playerType == PlayerClassEnum.Herald);
        if (prop != null)
        {
            prop.leftEquipement = Instantiate(Resources.Load("Asset/Cards/EquipementCard")) as GameObject;
            prop.leftEquipement.transform.Rotate(45, 180, 0);
            var equipProp = prop.leftEquipement.GetComponent<EquipementProperties>();
            equipProp.SetMaterial((Material)Resources.Load("Material/Cards/StartingEquipement/herald_shield_material", typeof(Material)));
            equipProp.equipementName = "Kite Shield";
            equipProp.blackArmorDices = 1;

            prop.armourEquipement = Instantiate(Resources.Load("Asset/Cards/EquipementCard")) as GameObject;
            prop.armourEquipement.transform.Rotate(45, 180, 0);
            equipProp = prop.armourEquipement.GetComponent<EquipementProperties>();
            equipProp.SetMaterial((Material)Resources.Load("Material/Cards/StartingEquipement/herald_armour_material", typeof(Material)));
            equipProp.equipementName = "Herald Armour";
            equipProp.strenghRequirement = 10;
            equipProp.faithRequirement = 10;
            equipProp.blackArmorDices = 1;
            equipProp.blackMagicArmorDices = 1;
            equipProp.dodgeRollsDices = 1;

            prop.rightEquipement = Instantiate(Resources.Load("Asset/Cards/EquipementCard")) as GameObject;
            prop.rightEquipement.transform.Rotate(45, 180, 0);
            equipProp = prop.rightEquipement.GetComponent<EquipementProperties>();
            equipProp.SetMaterial((Material)Resources.Load("Material/Cards/StartingEquipement/herald_spear_material", typeof(Material)));
            equipProp.equipementName = "Spear Axe";
            equipProp.attackList = new List<AttackDetail>()
            {
                new AttackDetail()
                {
                    blackAttackDices = 1,
                    staminaCost = 0,
                    minimumRange = 1,
                    range = 1
                },
                new AttackDetail()
                {
                    blackAttackDices = 1,
                    flatModifier = 1,
                    staminaCost = 3,
                    minimumRange = 1,
                    range = 1
                }
        };

            prop.sideEquipement = Instantiate(Resources.Load("Asset/Cards/EquipementCard")) as GameObject;
            prop.sideEquipement.transform.Rotate(45, 180, 0);
            equipProp = prop.sideEquipement.GetComponent<EquipementProperties>();
            equipProp.SetMaterial((Material)Resources.Load("Material/Cards/StartingEquipement/herald_talisman_material", typeof(Material)));
            equipProp.equipementName = "Talisman";
            equipProp.strenghRequirement = 12;
            equipProp.faithRequirement = 12;
            equipProp.attackList = new List<AttackDetail>()
            {
                new AttackDetail()
                {
                    staminaCost = 0,
                    range = 2,
                    flatModifier = -2,
                },
                new AttackDetail()
                {
                    staminaCost = 0,
                    range = 2,
                    flatModifier = -6
                }
        };
        }

        prop = pProps.FirstOrDefault(p => p.playerType == PlayerClassEnum.Knight);
        if (prop != null)
        {
            prop.leftEquipement = Instantiate(Resources.Load("Asset/Cards/EquipementCard")) as GameObject;
            var equipProp = prop.leftEquipement.GetComponent<EquipementProperties>();
            equipProp.SetMaterial((Material)Resources.Load("Material/Cards/StartingEquipement/knight_shield_material", typeof(Material)));
            equipProp.equipementName = "Kite Shield";
            equipProp.blackArmorDices = 1;

            prop.armourEquipement = Instantiate(Resources.Load("Asset/Cards/EquipementCard")) as GameObject;
            equipProp = prop.armourEquipement.GetComponent<EquipementProperties>();
            equipProp.SetMaterial((Material)Resources.Load("Material/Cards/StartingEquipement/knight_armour_material", typeof(Material)));
            equipProp.equipementName = "Knight Armour";
            equipProp.strenghRequirement = 12;
            equipProp.blueArmorDices = 1;
            equipProp.blackMagicArmorDices = 1;

            prop.rightEquipement = Instantiate(Resources.Load("Asset/Cards/EquipementCard")) as GameObject;
            equipProp = prop.rightEquipement.GetComponent<EquipementProperties>();
            equipProp.SetMaterial((Material)Resources.Load("Material/Cards/StartingEquipement/knight_sword_material", typeof(Material)));
            equipProp.equipementName = "Long Sword";
            equipProp.strenghRequirement = 13;
            equipProp.dextirityRequirement = 12;
            equipProp.attackList = new List<AttackDetail>()
            {
                new AttackDetail()
                {
                    blueAttackDices = 1,
                    staminaCost = 0,
                },
                new AttackDetail()
                {
                    blackAttackDices = 1,
                    blueAttackDices = 1,
                    staminaCost = 4,
                }
        };
        }

        prop = pProps.FirstOrDefault(p => p.playerType == PlayerClassEnum.Assassin);
        if (prop != null)
        {
            prop.leftEquipement = Instantiate(Resources.Load("Asset/Cards/EquipementCard")) as GameObject;
            var equipProp = prop.leftEquipement.GetComponent<EquipementProperties>();
            equipProp.SetMaterial((Material)Resources.Load("Material/Cards/StartingEquipement/assassin_shield_material", typeof(Material)));
            equipProp.equipementName = "Target Shield";
            equipProp.dodgeRollsDices = 1;

            prop.armourEquipement = Instantiate(Resources.Load("Asset/Cards/EquipementCard")) as GameObject;
            equipProp = prop.armourEquipement.GetComponent<EquipementProperties>();
            equipProp.SetMaterial((Material)Resources.Load("Material/Cards/StartingEquipement/assassin_armour_material", typeof(Material)));
            equipProp.equipementName = "Assassin Armour";
            equipProp.strenghRequirement = 10;
            equipProp.dextirityRequirement = 14;
            equipProp.blackArmorDices = 1;
            equipProp.blackMagicArmorDices = 1;
            equipProp.dodgeRollsDices = 1;

            prop.rightEquipement = Instantiate(Resources.Load("Asset/Cards/EquipementCard")) as GameObject;
            equipProp = prop.rightEquipement.GetComponent<EquipementProperties>();
            equipProp.SetMaterial((Material)Resources.Load("Material/Cards/StartingEquipement/assassin_sword_material", typeof(Material)));
            equipProp.equipementName = "Estoc";
            equipProp.dextirityRequirement = 14;
            equipProp.attackList = new List<AttackDetail>()
            {
                new AttackDetail()
                {
                    blueAttackDices = 2,
                    flatModifier = -1,
                    staminaCost = 0,
                },
                new AttackDetail()
                {
                    blackAttackDices = 3,
                    flatModifier = -1,
                    staminaCost = 3,
            }
        };
        }
    }
}
