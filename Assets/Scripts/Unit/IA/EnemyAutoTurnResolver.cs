using Assets.Scripts.Unit.Model.Attacks;
using BoardGame.Script.Events;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Unit.IA
{
    public class EnemyAutoTurnResolver : MonoBehaviour
    {

        private EnemyActionResolver attackResolver;

        private EnemyProperties enemy;
        private List<BehaviorAction> attackstoExecute;
        private bool doNextAction;

        void Start()
        {
            attackstoExecute = new List<BehaviorAction>();
            attackResolver = FindObjectOfType<EnemyActionResolver>();
            doNextAction = false;

            EventManager.StartListening(ObjectEventType.AttackApplied, AttackApplied);
        }

        void Update()
        {

            if (enemy != null)
            {
                if (attackstoExecute.Count == 0)
                {
                    enemy = null;
                    attackstoExecute.Clear();
                    EventManager.RaiseEvent(GameObjectEventType.EndUnitTurn);
                }
                else if(doNextAction)
                {
                    doNextAction = false;
                    LaunchAttack();
                }
            }
        }

        public void ResolveEnemyTurn(GameObject enemyObject)
        {
            attackstoExecute = new List<BehaviorAction>();
            enemy = enemyObject.GetComponent<EnemyProperties>();

            if (enemy.leftEquipement != null)
            {
                var t = enemy.leftEquipement.GetComponent<EquipementProperties>();
                attackstoExecute.AddRange(enemy.leftEquipement.GetComponent<EquipementProperties>().attackList);
            }
            if (enemy.rightEquipement != null)
            {
                attackstoExecute.AddRange(enemy.rightEquipement.GetComponent<EquipementProperties>().attackList);
            }
            if (enemy.armourEquipement != null)
            {
                attackstoExecute.AddRange(enemy.armourEquipement.GetComponent<EquipementProperties>().attackList);
            }

            doNextAction = true;
        }

        private bool LaunchAttack()
        {
            if (!attackstoExecute.Any())
            {
                EventManager.RaiseEvent(GameObjectEventType.EndUnitTurn);
                return false;
            }

            var nextAttack = attackstoExecute.First();
            if (nextAttack is AttackAction)
            {
                attackResolver.Execute(enemy, (AttackAction)nextAttack);
            }
            else if (nextAttack is MovementAction)
            {
                attackResolver.Execute(enemy, (MovementAction)nextAttack);
            }
            return true;
        }

        private void AttackApplied(object _)
        {
            if (enemy != null && attackstoExecute.Count > 0)
            {
                attackstoExecute.RemoveAt(0);
                doNextAction = true;
            }
        }
    }
}