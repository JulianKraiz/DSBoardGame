﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BoardGame.Unit;
using Assets.Scripts.Unit;
using Assets.Scripts.Unit.Model.Attacks;
using Assets.Scripts.Tile;
using BoardGame.Script.Events;
using System;

public class GameStateManager : MonoBehaviour
{
    public List<GameObject> tiles = new List<GameObject>();
    public List<GameObject> players = new List<GameObject>();

    public PlayerDisplayContainer characterDisplayContainer;
    public BonefireTileManager bonefireTile;
    public int bonefireSparks;
    public int soulCache;

    public Transform generatedCardHolder;
    public static GameStateManager Instance;

    private bool FirstFrameInitialisation;

    void Start()
    {
        FirstFrameInitialisation = false;
        Instance = this;
        tiles = GameObject.FindGameObjectsWithTag("Tile").ToList();
        players = GameObject.FindGameObjectsWithTag("Player").Where(p => p.activeSelf).ToList();

        bonefireSparks = 6 - players.Count;

        // TODO : Set from random encounter card attach to tile.
        foreach (var tile in tiles)
        {
            tile.GetComponent<TileManager>().monsterSettings = new TileMonsterSettings()
            {
                arbalestHollowSoldierCount = 1,
                //swordhollowSoldierCount = 2,
            };
        }

        // TODO : Do after selecting character from menu / previous scene.
        SetPlayerStartingEquipement();
        InitializeCharacterDisplays();


        EventManager.StartListening(GameObjectEventType.PlayerUnitKilled, PlayerDiedAtPosition);
        EventManager.StartListening(GameObjectEventType.RestPartyAtBonefire, RestPartyAtBonefire);
    }

    void Update()
    {
        if (!FirstFrameInitialisation)
        {
            FirstFrameInitialisation = true;
            EventManager.RaiseEvent(GameObjectEventType.TileFocused, bonefireTile.gameObject);
        }
    }

    private void InitializeCharacterDisplays()
    {
        characterDisplayContainer.Initialize();
    }

    public TileManager GetActiveTile()
    {
        var tileBehaviors = tiles.Select(t => t.GetComponent<TileManager>());
        return tileBehaviors.First(b => b.isFocused);
    }

