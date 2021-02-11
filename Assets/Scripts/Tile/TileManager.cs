﻿using Assets.Scripts.Tile;
using Assets.Scripts.Unit.Model.Attacks;
using BoardGame.Script.Events;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class TileManager : MonoBehaviour
{
    public bool isCleared = false;
    public bool isFocused = false;

    public List<UnitBasicProperties> players = new List<UnitBasicProperties>();
    public List<UnitBasicProperties> enemies = new List<UnitBasicProperties>();

    public Dictionary<GameObject, UnitBasicProperties> playersToProperties = new Dictionary<GameObject, UnitBasicProperties>();
    public Dictionary<GameObject, UnitBasicProperties> enemyToProperties = new Dictionary<GameObject, UnitBasicProperties>();

    protected List<PositionBehavior> positions = new List<PositionBehavior>();

    public TileMonsterSettings monsterSettings;

    protected EnemyGenerator enemyGenerator;
    private EnemyAutoTurnResolver enemyAi;

    private List<PositionBehavior> currentPath;
    private List<UnitBasicProperties> currentUnitInBrillance;
    private AttackDetail currentSelectedAttack;

    private UnitBasicProperties currentUnit;
    private bool currentUnitHasMoved;
    private bool currentUnitHasAttacked;
    private bool canMove => !currentUnitHasMoved || (currentUnitHasMoved && !currentUnitHasAttacked);

    private int LastActivePlayer = 0;
    private int LastActiveEnemy = 0;
    private bool firstMovementFree = true;

    internal bool isHovered = false;
    internal string enteredFrom;

    void Start()
    {
        if (transform.Find("Positions") != null)
        {
            foreach (Transform child in transform.Find("Positions").transform)
            {
                positions.Add(child.GetComponent<PositionBehavior>());
            }
        }

        enemyAi = FindObjectOfType<EnemyAutoTurnResolver>();
        enemyGenerator = GetComponent<EnemyGenerator>();
        currentPath = new List<PositionBehavior>();
        currentUnitInBrillance = new List<UnitBasicProperties>();

        EventManager.StartListening(GameObjectEventType.TileIsEntered, IntializeFocusedTileHandler);

        StartInternal();
    }

    protected virtual void StartInternal()
    {
    }

    void Update()
    {
        UpdateInternal();
    }

    protected virtual void UpdateInternal()
    {
        if (isFocused && isHovered && !isCleared && Input.GetButtonDown("End Turn"))
        {
            ActivateNextUnit();
        }

        if (isFocused && Input.GetButtonDown("Debug Clear Tile"))
        {
            Cleared();
        }
    }

    private void EndUnitTurn(GameObject _)
    {
        ActivateNextUnit();
    }

    private void ActivateNextUnit()
    {
        firstMovementFree = true;
        bool isEnemyTurn = false;

        if (currentUnit == null)
        {
            // first turn, set the  next player to be the one carying the activation token, or the first players by default.
            isEnemyTurn = true;
            LastActiveEnemy = -1;

            var lastactivePlayer = players.FirstOrDefault(p => p.hasActivationToken);
            if (lastactivePlayer == null)
            {
                LastActivePlayer = -1;
            }
            else
            {
                LastActivePlayer = players.IndexOf(lastactivePlayer) - 1;
            }
        }
        else
        {
            currentUnit.EndOfTurn();
            if (currentUnit.side == UnitSide.Hollow && LastActiveEnemy >= enemies.Count - 1)
            {
                LastActivePlayer = LastActivePlayer < players.Count - 1 ? LastActivePlayer : -1;
            }
            else
            {
                isEnemyTurn = true;
                if (currentUnit.side == UnitSide.Player)
                {
                    LastActiveEnemy = -1;
                }
            }
        }


        currentUnitHasAttacked = false;
        currentUnitHasMoved = false;
        CheckUnitsAlive();

        if (isEnemyTurn)
        {
            LastActiveEnemy++;
            currentUnit = enemies[LastActiveEnemy];
            //enemyAi.ResolveEnemyTurn(enemies[LastActiveEnemy].gameObject);
        }
        else
        {
            RemoveAllAggroToken();
            LastActivePlayer++;
            currentUnit = players[LastActivePlayer];
        }

        currentUnit.Activate();
    }

    private void RemoveAllAggroToken()
    {
        foreach (var player in players)
        {
            player.hasAggroToken = false;
        }
    }

    #region Battle Preparation
    private void IntializeFocusedTileHandler(GameObject _)
    {
        PrepareTileEntered();
    }

    public virtual void PrepareTileEntered()
    {

        if (isFocused && isHovered && !isCleared)
        {
            foreach (var child in positions)
            {
                child.GetComponent<PositionBehavior>().PositionClicked += PositionClicked;
            }

            EventManager.StartListening(GameObjectEventType.PositionHovered, ShowPath);
            EventManager.StartListening(GameObjectEventType.PositionHoveredExit, StopShowPath);
            EventManager.StartListening(ObjectEventType.AttackSelected, ShowSelectedAttackTargets);
            EventManager.StartListening(ObjectEventType.AttackDeselected, HideSelectedAttackTargets);
            EventManager.StartListening(ObjectEventType.AttackHovered, ShowAvailableAttackTargets);
            EventManager.StartListening(ObjectEventType.AttackHoverEnded, HideHoveredAttackTargets);
            EventManager.StartListening(GameObjectEventType.AttackTargetSelected, ApplyAttack);
            EventManager.StartListening(GameObjectEventType.UnitDestroyed, UnitDestroyed);
            EventManager.StartListening(ObjectEventType.UnitMoved, UnitMoved);
            EventManager.StartListening(GameObjectEventType.EndUnitTurn, EndUnitTurn);

            foreach (var position in positions)
            {
                position.ResetPosition(true);
            }

            SetupEnemies();
            SetupPlayers();
            ActivateNextUnit();
        }
    }

    private void SetupPlayers()
    {

        playersToProperties = GameObject.FindGameObjectsWithTag("Player")
            .ToDictionary(p => p, p => p.GetComponent<UnitBasicProperties>());
        players = playersToProperties.Values.OrderByDescending(p => p.initiative).ToList();

        foreach (var player in players)
        {
            player.EndOfTurn();
        }

        var potentialPositions = GetAuthorizedEntryPositions(enteredFrom);
        foreach (var player in players)
        {
            bool positioned = false;
            while (!positioned)
            {
                var index = Random.Range(0, potentialPositions.Count());
                var slot = potentialPositions[index];
                if (!slot.HasMaxUnit())
                {
                    slot.AddNonBossUnit(player.gameObject);
                    positioned = true;
                }
            }
        }
    }

    private void SetupEnemies()
    {
        enemies.Clear();
        for (int i = 0; i < monsterSettings.swordhollowSoldierCount; i++)
        {
            var enemy = enemyGenerator.CreateHollowSoldier(gameObject.transform);
            positions.First(p => p.isSpawnTwo).AddNonBossUnit(enemy);
            enemies.Add(enemy.GetComponent<UnitBasicProperties>());
            EventManager.RaiseEvent(GameObjectEventType.EnemyCreated, enemy);
        }

        for (int i = 0; i < monsterSettings.arbalestHollowSoldierCount; i++)
        {
            var enemy = enemyGenerator.CreateCrossbowHollowSoldier(gameObject.transform);
            positions.First(p => p.isSpawnOne).AddNonBossUnit(enemy);
            enemies.Add(enemy.GetComponent<UnitBasicProperties>());
            EventManager.RaiseEvent(GameObjectEventType.EnemyCreated, enemy);
        }

        enemies = enemies.OrderBy(p => p.initiative).ToList();
        foreach (var enemy in enemies)
        {
            enemy.ResetStaminaAndInjuries();
            enemy.Deactivate();
        }

        enemyToProperties = enemies.ToDictionary(e => e.gameObject, e => e);
    }

    private IList<PositionBehavior> GetAuthorizedEntryPositions(string enteredFrom)
    {
        if (string.IsNullOrEmpty(enteredFrom))
        {
            return positions;
        }
        else
        {
            if (enteredFrom.Contains("west"))
            {
                return positions.Where(p => p.isWest).ToList();
            }
            if (enteredFrom.Contains("east"))
            {
                return positions.Where(p => p.isEast).ToList();
            }
            if (enteredFrom.Contains("north"))
            {
                return positions.Where(p => p.isNorth).ToList();
            }
            if (enteredFrom.Contains("south"))
            {
                return positions.Where(p => p.isSouth).ToList();
            }

            Debug.LogError($"room {gameObject.name} entered from side {enteredFrom} not recognized in preset of potential entrance.");
            return positions;
        }
    }
    #endregion

    #region Exit/Clean
    public virtual void ExitTile()
    {
        foreach (var position in positions)
        {
            position.ResetPosition(false);
        }

        foreach (var unit in enemies.ToList())
        {
            RemoveEnemy(unit);
        }

        foreach (var unit in players)
        {
            unit.Deactivate();
        }

        EventManager.StopListening(GameObjectEventType.PositionHovered, ShowPath);
        EventManager.StopListening(GameObjectEventType.PositionHoveredExit, StopShowPath);
        EventManager.StopListening(ObjectEventType.AttackSelected, ShowSelectedAttackTargets);
        EventManager.StopListening(ObjectEventType.AttackDeselected, HideHoveredAttackTargets);
        EventManager.StopListening(ObjectEventType.AttackHovered, ShowAvailableAttackTargets);
        EventManager.StopListening(ObjectEventType.AttackHoverEnded, HideHoveredAttackTargets);
        EventManager.StopListening(GameObjectEventType.AttackTargetSelected, ApplyAttack);
        EventManager.StopListening(GameObjectEventType.UnitDestroyed, UnitDestroyed);
        EventManager.StopListening(ObjectEventType.UnitMoved, UnitMoved);
        EventManager.StopListening(GameObjectEventType.EndUnitTurn, EndUnitTurn);

        foreach (var child in positions)
        {
            child.GetComponent<PositionBehavior>().PositionClicked -= PositionClicked;
        }

        EventManager.RaiseEvent(GameObjectEventType.ResetAndHideEnemyDisplays);
        EventManager.RaiseEvent(GameObjectEventType.ResetAndHideAttackDial);
    }

    private void RemoveEnemy(UnitBasicProperties enemy)
    {
        enemy.ClearEquipement();
        enemyToProperties.Remove(enemy.gameObject);
        enemies.Remove(enemy);
        foreach (var position in positions)
        {
            position.RemoveNonBossUnit(enemy.gameObject);
        }
        Destroy(enemy.gameObject);
    }

    private void Cleared()
    {
        isCleared = true;

        foreach (var unit in players)
        {
            unit.ResetStaminaAndInjuries();
            unit.RemoveStatusEffect(false);
            unit.hasActivationToken = false;
        }

        var nextActiveplayer = LastActivePlayer < players.Count - 1 ? LastActivePlayer + 1 : 0;
        players[nextActiveplayer].hasActivationToken = true;

        EventManager.RaiseEvent(GameObjectEventType.TileCleared, gameObject);
    }
    #endregion

    #region Path
    public List<PositionBehavior> GetPositions()
    {
        return positions.ToList();
    }

    private void PositionClicked(GameObject position)
    {
        if (canMove)
        {
            var currentUnitObject = currentUnit.gameObject;
            var pathcost = PathFinder.GetPathStaminaCost(currentPath, firstMovementFree, currentUnit.isFrozen);

            if (currentUnit.HasEnoughStaminaToMove() <= pathcost)
            {
                return;
            }

            var moveCommand = new UnitMovement()
            {
                MoveFrom = positions.FirstOrDefault(p => p.HasUnit(currentUnitObject)),
                MoveTo = positions.First(pos => pos.gameObject == position),
                Unit = currentUnitObject
            };

            UnitMoved(moveCommand);
            firstMovementFree = false;
            currentUnitHasMoved = true;
            currentUnit.ConsumeStamina(pathcost);

            EventManager.RaiseEvent(GameObjectEventType.ActiveUnitMoved, currentUnit.gameObject);
        }
    }

    private void UnitMoved(object unitMovement)
    {
        var movement = (UnitMovement)unitMovement;
        movement.MoveFrom.RemoveNonBossUnit(movement.Unit);
        movement.MoveTo.AddNonBossUnit(movement.Unit);
    }

    private void ShowPath(GameObject targetPosition)
    {
        if (canMove)
        {
            var starting = positions.First(p => p.HasActiveUnit());
            var target = positions.First(p => p.name == targetPosition.name);

            var path = PathFinder.GetPath(starting, target);
            if (path != null)
            {
                currentPath = path;
                foreach (var node in path)
                {
                    node.Show();
                }
            }
        }
    }

    private void StopShowPath(GameObject targetPosition)
    {
        currentPath.Clear();
        foreach (var node in positions)
        {
            node.Hide();
        }
    }

    public PositionBehavior GetUnitPosition(GameObject unit)
    {
        foreach (var position in positions)
        {
            if (position.HasUnit(unit))
            {
                return position;
            }
        }
        return null;
    }
    #endregion

    #region Attacks
    private void ShowSelectedAttackTargets(object attackObject)
    {
        currentSelectedAttack = (AttackDetail)attackObject;
        ShowAvailableAttackTargets(attackObject);
    }

    private void ShowAvailableAttackTargets(object attackObject)
    {
        InternalHideAttackTargets();

        var currentAttack = (AttackDetail)attackObject;
        var startingPosition = GetUnitPosition(currentUnit.gameObject);

        var targets = currentAttack.FindTargetsInRange(currentUnit, startingPosition, positions);
        foreach (var unit in targets)
        {
            var properties = unit.GetComponent<UnitBasicProperties>();
            properties.ShowHoverBrillance();
            currentUnitInBrillance.Add(properties);
        }
    }

    private void HideSelectedAttackTargets(object deselectedLoad)
    {
        var deselectedAttack = (AttackDetail)deselectedLoad;
        if (deselectedAttack == currentSelectedAttack)
        {
            InternalHideAttackTargets();
            currentSelectedAttack = null;
        }
    }

    private void HideHoveredAttackTargets(object _)
    {
        InternalHideAttackTargets();
        if (currentSelectedAttack != null)
        {
            ShowAvailableAttackTargets(currentSelectedAttack);
        }
    }

    private void InternalHideAttackTargets()
    {
        foreach (var unit in currentUnitInBrillance)
        {
            unit.HideHoverBrillance();
        }
        currentUnitInBrillance.Clear();
    }

    private void ApplyAttack(GameObject target)
    {
        var targetProperties = target.GetComponent<UnitBasicProperties>();
        ComputeAndApplyAttack(currentSelectedAttack, targetProperties);
    }

    private void ComputeAndApplyAttack(AttackDetail attack, UnitBasicProperties originalTarget)
    {
        var sourcePosition = positions.FirstOrDefault(p => p.HasActiveUnit());
        var targetPosition = positions.FirstOrDefault(p => p.HasUnit(originalTarget.gameObject));

        var encounter = new Encounter();
        encounter.Attacker = currentUnit;
        encounter.Defender = originalTarget;
        encounter.Attack = attack;
        encounter.Defense = originalTarget.GetDefenseDices(attack.MagicAttack);

        EventManager.RaiseEvent(ObjectEventType.EncounterToResolve, encounter);
        EventManager.StartListening(ObjectEventType.EncountersResolved, CheckUnitStatusAfterEncounter);
    }

    private void CheckUnitStatusAfterEncounter(object _)
    {
        EventManager.StopListening(ObjectEventType.EncountersResolved, CheckUnitStatusAfterEncounter);

        var currentAttack = currentSelectedAttack;

        currentUnitHasAttacked = true;
        currentSelectedAttack = null;

        InternalHideAttackTargets();
        CheckUnitsAlive();
        EventManager.RaiseEvent(ObjectEventType.AttackApplied, currentAttack);


    }

    public void CheckUnitsAlive()
    {
        foreach (var enemy in enemies.ToList())
        {
            if (enemy.StaminaLeft() <= 0)
            {
                EventManager.RaiseEvent(GameObjectEventType.UnitDestroyed, enemy.gameObject);
            }
        }
        foreach (var player in players.ToList())
        {
            if (player.StaminaLeft() <= 0)
            {
                EventManager.RaiseEvent(GameObjectEventType.UnitDestroyed, player.gameObject);
            }
        }
    }

    private void UnitDestroyed(GameObject unit)
    {
        if (playersToProperties.TryGetValue(unit, out var unitProperties))
        {
            // player is dead, back to bonefire, party is defeated.
        }
        else if (enemyToProperties.TryGetValue(unit, out var enemyProperties))
        {
            RemoveEnemy(enemyProperties);
            if (enemies.Count == 0)
            {
                Cleared();
                EventManager.RaiseEvent(GameObjectEventType.ResetAndHideAttackDial, gameObject);
            }
        }
    }
    #endregion
}
