using Assets.Scripts.Tile;
using Assets.Scripts.Unit.Model.Attacks;
using BoardGame.Script.Events;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Unit
{
    public class EnemyAttackResolver : MonoBehaviour
    {
        private GameStateManager gameState;

        private void Start()
        {
            gameState = FindObjectOfType<GameStateManager>();
        }

        public void ExecuteAttack(EnemyProperties enemy, AttackAction attack)
        {
            var tile = gameState.GetActiveTile();
            var positions = tile.GetPositions();
            var currentPosition = positions.First(p => p.HasUnit(enemy.gameObject));

            var target = FindClosestTarget(enemy, attack, currentPosition);

            EventManager.RaiseEvent(ObjectEventType.AttackSelected, attack);
            EventManager.RaiseEvent(GameObjectEventType.AttackTargetSelected, target);
        }

        private GameObject FindClosestTarget(EnemyProperties attacker, AttackAction attack, PositionBehavior currentPosition)
        {
            var tile = gameState.GetActiveTile();
            var positions = tile.GetPositions();

            var closestPlayers = new List<GameObject>();
            var closestDistance = 20;


            var targetsInRange = attack.FindTargetsInRange(attacker, currentPosition, positions);
            foreach (var target in targetsInRange)
            {
                if (attack.TargetPreference == PreferedTarget.Aggro && !target.hasAggroToken)
                {
                    continue;
                }

                var position = tile.GetUnitPosition(target.gameObject);
                var distance = PathFinder.GetPath(currentPosition, position).Count;

                if (distance < closestDistance)
                {
                    closestPlayers.Clear();
                    closestPlayers.Add(target.gameObject);
                    closestDistance = distance;
                }
                else if (distance == closestDistance)
                {
                    closestPlayers.Add(target.gameObject);
                }
            }

            if (!closestPlayers.Any())
            {
                return null;
            }

            var closestProperties = closestPlayers.Select(p => p.GetComponent<PlayerProperties>()).ToList();
            if (closestProperties.Count == 1)
            {
                return closestProperties.First().gameObject;
            }
            if (closestProperties.Count > 1)
            {
                var aggro = closestProperties.FirstOrDefault(p => p.hasAggroToken);
                if (aggro != null)
                {
                    return aggro.gameObject;
                }
                else
                {
                    var taunt = closestProperties.OrderByDescending(p => p.initiative).First();
                    return taunt.gameObject;
                }
            }

            return null;
        }
    }
}