    private void SetPlayerStartingEquipement()
    {
        var pProps = players.Select(p => p.GetComponent<PlayerProperties>());
        
        var prop = pProps.FirstOrDefault(p => p.playerType == PlayerClassEnum.Warrior);
        if (prop != null)
        {
            prop.initiative = 9;
            prop.leftEquipement = Instantiate(Resources.Load("Asset/Cards/EquipementCard"), generatedCardHolder) as GameObject;
            prop.leftEquipement.transform.Rotate(45, 180, 0);
            var equipProp = prop.leftEquipement.GetComponent<EquipementProperties>();
            equipProp.SetMaterial((Material)Resources.Load("Material/Cards/StartingEquipement/warrior_shield_material", typeof(Material)));
            equipProp.equipementName = "Round Shield";
            equipProp.name = "Round Shield";
            equipProp.blackArmorDices = 1;

            prop.armourEquipement = Instantiate(Resources.Load("Asset/Cards/EquipementCard"), generatedCardHolder) as GameObject;
            prop.armourEquipement.transform.Rotate(45, 180, 0);
            equipProp = prop.armourEquipement.GetComponent<EquipementProperties>();
            equipProp.SetMaterial((Material)Resources.Load("Material/Cards/StartingEquipement/warrior_armour_material", typeof(Material)));
            equipProp.equipementName = "Northern Armour";
            equipProp.name = "Northern Armour";
            equipProp.strenghRequirement = 16;
            equipProp.dextirityRequirement = 8;
            equipProp.blackArmorDices = 1;
            equipProp.blackMagicArmorDices = 1;
            equipProp.dodgeRollsDices = 1;

            prop.rightEquipement = Instantiate(Resources.Load("Asset/Cards/EquipementCard"), generatedCardHolder) as GameObject;
            prop.rightEquipement.transform.Rotate(45, 180, 0);
            equipProp = prop.rightEquipement.GetComponent<EquipementProperties>();
            equipProp.SetMaterial((Material)Resources.Load("Material/Cards/StartingEquipement/warrior_axe_material", typeof(Material)));
            equipProp.equipementName = "Battle Axe";
            equipProp.name = "Battle Axe";
            equipProp.strenghRequirement = 14;
            equipProp.attackList = new List<BehaviorAction>()
            {
                new AttackAction()
                {
                    BlackDices = 2,
                    StaminaCost = 0,
                },
                new AttackAction()
                {
                    BlackDices = 2,
                    StaminaCost = 2,
                    NodeSplash = true,
                }
            };
        }

        prop = pProps.FirstOrDefault(p => p.playerType == PlayerClassEnum.Herald);
        if (prop != null)
        {
            prop.injuries = 9;
            prop.initiative = 4;
            prop.leftEquipement = Instantiate(Resources.Load("Asset/Cards/EquipementCard"), generatedCardHolder) as GameObject;
            prop.leftEquipement.transform.Rotate(45, 180, 0);
            var equipProp = prop.leftEquipement.GetComponent<EquipementProperties>();
            equipProp.SetMaterial((Material)Resources.Load("Material/Cards/StartingEquipement/herald_shield_material", typeof(Material)));
            equipProp.equipementName = "Kite Shield";
            equipProp.name = "Kite Shield";
            equipProp.blackArmorDices = 1;

            prop.armourEquipement = Instantiate(Resources.Load("Asset/Cards/EquipementCard"), generatedCardHolder) as GameObject;
            prop.armourEquipement.transform.Rotate(45, 180, 0);
            equipProp = prop.armourEquipement.GetComponent<EquipementProperties>();
            equipProp.SetMaterial((Material)Resources.Load("Material/Cards/StartingEquipement/herald_armour_material", typeof(Material)));
            equipProp.equipementName = "Herald Armour";
            equipProp.name = "Herald Armour";
            equipProp.strenghRequirement = 10;
            equipProp.faithRequirement = 10;
            equipProp.blackArmorDices = 1;
            equipProp.blackMagicArmorDices = 1;
            equipProp.dodgeRollsDices = 1;

            prop.rightEquipement = Instantiate(Resources.Load("Asset/Cards/EquipementCard"), generatedCardHolder) as GameObject;
            prop.rightEquipement.transform.Rotate(45, 180, 0);
            equipProp = prop.rightEquipement.GetComponent<EquipementProperties>();
            equipProp.SetMaterial((Material)Resources.Load("Material/Cards/StartingEquipement/herald_spear_material", typeof(Material)));
            equipProp.equipementName = "Spear Axe";
            equipProp.name = "Spear Axe";
            equipProp.attackList = new List<BehaviorAction>()
            {
                new AttackAction()
                {
                    BlackDices = 1,
                    StaminaCost = 0,
                    MinimumRange = 1,
                    Range = 1
                },
                new AttackAction()
                {
                    BlackDices = 1,
                    FlatModifier = 1,
                    StaminaCost = 3,
                    MinimumRange = 1,
                    Range = 1
                }
        };

            prop.sideEquipement = Instantiate(Resources.Load("Asset/Cards/EquipementCard"), generatedCardHolder) as GameObject;
            prop.sideEquipement.transform.Rotate(45, 180, 0);
            equipProp = prop.sideEquipement.GetComponent<EquipementProperties>();
            equipProp.SetMaterial((Material)Resources.Load("Material/Cards/StartingEquipement/herald_talisman_material", typeof(Material)));
            equipProp.equipementName = "Talisman";
            equipProp.name = "Talisman";
            equipProp.strenghRequirement = 12;
            equipProp.faithRequirement = 12;
            equipProp.attackList = new List<BehaviorAction>()
            {
                new AttackAction()
                {
                    StaminaCost = 0,
                    Range = 2,
                    FlatModifier = -2,
                },
                new AttackAction()
                {
                    StaminaCost = 0,
                    Range = 2,
                    FlatModifier = -6
                }
        };
        }

        prop = pProps.FirstOrDefault(p => p.playerType == PlayerClassEnum.Knight);
        if (prop != null)
        {
            prop.initiative = 10;
            prop.leftEquipement = Instantiate(Resources.Load("Asset/Cards/EquipementCard"), generatedCardHolder) as GameObject;
            var equipProp = prop.leftEquipement.GetComponent<EquipementProperties>();
            equipProp.SetMaterial((Material)Resources.Load("Material/Cards/StartingEquipement/knight_shield_material", typeof(Material)));
            equipProp.equipementName = "Kite Shield";
            equipProp.name = "Kite Shield";
            equipProp.blackArmorDices = 1;

            prop.armourEquipement = Instantiate(Resources.Load("Asset/Cards/EquipementCard"), generatedCardHolder) as GameObject;
            equipProp = prop.armourEquipement.GetComponent<EquipementProperties>();
            equipProp.SetMaterial((Material)Resources.Load("Material/Cards/StartingEquipement/knight_armour_material", typeof(Material)));
            equipProp.equipementName = "Knight Armour";
            equipProp.name = "Knight Armour";
            equipProp.strenghRequirement = 12;
            equipProp.blueArmorDices = 1;
            equipProp.blackMagicArmorDices = 1;

            prop.rightEquipement = Instantiate(Resources.Load("Asset/Cards/EquipementCard"), generatedCardHolder) as GameObject;
            equipProp = prop.rightEquipement.GetComponent<EquipementProperties>();
            equipProp.SetMaterial((Material)Resources.Load("Material/Cards/StartingEquipement/knight_sword_material", typeof(Material)));
            equipProp.equipementName = "Long Sword";
            equipProp.name = "Long Sword";
            equipProp.strenghRequirement = 13;
            equipProp.dextirityRequirement = 12;
            equipProp.attackList = new List<BehaviorAction>()
            {
                new AttackAction()
                {
                    BlueDices = 1,
                    StaminaCost = 0,
                },
                new AttackAction()
                {
                    BlackDices = 1,
                    BlueDices = 1,
                    StaminaCost = 4,
                }
        };
        }

        prop = pProps.FirstOrDefault(p => p.playerType == PlayerClassEnum.Assassin);
        if (prop != null)
        {
            prop.initiative = 8;
            prop.leftEquipement = Instantiate(Resources.Load("Asset/Cards/EquipementCard"), generatedCardHolder) as GameObject;
            var equipProp = prop.leftEquipement.GetComponent<EquipementProperties>();
            equipProp.SetMaterial((Material)Resources.Load("Material/Cards/StartingEquipement/assassin_shield_material", typeof(Material)));
            equipProp.equipementName = "Target Shield";
            equipProp.name = "Target Shield";
            equipProp.dodgeRollsDices = 1;

            prop.armourEquipement = Instantiate(Resources.Load("Asset/Cards/EquipementCard"), generatedCardHolder) as GameObject;
            equipProp = prop.armourEquipement.GetComponent<EquipementProperties>();
            equipProp.SetMaterial((Material)Resources.Load("Material/Cards/StartingEquipement/assassin_armour_material", typeof(Material)));
            equipProp.equipementName = "Assassin Armour";
            equipProp.name = "Assassin Armour";
            equipProp.strenghRequirement = 10;
            equipProp.dextirityRequirement = 14;
            equipProp.blackArmorDices = 1;
            equipProp.blackMagicArmorDices = 1;
            equipProp.dodgeRollsDices = 1;

            prop.rightEquipement = Instantiate(Resources.Load("Asset/Cards/EquipementCard"), generatedCardHolder) as GameObject;
            equipProp = prop.rightEquipement.GetComponent<EquipementProperties>();
            equipProp.SetMaterial((Material)Resources.Load("Material/Cards/StartingEquipement/assassin_sword_material", typeof(Material)));
            equipProp.equipementName = "Estoc";
            equipProp.name = "Estoc";
            equipProp.dextirityRequirement = 14;
            equipProp.attackList = new List<BehaviorAction>()
            {
                new AttackAction()
                {
                    BlueDices = 2,
                    FlatModifier = -1,
                    StaminaCost = 0,
                },
                new AttackAction()
                {
                    BlackDices = 3,
                    FlatModifier = -1,
                    StaminaCost = 3,
            }
        };
        }
    }

