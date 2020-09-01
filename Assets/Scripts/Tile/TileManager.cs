using Assets.Scripts.Tile;
using BoardGame.Script.Events;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public bool isCleared = false;
    public bool isFocused = false;

    public List<UnitBasicProperties> players = new List<UnitBasicProperties>();
    public List<UnitBasicProperties> enemies = new List<UnitBasicProperties>();

    public List<PositionBehavior> positions = new List<PositionBehavior>();

    public TileMonsterSettings monsterSettings;

    protected EnemyGenerator enemyGenerator;
    protected PathFinder pathFinder;

    private List<PositionBehavior> currentPath;

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

        StartInternal();
    }

    protected virtual void StartInternal()
    {
        foreach (var child in positions)
        {
            child.GetComponent<PositionBehavior>().PositionClicked += PositionClicked;
        }

        EventManager.StartListening(EventTypes.PositionHovered, ShowPath);
        EventManager.StartListening(EventTypes.PositionHoveredExit, StopShowPath);
    }

    void Update()
    {
        UpdateInternal();
    }

    protected virtual void UpdateInternal()
    {
        if (isFocused && isHovered && Input.GetButtonDown("End Turn"))
        {
            firstMovementFree = true;
            bool isEnemyTurn = false;

            var previousPlayer = players.FirstOrDefault(p => p.isActive);
            if (previousPlayer != null)// || enemies.IndexOf(enemies.FirstOrDefault(p => p.isActive)) < enemies.Count - 1)
            {
                previousPlayer.isActive = false;
                isEnemyTurn = true;
                LastActiveEnemy = -1;
            }
            else
            {
                enemies.FirstOrDefault(p => p.isActive).isActive = false;
                if (LastActiveEnemy < enemies.Count - 1)
                {
                    isEnemyTurn = true;
                }
            }

            if (isEnemyTurn)
            {
                LastActiveEnemy++;
                enemies[LastActiveEnemy].isActive = true;
                EventManager.RaiseEvent(EventTypes.UnitIsActivated, enemies[LastActiveEnemy].gameObject);
            }
            else
            {
                LastActivePlayer = LastActivePlayer < players.Count - 1 ? ++LastActivePlayer : 0;

                players[LastActivePlayer].hasActivationToken = true;
                players[LastActivePlayer].hasAggroToken = true;
                players[LastActivePlayer].isActive = true;
                players[LastActivePlayer].stamina-=2;
                EventManager.RaiseEvent(EventTypes.UnitIsActivated, players[LastActivePlayer].gameObject);
            }
        }

        if (isFocused && Input.GetButtonDown("Debug Clear Tile"))
        {
            Cleared();
        }
    }

    #region Battle Preparation
    public virtual void PrepareTileEntered()
    {
        if (isFocused && !isCleared)
        {
            foreach (var position in positions)
            {
                position.ResetPosition(true);
            }

            SetupEnemies();
            SetupPlayers();
            SetupFirstPlayerToActivate();
        }
    }
    
    private void SetupPlayers()
    {
        players = GameObject.FindGameObjectsWithTag("Player")
              .Select(p => p.GetComponent<UnitBasicProperties>())
              .OrderByDescending(p => p.initiative)
              .ToList();

        foreach (var player in players)
        {
            player.isActive = false;
        }

        var potentialPositions = GetAuthorizedEntryPositions(enteredFrom);
        foreach (var player in players)
        {
            bool positioned = false;
            while (!positioned)
            {
                var index = UnityEngine.Random.Range(0, potentialPositions.Count());
                var slot = potentialPositions[index];
                if (!slot.HasMaxUnit())
                {
                    slot.AddNonBossUnit(player.gameObject);
                    positioned = true;
                }
            }
        }

        players[UnityEngine.Random.Range(0, players.Count - 1)].hasAggroToken = true;
    }
    private void SetupFirstPlayerToActivate()
    {
        var lastactivePlayer = players.FirstOrDefault(p => p.hasActivationToken);
        if (lastactivePlayer == null)
        {
            lastactivePlayer = players.First();
        }

        LastActivePlayer = players.IndexOf(lastactivePlayer);
    }
    private void SetupEnemies()
    {
        enemies.Clear();
        for (int i = 0; i < monsterSettings.swordhollowSoldierCount; i++)
        {
            var enemy = enemyGenerator.CreateHollowSoldier(gameObject.transform);
            positions.First(p => p.isSpawnTwo).AddNonBossUnit(enemy);
            enemies.Add(enemy.GetComponent<UnitBasicProperties>());
            EventManager.RaiseEvent(EventTypes.EnemyCreated, enemy);
        }

        for (int i = 0; i < monsterSettings.arbalestHollowSoldierCount; i++)
        {
            var enemy = enemyGenerator.CreateCrossbowHollowSoldier(gameObject.transform);
            positions.First(p => p.isSpawnOne).AddNonBossUnit(enemy);
            enemies.Add(enemy.GetComponent<UnitBasicProperties>());
            EventManager.RaiseEvent(EventTypes.EnemyCreated, enemy);
        }

        enemies = enemies.OrderBy(p => p.initiative).ToList();
        enemies.First().isActive = true;
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

    #region Tile Exit/Clean
    public virtual void ExitTile()
    {
        foreach (var child in positions)
        {
            child.ResetPosition(false);
        }

        foreach (var unit in enemies)
        {
            RemoveEnemy(unit);
        }

        enemies.Clear();
    }

    private void RemoveEnemy(UnitBasicProperties enemy)
    {
        enemy.ClearEquipement();
        Destroy(enemy.gameObject);
        EventManager.RaiseEvent(EventTypes.EnemyRemoved, enemy.gameObject);
    }

    private void Cleared()
    {
        isCleared = true;

        foreach (var position in positions)
        {
            position.ResetPosition(false);
        }

        foreach (var player in players)
        {
            player.hasAggroToken = false;
            player.isActive = false;
            player.hasActivationToken = false;
        }

        var nextActiveplayer = LastActivePlayer < players.Count - 1 ? LastActivePlayer + 1 : 0;
        players[nextActiveplayer].hasActivationToken = true;


        EventManager.RaiseEvent(EventTypes.TileCleared, gameObject);
    }
    #endregion

    private void PositionClicked(GameObject position)
    {
        if (isFocused)
        {
            var pathcost = pathFinder.GetPathhStaminaCost(currentPath) - (firstMovementFree ? 1 : 0);

            var currentUnit = players.FirstOrDefault(p => p.isActive) ?? enemies.FirstOrDefault(p => p.isActive);
            var currentUnitObject = currentUnit.gameObject;

            if(currentUnit.StaminaLeft() <= pathcost)
            {
                return;
            }

            firstMovementFree = false;

            var currentPosition = positions.FirstOrDefault(p => p.HasUnit(currentUnitObject));
            currentPosition.RemoveNonBossUnit(currentUnitObject);

            var targetposition = positions.First(pos => pos.gameObject == position);
            targetposition.AddNonBossUnit(currentUnitObject);

            currentUnit.stamina += pathcost;
        }
    }

    private void ShowPath(GameObject targetPosition)
    {
        if (isFocused && isHovered)
        {
            var starting = positions.First(p => p.HasActiveUnit());
            var target = positions.First(p => p.name == targetPosition.name);

            var path = pathFinder.GetPath(starting, target, new List<PositionBehavior>());
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
}
