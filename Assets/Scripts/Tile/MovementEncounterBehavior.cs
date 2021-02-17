using Assets.Scripts.Unit.Model.Attacks;
using BoardGame.Script.Events;
using System;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Tile
{
    public class MovementEncounterBehavior : MonoBehaviour
    {
        private bool resolvingMovement;
        private MovementAction movementRecieved;
        private UnitBasicProperties attackerRecieved;
        private UnitBasicProperties primaryTargetRecieved;

        private MovementStep currentStep;
        private bool checkNextStep;

        private int currentStepCounter;
        private int attackRepeatCounter;


        public void Start()
        {
            EventManager.StartListening(ObjectEventType.EncounterToResolve, Resolve);
        }

        public void Resolve(object eventLoad)
        {
            var castLoad = (Encounter)eventLoad;
            if (castLoad.Action.GetType() != typeof(MovementAction))
            {
                return;
            }

           
            resolvingMovement = true;
            attackRepeatCounter = 1;

            movementRecieved = (MovementAction)castLoad.Action.Clone();
            attackerRecieved = castLoad.Attacker;
            primaryTargetRecieved = castLoad.Defender;
            ApplyFrostbite(movementRecieved, attackerRecieved);

            PrepareEncounter();            
        }

        private void ApplyFrostbite(MovementAction movement, UnitBasicProperties attacker)
        {
            if (attacker.side == UnitSide.Hollow)
            {
                movement.MoveDistance = Math.Max(0, movement.MoveDistance - (attacker.isFrozen ? 1 : 0));
            }
        }

        private void Update()
        {
            if(resolvingMovement)
            {
                if(currentStep == MovementStep.None)
                {
                    currentStep = MovementStep.Movement;
                    checkNextStep = true;
                }
                else if (currentStep == MovementStep.Movement && checkNextStep)
                {
                    if(currentStepCounter < movementRecieved.MoveDistance)
                    {
                        MoveOneStep();
                    }
                    else
                    {
                        currentStep = MovementStep.Resolved;
                    }
                }
                else if(currentStep == MovementStep.Resolved)
                {
                    EventManager.RaiseEvent(ObjectEventType.EncountersResolved);
                    FinalizeEncounter();
                }
            }
        }

        private void PrepareEncounter()
        {
            currentStepCounter = 0;
            currentStep = MovementStep.None;
            checkNextStep = false;
        }

        private void FinalizeEncounter()
        {
            resolvingMovement = false;
            attackRepeatCounter = 1;

            movementRecieved = null;
            attackerRecieved = null;
            primaryTargetRecieved = null;
        }

        private void MoveOneStep()
        {
            checkNextStep = false;
            var attackerPosition = GameStateManager.Instance.GetActiveTile().GetUnitPosition(attackerRecieved.gameObject);
            var targetPosition = GameStateManager.Instance.GetActiveTile().GetUnitPosition(primaryTargetRecieved.gameObject);

            var nodeToMoveTo = DetermineTargetNode(attackerPosition, targetPosition);

            var moveCommand = new UnitMovement()
            {
                MoveFrom = attackerPosition,
                MoveTo = nodeToMoveTo,
                Unit = attackerRecieved.gameObject,
            };
            
            EventManager.RaiseEvent(ObjectEventType.UnitMoved, moveCommand);

            currentStepCounter++;
            checkNextStep = true;
        }

        private PositionBehavior DetermineTargetNode(PositionBehavior from, PositionBehavior to)
        {
            PositionBehavior bestNode;
            var distances = PathFinder.GetNodeDistances(to, from.GetAdjacentNodes());

            if (movementRecieved.Direction == MovementDirection.Forward)
            {
                bestNode = distances.OrderBy(e => e.Lengh).First().Node;
            }
            else if (movementRecieved.Direction == MovementDirection.Backward)
            {
                bestNode = distances.OrderByDescending(e => e.Lengh).First().Node;
            }
            else
            {
                throw new NotImplementedException();
            }

            return bestNode;
        }

        private enum MovementStep
        {
            None,
            Movement,
            Resolved,
        }
    }
}
