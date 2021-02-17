using Assets.Scripts.Tile;
using Assets.Scripts.Unit.Model.Attacks;
using BoardGame.Script.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Unit.IA
{
    public class EnemyActionResolver : MonoBehaviour
    {
        protected GameStateManager gameState;

        private void Start()
        {
            gameState = FindObjectOfType<GameStateManager>();
        }

        public void Execute(UnitBasicProperties unit, MovementAction movement)
        {
            var tile = gameState.GetActiveTile();
            var positions = tile.GetPositions();
            var currentPosition = positions.First(p => p.HasUnit(unit.gameObject));

            var targetsInRange = movement.FindTargetsInRange(unit, currentPosition, positions);
            var target = FindClosestTarget(currentPosition, movement.TargetPreference, targetsInRange);

            EventManager.RaiseEvent(ObjectEventType.AttackSelected, movement);
            EventManager.RaiseEvent(GameObjectEventType.AttackTargetSelected, target);
        }

        public void Execute(EnemyProperties enemy, AttackAction attack)
        {
            var tile = gameState.GetActiveTile();
            var positions = tile.GetPositions();
            var currentPosition = positions.First(p => p.HasUnit(enemy.gameObject));

            var targetsInRange = attack.FindTargetsInWeaponRange(enemy, currentPosition, positions);
            var target = FindClosestTarget(currentPosition, attack.TargetPreference, targetsInRange);

            EventManager.RaiseEvent(ObjectEventType.AttackSelected, attack);
            EventManager.RaiseEvent(GameObjectEventType.AttackTargetSelected, target);
        }

        private GameObject FindClosestTarget(PositionBehavior currentPosition, PreferedTarget targetPriority, List<UnitBasicProperties> inRangeTargets)
        {
            var tile = gameState.GetActiveTile();
            var positions = tile.GetPositions();

            var closestPlayers = new List<GameObject>();
            var closestDistance = 20;

            foreach (var target in inRangeTargets)
            {
                if (targetPriority == PreferedTarget.Aggro && !target.hasAggroToken)
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
