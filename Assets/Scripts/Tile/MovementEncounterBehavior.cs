using Assets.Scripts.Unit.Model.Attacks;
using BoardGame.Script.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Tile
{
    public class MovementEncounterBehavior : MonoBehaviour
    {
        public GameObject attackerPortrait;
        public GameObject defenderPortrait;
        public GameObject attackOptions;
        public GameObject defenseOptions;
        public GameObject attackIcon;
        public GameObject defenseIcon;

        public MeshRenderer defenderPortraitRenderer;
        public MeshRenderer attackerPortraitRenderer;

        public GameObject defenseBlackDiceContainer;
        public TextMesh defenseBlackDiceText;
        public GameObject defenseBlueDiceContainer;
        public TextMesh defenseBlueDiceText;
        public GameObject defenseOrangeDiceContainer;
        public TextMesh defenseOrangeDiceText;
        public GameObject defenseFlatModifierContainer;
        public TextMesh defenseFlatModifierText;
        public GameObject defenseDodgeDiceContainer;
        public TextMesh defenseDodgeDiceText;
        public GameObject defenseAnchor;

        public GameObject attackBleedToken;
        public GameObject attackPoisonToken;
        public GameObject attackStaggerToken;
        public GameObject attackFrozenToken;

        public GameObject defenseBleedToken;
        public GameObject defensePoisonToken;
        public GameObject defenseStaggerToken;
        public GameObject defenseFrozenToken;

        public TokenBehavior defenseEstusBehavior;
        public TokenBehavior defenseLuckBehavior;
        public TokenBehavior defenseEmberBehavior;

        public GameObject pushContainer;
        public TextMesh pushText;
        public GameObject pushDodgeContainer;
        public TextMesh pushDodgeText;

        public GameObject blockButton;
        public GameObject dodgeButton;
        public MeshRenderer blockButtonRenderer;
        public MeshRenderer dodgeButtonRenderer;

        public GameObject confirmButton;
        public MoveChoserBehavior dodgeMover;
        public MoveChoserBehavior pushMover;

        private bool resolvingMovement;
        private MovementAction movementRecieved;
        private UnitBasicProperties attackerRecieved;
        private UnitBasicProperties primaryTargetRecieved;

        private MovementStep currentStep;
        private bool checkNextStep;

        private int currentStepCounter;
        private int attackRepeatCounter;

        private List<UnitBasicProperties> unitToPushRemaining;
        private UnitBasicProperties currentDefender;

        public void Start()
        {
            SetGlobalPanelVisibility(false);
            EventManager.StartListening(ObjectEventType.EncounterToResolve, Resolve);

            pushMover.PositionClicked += PushMoveSelected;
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
            if (resolvingMovement)
            {
                if (currentStep == MovementStep.None)
                {
                    PrepareInitialPush();
                }
                else if (currentStep == MovementStep.InitialPush)
                {
                    if (checkNextStep)
                    {
                        MoveOneStep();
                    }
                    else if(currentDefender == null)
                    {
                        ShowNextInitialPush();
                    }
                }
                else if (currentStep == MovementStep.Movement && checkNextStep)
                {
                    PreparePushAway();
                }
                else if (currentStep == MovementStep.PushAway)
                {
                    if (checkNextStep)
                    {
                        currentStepCounter++;
                        currentStep = MovementStep.Resolved;
                    }
                    else if (currentDefender == null)
                    {
                        ShowNextPushAway();
                    }
                }
                else if (currentStep == MovementStep.Resolved)
                {
                    if (currentStepCounter < movementRecieved.MoveDistance)
                    {
                        currentStep = MovementStep.None;
                    }
                    else
                    {
                        EventManager.RaiseEvent(ObjectEventType.EncountersResolved);
                        FinalizeEncounter();
                    }
                }
            }
        }

        private void PrepareEncounter()
        {
            currentStepCounter = 0;
            currentStep = MovementStep.None;
            checkNextStep = false;

            if (currentStepCounter == movementRecieved.MoveDistance)
            {
                currentStep = MovementStep.Resolved;
            }
        }

        private void FinalizeEncounter()
        {
            resolvingMovement = false;
            attackRepeatCounter = 1;

            movementRecieved = null;
            attackerRecieved = null;
            primaryTargetRecieved = null;

            SetGlobalPanelVisibility(false);
        }

        private void PrepareInitialPush()
        {
            currentStep = MovementStep.InitialPush;
            checkNextStep = false;

            if (!movementRecieved.Push)
            {
                FinishCurrentStep();
                return;
            }

            SetPushVisibility();
            unitToPushRemaining = GetUnitsToPush();
            currentDefender = null;
        }

        private void ShowNextInitialPush()
        {
            if(unitToPushRemaining.Count == 0)
            {
                FinishCurrentStep();
                return;
            }

            currentDefender = unitToPushRemaining[0];
            unitToPushRemaining.RemoveAt(0);

            defenderPortraitRenderer.material = currentDefender.portrait;
            attackerPortraitRenderer.material = attackerRecieved.portrait;

            SetDefenderStatusToken();
            SetAttackerStatusToken();

            pushMover.SetupAndShow(currentDefender, MoveChoserType.Push, attackerRecieved);
        }

        private void MoveOneStep()
        {
            currentStep = MovementStep.Movement;
            checkNextStep = false;

            var attackerPosition = GameStateManager.Instance.GetActiveTile().GetUnitPosition(attackerRecieved.gameObject);
            var targetPosition = GameStateManager.Instance.GetActiveTile().GetUnitPosition(primaryTargetRecieved.gameObject);

            var nodeToMoveTo = DetermineTargetNode(attackerPosition, targetPosition);
            if (nodeToMoveTo != null)
            {
                var moveCommand = new UnitMovement()
                {
                    MoveFrom = attackerPosition,
                    MoveTo = nodeToMoveTo,
                    Unit = attackerRecieved.gameObject,
                };

                EventManager.RaiseEvent(ObjectEventType.UnitMoved, moveCommand);
            }

            currentStepCounter++;
            FinishCurrentStep();
        }

        private void PreparePushAway()
        {
            currentStep = MovementStep.PushAway;
            checkNextStep = false;

            if (!movementRecieved.Push)
            {
                FinishCurrentStep();
                return;
            }

            SetPushVisibility();
            unitToPushRemaining = GetUnitsToPush();
            currentDefender = null;
        }

        private void ShowNextPushAway()
        {
            if (unitToPushRemaining.Count == 0)
            {
                FinishCurrentStep();
                return;
            }

            currentDefender = unitToPushRemaining[0];
            unitToPushRemaining.RemoveAt(0);

            defenderPortraitRenderer.material = currentDefender.portrait;
            attackerPortraitRenderer.material = attackerRecieved.portrait;

            pushMover.SetupAndShow(currentDefender, MoveChoserType.Push, attackerRecieved);
        }

        private List<UnitBasicProperties> GetUnitsToPush()
        {
            var attackerPosition = GameStateManager.Instance.GetActiveTile().GetUnitPosition(attackerRecieved.gameObject);
            return attackerPosition.GetUnits(attackerRecieved.side == UnitSide.Hollow ? UnitSide.Player : UnitSide.Hollow).Select(e => e.GetComponent<UnitBasicProperties>()).ToList();
        }

      
        private PositionBehavior DetermineTargetNode(PositionBehavior from, PositionBehavior to)
        {
            var currentPathLength = PathFinder.NodeWorldDistance(from, to);

            PositionBehavior bestNode;
            var distances = PathFinder.GetNodeDistances(to, from.GetAdjacentNodes());

            

            if (movementRecieved.Direction == MovementDirection.Forward)
            {
                bestNode = distances.OrderBy(e => e.Lengh).First().Node;
                if (PathFinder.NodeWorldDistance(bestNode, to) > PathFinder.NodeWorldDistance(from, to))
                {
                    return null;
                }
            }
            else if (movementRecieved.Direction == MovementDirection.Backward)
            {
                bestNode = distances.OrderByDescending(e => e.Lengh).First().Node;
                if (PathFinder.NodeWorldDistance(bestNode, to) < PathFinder.NodeWorldDistance(from, to))
                {
                    return null;
                }
            }
            else
            {
                throw new NotImplementedException();
            }

            

            return bestNode;
        }

        private void PushMoveSelected(PositionBehavior position)
        {
            if(currentStep == MovementStep.InitialPush)
            {
                currentDefender = null;
            }
            else if (currentStep == MovementStep.PushAway)
            {
                currentDefender = null;
            }
        }

        private void FinishCurrentStep()
        {
            checkNextStep = true;
        }

        private void SetGlobalPanelVisibility(bool visibility)
        {
            attackerPortrait.SetActive(visibility);
            defenderPortrait.SetActive(visibility);
            attackOptions.SetActive(visibility);
            defenseOptions.SetActive(visibility);
            attackIcon.SetActive(visibility);
            defenseIcon.SetActive(visibility);

            defenseBlackDiceContainer.SetActive(visibility);
            defenseBlueDiceContainer.SetActive(visibility);
            defenseOrangeDiceContainer.SetActive(visibility);
            defenseFlatModifierContainer.SetActive(visibility);
            defenseDodgeDiceContainer.SetActive(visibility);
            defenseAnchor.SetActive(visibility);

            pushContainer.SetActive(visibility);
            pushDodgeContainer.SetActive(visibility);

            blockButton.SetActive(visibility);
            dodgeButton.SetActive(visibility);

            confirmButton.SetActive(visibility);
            dodgeMover.Hide();
            pushMover.Hide();
        }

        private void SetPushVisibility()
        {
            attackerPortrait.SetActive(true);
            defenderPortrait.SetActive(true);
            attackOptions.SetActive(true);
            defenseOptions.SetActive(true);
            attackIcon.SetActive(true);
            defenseIcon.SetActive(true);

            defenseBlackDiceContainer.SetActive(false);
            defenseBlueDiceContainer.SetActive(false);
            defenseOrangeDiceContainer.SetActive(false);
            defenseFlatModifierContainer.SetActive(false);
            defenseDodgeDiceContainer.SetActive(false);
            defenseAnchor.SetActive(false);

            defenseLuckBehavior.gameObject.SetActive(false);
            defenseEmberBehavior.gameObject.SetActive(false);
            defenseEstusBehavior.gameObject.SetActive(false);

            pushContainer.SetActive(true);
            pushDodgeContainer.SetActive(false);

            blockButton.SetActive(false);
            dodgeButton.SetActive(false);

            confirmButton.SetActive(false);
        }

        private void SetPushDamageVisibility()
        {
            attackerPortrait.SetActive(true);
            defenderPortrait.SetActive(true);
            attackOptions.SetActive(true);
            defenseOptions.SetActive(true);
            attackIcon.SetActive(true);
            defenseIcon.SetActive(true);

            defenseBlackDiceContainer.SetActive(true);
            defenseBlueDiceContainer.SetActive(true);
            defenseOrangeDiceContainer.SetActive(true);
            defenseFlatModifierContainer.SetActive(true);
            defenseDodgeDiceContainer.SetActive(true);
            defenseAnchor.SetActive(true);

            pushContainer.SetActive(true);
            pushDodgeContainer.SetActive(true);

            blockButton.SetActive(true);
            dodgeButton.SetActive(true);

            confirmButton.SetActive(false);
        }

        private void SetDefenderStatusToken()
        {
            defenseBleedToken.SetActive(currentDefender.isBleeding);
            defensePoisonToken.SetActive(currentDefender.isPoisoned);
            defenseStaggerToken.SetActive(currentDefender.isStaggered);
            defenseFrozenToken.SetActive(currentDefender.isFrozen);
        }

        private void SetAttackerStatusToken()
        {
            defenseBleedToken.SetActive(attackerRecieved.isBleeding);
            defensePoisonToken.SetActive(attackerRecieved.isPoisoned);
            defenseStaggerToken.SetActive(attackerRecieved.isStaggered);
            defenseFrozenToken.SetActive(attackerRecieved.isFrozen);
        }

        private enum MovementStep
        {
            None,
            InitialPush,
            Movement,
            PushAway,
            Resolved,
        }
    }
}