    private void PlayerDiedAtPosition(GameObject defeatedUnit)
    {
        DropSouls(defeatedUnit);
        RestPartyAtBonefire(null);
    }

    private void RestPartyAtBonefire(GameObject _)
    {
        MoveToBonefire();
        bonefireSparks--;
        ResetPlayers();
    }

    private void DropSouls(GameObject defeatedUnit)
    {

        foreach (var tile in tiles)
        {
            foreach (var position in tile.GetComponent<TileManager>().GetPositions())
            {
                position.SoulCache = 0;
            }
        }

        var positionToDrop = GetActiveTile().GetUnitPosition(defeatedUnit);
        positionToDrop.AddSoulCache(soulCache);
        EventManager.RaiseEvent(ObjectEventType.AddSoulsToCache, -soulCache);
    }

    private void MoveToBonefire()
    {
        if (!bonefireTile.isFocused)
        {
            GetActiveTile().isFocused = false;
            bonefireTile.isFocused = true;
        }

        EventManager.RaiseEvent(GameObjectEventType.TileFocused, bonefireTile.gameObject);
    }

    private void ResetPlayers()
    {
        foreach (var unit in players.Select(u => u.GetComponent<UnitBasicProperties>()))
        {
            unit.ResetStaminaAndInjuries();
            unit.RemoveStatusEffect(false);
            unit.hasActivationToken = false;
        }
    }

}
