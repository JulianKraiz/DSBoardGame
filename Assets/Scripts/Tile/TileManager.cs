using Assets.Scripts.Tile;
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

    public List<PositionBehavior> positions = new List<PositionBehavior>();

    public TileMonsterSettings monsterSettings;

    protected EnemyGenerator enemyGenerator;
    protected PathFinder pathFinder;

    private List<PositionBehavior> currentPath;
    private List<UnitBasicProperties> currentUnitInBrillance;
    private AttackDetail currentSelectedAttack;

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

        enemyGenerator = GetComponent<EnemyGenerator>();
        pathFinder = new PathFinder();
        currentPath = new List<PositionBehavior>();
        currentUnitInBrillance = new List<UnitBasicProperties>();

        EventManager.StartListeningGameObject(EventTypes.TileIsEntered, IntializeFocusedTileHandler);

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

    private void ActivateNextUnit()
    {
        firstMovementFree = true;

        bool isEnemyTurn = false;

        var previousPlayer = players.FirstOrDefault(p => p.isActive);
        var activeEnemy = enemies.FirstOrDefault(p => p.isActive);
        if (previousPlayer != null)
        {
            // next turn is enemy
            previousPlayer.Deactivate();
            isEnemyTurn = true;
            LastActiveEnemy = -1;
        }
        else if (activeEnemy != null)
        {
            activeEnemy.Deactivate();
            if (LastActiveEnemy < enemies.Count - 1)
            {
                // next turn is still enemy
                isEnemyTurn = true;
            }
            else
            {
                // next turn is player.
                LastActivePlayer = LastActivePlayer < players.Count - 1 ? LastActivePlayer : -1;
            }
        }
        else
        {
            isEnemyTurn = true;
            LastActiveEnemy = -1;

            // first turn, set the  next player to be the one carying the activation token, or the first players by default.
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

        if (isEnemyTurn)
        {
            LastActiveEnemy++;
            enemies[LastActiveEnemy].Activate();
        }
        else
        {
            LastActivePlayer++;
            players[LastActivePlayer].Activate();
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

            EventManager.StartListeningGameObject(EventTypes.PositionHovered, ShowPath);
            EventManager.StartListeningGameObject(EventTypes.PositionHoveredExit, StopShowPath);
            EventManager.StartListeningObject(EventTypes.AttackSelected, ShowSelectedAttackTargets);
            EventManager.StartListeningObject(EventTypes.AttackDeselected, HideSelectedAttackTargets);
            EventManager.StartListeningObject(EventTypes.AttackHovered, ShowAvailableAttackTargets);
            EventManager.StartListeningObject(EventTypes.AttackHoverEnded, HideHoveredAttackTargets);
            EventManager.StartListeningGameObject(EventTypes.AttackTargetSelected, ApplyAttack);
            EventManager.StartListeningGameObject(EventTypes.UnitDestroyed, UnitDestroyed);

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
            player.ResetStaminaAndInjuries();
            player.Deactivate();
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

        // should be determined before entering in a room.
        players[Random.Range(0, players.Count - 1)].hasAggroToken = true;
    }

    private void SetupEnemies()
    {
        enemies.Clear();
        for (int i = 0; i < monsterSettings.swordhollowSoldierCount; i++)
        {
            var enemy = enemyGenerator.CreateHollowSoldier(gameObject.transform);
            positions.First(p => p.isSpawnTwo).AddNonBossUnit(enemy);
            enemies.Add(enemy.GetComponent<UnitBasicProperties>());
            EventManager.RaiseEventGameObject(EventTypes.EnemyCreated, enemy);
        }

        for (int i = 0; i < monsterSettings.arbalestHollowSoldierCount; i++)
        {
            var enemy = enemyGenerator.CreateCrossbowHollowSoldier(gameObject.transform);
            positions.First(p => p.isSpawnOne).AddNonBossUnit(enemy);
            enemies.Add(enemy.GetComponent<UnitBasicProperties>());
            EventManager.RaiseEventGameObject(EventTypes.EnemyCreated, enemy);
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
            unit.hasAggroToken = false;
            unit.hasActivationToken = false;
        }

        EventManager.StopListeningGameObject(EventTypes.PositionHovered, ShowPath);
        EventManager.StopListeningGameObject(EventTypes.PositionHoveredExit, StopShowPath);
        EventManager.StopListeningObject(EventTypes.AttackSelected, ShowSelectedAttackTargets);
        EventManager.StopListeningObject(EventTypes.AttackDeselected, HideHoveredAttackTargets);
        EventManager.StopListeningObject(EventTypes.AttackHovered, ShowAvailableAttackTargets);
        EventManager.StopListeningObject(EventTypes.AttackHoverEnded, HideHoveredAttackTargets);
        EventManager.StopListeningGameObject(EventTypes.AttackTargetSelected, ApplyAttack);
        EventManager.StopListeningGameObject(EventTypes.UnitDestroyed, UnitDestroyed);

        foreach (var child in positions)
        {
            child.GetComponent<PositionBehavior>().PositionClicked -= PositionClicked;
        }

        EventManager.RaiseEventGameObject(EventTypes.ResetAndHideEnemyDisplays);
        EventManager.RaiseEventObject(EventTypes.ResetAndHideAttackDial);
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
        }

        var nextActiveplayer = LastActivePlayer < players.Count - 1 ? LastActivePlayer + 1 : 0;
        players[nextActiveplayer].hasActivationToken = true;

        EventManager.RaiseEventGameObject(EventTypes.TileCleared, gameObject);
    }
    #endregion

    #region path
    private void PositionClicked(GameObject position)
    {
        var pathcost = pathFinder.GetPathhStaminaCost(currentPath) - (firstMovementFree ? 1 : 0);

        var currentUnit = players.FirstOrDefault(p => p.isActive) ?? enemies.FirstOrDefault(p => p.isActive);
        var currentUnitObject = currentUnit.gameObject;

        if (currentUnit.HasEnoughStaminaToMove() <= pathcost)
        {
            return;
        }

        firstMovementFree = false;

        var currentPosition = positions.FirstOrDefault(p => p.HasUnit(currentUnitObject));
        currentPosition.RemoveNonBossUnit(currentUnitObject);

        var targetposition = positions.First(pos => pos.gameObject == position);
        targetposition.AddNonBossUnit(currentUnitObject);

        currentUnit.ConsumeStamina(pathcost);

        EventManager.RaiseEventGameObject(EventTypes.ActiveUnitMoved, currentUnit.gameObject);
    }

    private void ShowPath(GameObject targetPosition)
    {
        var starting = positions.First(p => p.HasActiveUnit());
        var target = positions.First(p => p.name == targetPosition.name);

        var path = pathFinder.GetPath(starting, target);
        if (path != null)
        {
            currentPath = path;
            foreach (var node in path)
            {
                node.Show();
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

    private Dictionary<PositionBehavior, int> GetAllNodeWithinRange(int minRange, int maxRange, PositionBehavior start)
    {
        var result = new Dictionary<PositionBehavior, int>();

        var otherNodes = positions.ToList();

        foreach (var other in otherNodes)
        {
            var path = pathFinder.GetPath(start, other);
            if (path != null && path.Count >= minRange && path.Count <= maxRange)
            {
                result.Add(other, path.Count);
            }
        }
        return result;
    }
    #endregion

    #region attacks
    private void ShowSelectedAttackTargets(object attackObject)
    {
        currentSelectedAttack = (AttackDetail)attackObject;
        ShowAvailableAttackTargets(attackObject);
    }

    private void ShowAvailableAttackTargets(object attackObject)
    {
        InternalHideAttackTargets();

        var currentAttack = (AttackDetail)attackObject;

        var currentSide = players.Any(p => p.isActive) ? UnitSide.Player : UnitSide.Hollow;
        var targetSide = currentAttack.targetAllies ? currentSide : currentSide == UnitSide.Hollow ? UnitSide.Player : UnitSide.Hollow;

        var minRange = currentAttack.minimumRange;
        var maxRange = currentAttack.infiniteRange ? 20 : currentAttack.range;
        var startingPosition = positions.First(positions => positions.HasActiveUnit());

        var inRangeNodes = GetAllNodeWithinRange(minRange, maxRange, startingPosition).Keys;

        foreach (var node in inRangeNodes)
        {
            foreach (var unit in node.GetUnits(targetSide))
            {
                var properties = unit.GetComponent<UnitBasicProperties>();
                properties.ShowHoverBrillance();
                currentUnitInBrillance.Add(properties);
            }
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
        ComputeAndApplyAttack(currentSelectedAttack, targetProperties, currentUnitInBrillance, positions);
    }

    private void ComputeAndApplyAttack(AttackDetail attack, UnitBasicProperties originalTarget, IEnumerable<UnitBasicProperties> allAvailableTargets, IEnumerable<PositionBehavior> allPositions)
    {
        var encounters = new List<Encounter>();

        var source = players.FirstOrDefault(p => p.isActive) ?? enemies.First(u => u.isActive);

        var sourcePosition = positions.FirstOrDefault(p => p.HasActiveUnit());
        var targetPosition = positions.FirstOrDefault(p => p.HasUnit(originalTarget.gameObject));

        var currentSide = players.Any(p => p.isActive) ? UnitSide.Player : UnitSide.Hollow;
        var targetSide = attack.targetAllies ? currentSide : currentSide == UnitSide.Hollow ? UnitSide.Player : UnitSide.Hollow;

        IList<UnitBasicProperties> targets = new List<UnitBasicProperties>();
        if (attack.nodeSplash)
        {
            targets = targetPosition.GetUnits(targetSide).Select(u => u.GetComponent<UnitBasicProperties>()).ToList();
        }
        else
        {
            targets.Add(originalTarget);
        }

        foreach (var target in targets)
        {
            var encounter = new Encounter();
            encounter.Attacker = source;
            encounter.Defender = target;
            encounter.Attack = attack;
            encounter.Defense = target.GetDefenseDices(attack.magicAttack);
            encounters.Add(encounter);
        }

        EventManager.RaiseEventObject(EventTypes.EncountersToResolve, encounters);
        EventManager.StartListeningObject(EventTypes.EncountersResolved, CheckUnitStatusAfterEncounter);
    }

    private void CheckUnitStatusAfterEncounter(object encounterLoad)
    {
        EventManager.StopListeningObject(EventTypes.EncountersResolved, CheckUnitStatusAfterEncounter);
        var encounters = (List<Encounter>)encounterLoad;
        foreach (var encounter in encounters)
        {
            var target = encounter.Defender;
            if (target.StaminaLeft() <= 0)
            {
                EventManager.RaiseEventGameObject(EventTypes.UnitDestroyed, target.gameObject);
            }
        }
        EventManager.RaiseEventObject(EventTypes.AttackApplied, currentSelectedAttack);
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
                EventManager.RaiseEventObject(EventTypes.ResetAndHideAttackDial, gameObject);
            }
        }
    }
    #endregion
}
