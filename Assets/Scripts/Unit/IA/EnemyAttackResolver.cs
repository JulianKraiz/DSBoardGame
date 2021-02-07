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
        public void ExecuteAttack(EnemyProperties enemy, AttackDetail attack)
        {
            var gameState = FindObjectOfType<GameStateManager>();
            var tile = gameState.GetActiveTile();
            var positions = tile.GetPositions();
            var currentPosition = positions.First(p => p.HasUnit(enemy.gameObject));

            var target = FindClosestTarget(enemy , attack, currentPosition, positions);

            EventManager.RaiseEvent(ObjectEventType.AttackSelected, attack);
            EventManager.RaiseEvent(GameObjectEventType.AttackTargetSelected, target);
        }

        private GameObject FindClosestTarget(EnemyProperties enemy, AttackDetail attack, PositionBehavior currentPosition, List<PositionBehavior> positions)
        {
            var closestPlayers = new List<GameObject>();
            var closestDistance = 20;

            foreach (var position in positions)
            {
                var pathLength = PathFinder.GetPath(currentPosition, position).Count;
                if (!attack.InRange(pathLength))
                {
                    continue;
                }

                var potential = position.GetUnits(UnitSide.Player).ToList();
                if (attack.targetPreference == PreferedTarget.Aggro)
                {
                    potential = potential.Where(a => a.GetComponent<PlayerProperties>().hasAggroToken).ToList();
                }

                if (potential.Count > 0)
                {
                    if (pathLength < closestDistance)
                    {
                        closestPlayers = potential;
                        closestDistance = pathLength;
                    }
                    else if (pathLength == closestDistance)
                    {
                        closestPlayers.AddRange(potential);
                    }
                }
            }

            if(!closestPlayers.Any())
            {
                return null;
            }

            var closestProperties = closestPlayers.Select(p => p.GetComponent<PlayerProperties>()).ToList();
            if(closestProperties.Count > 1)
            {
                var aggro = closestProperties.FirstOrDefault(p => p.hasAggroToken);
                if(aggro != null)
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
