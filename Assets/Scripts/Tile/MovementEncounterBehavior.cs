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
        #region Properties
        public GameObject throwPoint;
        public GameObject blackDiceTemplate;
        public GameObject blueDiceTemplate;
        public GameObject orangeDiceTemplate;
        public GameObject dodgeDiceTemplate;

        private Vector3 anchorOffset;
        private Vector3 offsetDiceResultPresentation;

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

        public GameObject attackPushContainer;
        public TextMesh   attackPushText;
        public GameObject attackDodgeDiceContainer;
        public TextMesh   attackDodgeDiceText;

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

        private List<GameObject> dices;
        private int diceResultRecieved;
        private EncounterRollType rollType;
        private int blockRoll;
        private int dodgeRoll;

        private List<UnitBasicProperties> unitToPushRemaining;
        private UnitBasicProperties currentDefender;
        private DefenseDices currentDefense;
        private PositionBehavior attackerOriginNode;
        #endregion

        public void Start()
        {
            dices = new List<GameObject>();
            anchorOffset = new Vector3(1.35f, 0f, 0f);
            offsetDiceResultPresentation = new Vector3(2, 0, 0);

            SetGlobalPanelVisibility(false);

            blockButton.GetComponent<RaiseEventOnClicked>().PositionClicked += BlockSelected;
            blockButton.GetComponent<RaiseEventOnEnterExit>().PositionEnter += BlockHovered;
            blockButton.GetComponent<RaiseEventOnEnterExit>().PositionExit += BlockHoverEnded;

            dodgeButton.GetComponent<RaiseEventOnClicked>().PositionClicked += DodgeSelected;
            dodgeButton.GetComponent<RaiseEventOnEnterExit>().PositionEnter += DodgeHovered;
            dodgeButton.GetComponent<RaiseEventOnEnterExit>().PositionExit += DodgeHoverEnded;

            defenseLuckBehavior.GetComponent<RaiseEventOnClicked>().PositionClicked += UseLuckEvent;
            confirmButton.transform.Find("BackgroundButton").GetComponent<RaiseEventOnClicked>().PositionClicked += ConfirmResult;

            pushMover.PositionClicked += PushMoveSelected;
            dodgeMover.PositionClicked += DodgeMoveSelected;
            
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
            currentStepCounter = 0;

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
                        PrepareMove();
                    }
                    else if (currentDefender == null)
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
            dodgeRoll = 0;
            blockRoll = 0;
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

            movementRecieved = null;
            attackerRecieved = null;
            primaryTargetRecieved = null;

            SetGlobalPanelVisibility(false);
        }

        private void PrepareInitialPush()
        {
            currentStep = MovementStep.InitialPush;
            checkNextStep = false;
            DestroyDices();

            if (!movementRecieved.Push)
            {
                FinishCurrentStep();
                return;
            }

            SetInitialPushVisibility();
            unitToPushRemaining = GetUnitsToPush();
            currentDefender = null;
            attackerOriginNode = null;
        }

        private void ShowNextInitialPush()
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

            SetDefenderPanel();
            SetAttackerPanel();

            pushMover.SetupAndShow(currentDefender, MoveChoserType.Push, attackerRecieved);
        }

        private void PrepareMove()
        {
            currentStep = MovementStep.Movement;
            checkNextStep = false;
            dodgeMover.Hide();
            pushMover.Hide();
            DestroyDices();

            var attackerPosition = GameStateManager.Instance.GetActiveTile().GetUnitPosition(attackerRecieved.gameObject);
            var targetPosition = GameStateManager.Instance.GetActiveTile().GetUnitPosition(primaryTargetRecieved.gameObject);

            attackerOriginNode = attackerPosition;

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

            SetPushAwayVisibility();
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

            SetDefenderPanel();
            SetAttackerPanel();

            defenderPortraitRenderer.material = currentDefender.portrait;
            attackerPortraitRenderer.material = attackerRecieved.portrait;

            if (movementRecieved.PushDamage > 0)
            {
                ShowNextPushAwayBlockDodge();
            }
            else
            {
                ShowNextPushAwayPush();
            }
        }

        private void ShowNextPushAwayBlockDodge()
        {
            DestroyDices();

            dodgeMover.Hide();
            pushMover.Hide();
            SetAttackerPanel();
            SetDefenderPanel();

            dodgeButton.SetActive(true);
            blockButton.SetActive(true);
        }

        private void ShowNextPushAwayPush()
        {
            DestroyDices();
            pushMover.SetupAndShow(currentDefender, MoveChoserType.Push, attackerOriginNode);
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
            if (currentStep == MovementStep.InitialPush)
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

        #region Defense Select
        private void UseLuckEvent(GameObject position)
        {
            var unit = (PlayerProperties)currentDefender;
            if (unit.hasLuckToken)
            {
                foreach (var dice in dices)
                {
                    dice.GetComponent<RaiseEventOnClicked>().PositionClicked += DiceSelectedForRethrow;
                }
                unit.hasLuckToken = false;
                SetConfirmButtonVisibility(false);
            }
        }

        private void DiceSelectedForRethrow(GameObject dice)
        {
            foreach (var alldice in dices)
            {
                alldice.GetComponent<RaiseEventOnClicked>().PositionClicked -= DiceSelectedForRethrow;
            }

            diceResultRecieved--;
            ThrowOneDice(dice);
            EventManager.StartListening(GameObjectEventType.DiceStoppedMoving, AddDiceResult);
        }

        private void DodgeHoverEnded(GameObject position)
        {
            dodgeButtonRenderer.enabled = false;
        }

        private void DodgeHovered(GameObject position)
        {
            dodgeButtonRenderer.enabled = true;
            SpawnDices(0, 0, 0, currentDefense.DodgeDices);
        }

        private void DodgeSelected(GameObject position)
        {
            dodgeMover.SetupAndShow(currentDefender, MoveChoserType.Dodge);
        }

        private void DodgeMoveSelected(PositionBehavior _)
        {
            dodgeMover.Hide();
            blockButton.SetActive(false);
            dodgeButton.SetActive(false);
            rollType = EncounterRollType.Dodge;
            diceResultRecieved = 0;
            ThrowDices();
        }

        private void BlockHoverEnded(GameObject position)
        {
            blockButtonRenderer.enabled = false;
        }

        private void BlockHovered(GameObject position)
        {
            blockButtonRenderer.enabled = true;
            SpawnDices(currentDefense.BlackDices, currentDefense.BlueDices, currentDefense.OrangeDices, 0);
        }

        private void BlockSelected(GameObject position)
        {
            blockButton.SetActive(false);
            dodgeButton.SetActive(false);
            rollType = EncounterRollType.Block;
            diceResultRecieved = 0;
            ThrowDices();
        }
        #endregion

        #region Dices
        private void SpawnDices(int blackDicesCount, int blueDicesCount, int orangeDicesCount, int dodgeDicesCount)
        {
            foreach (var dice in dices)
            {
                Destroy(dice);
            }
            dices.Clear();

            for (int i = 0; i < blackDicesCount; i++)
            {
                dices.Add(SpawnDice(blackDiceTemplate));
            }
            for (int i = 0; i < blueDicesCount; i++)
            {
                dices.Add(SpawnDice(blueDiceTemplate));
            }
            for (int i = 0; i < orangeDicesCount; i++)
            {
                dices.Add(SpawnDice(orangeDiceTemplate));
            }
            for (int i = 0; i < dodgeDicesCount; i++)
            {
                dices.Add(SpawnDice(dodgeDiceTemplate));
            }

        }

        private GameObject SpawnDice(GameObject diceTemplate)
        {
            var dice = Instantiate(diceTemplate);
            var offsetPoint = new Vector3(UnityEngine.Random.Range(-1.5f, 1.5f), 0, 0);
            dice.transform.position = throwPoint.transform.position + offsetPoint;
            dice.transform.localRotation = Quaternion.Euler(UnityEngine.Random.Range(0, 90), UnityEngine.Random.Range(0, 90), UnityEngine.Random.Range(0, 90));

            return dice;
        }

        private void ThrowDices()
        {
            EventManager.StartListening(GameObjectEventType.DiceStoppedMoving, AddDiceResult);
            foreach (var dice in dices)
            {
                ThrowOneDice(dice);
            }
        }

        private void ThrowOneDice(GameObject dice)
        {
            var behavior = dice.GetComponent<DiceBahevior>();
            behavior.ResetForThrow();
            var body = dice.GetComponent<Rigidbody>();
            body.isKinematic = false;
            dice.GetComponent<Rigidbody>().AddForceAtPosition(new Vector3(0, UnityEngine.Random.Range(1f, 1.5f), UnityEngine.Random.Range(1.5f, 2f)), dice.transform.position + new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(0, 0.5f), UnityEngine.Random.Range(-0.5f, 0.5f)), ForceMode.Impulse);
        }

        private void DestroyDices()
        {
            foreach(var dice in dices.ToList())
            {
                Destroy(dice);
            }
            dices.Clear();
        }

        public void AddDiceResult(GameObject diceResult)
        {
            var body = diceResult.GetComponent<Rigidbody>().isKinematic = true;
            diceResultRecieved++;

            if (diceResultRecieved == dices.Count)
            {
                EventManager.StopListening(GameObjectEventType.DiceStoppedMoving, AddDiceResult);
                if (rollType == EncounterRollType.Block)
                {
                    blockRoll = currentDefense.FlatReduce;
                }
                else if (rollType == EncounterRollType.Dodge)
                {
                    dodgeRoll = 0;
                }

                foreach (var dice in dices)
                {
                    var behavior = dice.GetComponent<DiceBahevior>();
                    if (rollType == EncounterRollType.Block)
                    {
                        blockRoll += behavior.GetValue();
                    }
                    else if (rollType == EncounterRollType.Dodge)
                    {
                        dodgeRoll += behavior.GetValue();
                    }
                }

                int i = 0;
                foreach (var dice in dices)
                {
                    dice.transform.position = throwPoint.transform.position + offsetDiceResultPresentation * i;
                    i++;
                }

                if (CanAutoConfirmResult())
                {
                    Invoke(nameof(ApplyPushDamage), 2f);
                }
                else
                {
                    SetConfirmButtonVisibility(true);
                }
            }
        }

        private bool CanAutoConfirmResult()
        {
            var canAutoResolve = true;
            if (currentStep == MovementStep.PushAway && currentDefender is PlayerProperties)
            {
                var properties = (PlayerProperties)currentDefender;
                if ((properties.isActive && properties.hasEstus) || properties.hasLuckToken)
                    canAutoResolve = false;
            }

            return canAutoResolve;
        }

        private void SetConfirmButtonVisibility(bool visibility)
        {
            confirmButton.SetActive(visibility);
        }

        private void ConfirmResult(GameObject position)
        {
            if (currentStep == MovementStep.PushAway && currentDefender is PlayerProperties)
            {
                ApplyPushDamage();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private void ApplyPushDamage()
        {
            var damageToApply = movementRecieved.PushDamage;
            var hit = true;

            if (rollType == EncounterRollType.Block)
            {
                damageToApply = Math.Max(0, damageToApply - blockRoll);
            }
            else if (rollType == EncounterRollType.Dodge)
            {
                if (dodgeRoll >= movementRecieved.DodgeLevel)
                {
                    damageToApply = 0;
                    hit = false;
                }
                currentDefender.ConsumeStamina(1 + (currentDefender.isFrozen ? 1 : 0));
            }

            currentDefender.RecieveInjuries(damageToApply);

            DestroyDices();
            confirmButton.SetActive(false);
            if (hit)
            {
                ShowNextPushAwayPush();
            }
            else
            {
                currentDefender = null;
            }
        }
        #endregion

        #region Visibility
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

            attackPushContainer.SetActive(visibility);
            attackDodgeDiceContainer.SetActive(visibility);

            blockButton.SetActive(visibility);
            dodgeButton.SetActive(visibility);

            confirmButton.SetActive(visibility);
            dodgeMover.Hide();
            pushMover.Hide();
        }

        private void SetInitialPushVisibility()
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

            attackPushContainer.SetActive(true);
            attackDodgeDiceContainer.SetActive(true);

            blockButton.SetActive(false);
            dodgeButton.SetActive(false);

            confirmButton.SetActive(false);
        }

        private void SetPushAwayVisibility()
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

            defenseLuckBehavior.gameObject.SetActive(true);
            defenseEmberBehavior.gameObject.SetActive(true);
            defenseEstusBehavior.gameObject.SetActive(true);

            attackPushContainer.SetActive(true);
            attackDodgeDiceContainer.SetActive(true);

            blockButton.SetActive(true);
            dodgeButton.SetActive(true);

            confirmButton.SetActive(false);
        }

        private void SetDefenderPanel()
        {
            var index = 0;
            currentDefense = currentDefender.GetDefenseDices(movementRecieved.MagicAttack);

            PlaceAndDisplayModifier(defenseBlackDiceContainer, defenseBlackDiceText, currentDefense.BlackDices, defenseAnchor, anchorOffset, false, ref index);
            PlaceAndDisplayModifier(defenseBlueDiceContainer, defenseBlueDiceText, currentDefense.BlueDices, defenseAnchor, anchorOffset, false, ref index);
            PlaceAndDisplayModifier(defenseOrangeDiceContainer, defenseOrangeDiceText, currentDefense.OrangeDices, defenseAnchor, anchorOffset, false, ref index);
            PlaceAndDisplayModifier(defenseFlatModifierContainer, defenseFlatModifierText, currentDefense.FlatReduce, defenseAnchor, anchorOffset, index == 0 ? true : false, ref index);

            PlaceAndDisplayModifier(defenseDodgeDiceContainer, defenseDodgeDiceText, currentDefense.DodgeDices, defenseDodgeDiceContainer, Vector3.zero, true, ref index);

            defenseBleedToken.SetActive(currentDefender.isBleeding);
            defensePoisonToken.SetActive(currentDefender.isPoisoned);
            defenseStaggerToken.SetActive(currentDefender.isStaggered);
            defenseFrozenToken.SetActive(currentDefender.isFrozen);

            defenseLuckBehavior.SetUnit(currentDefender);
            defenseEmberBehavior.SetUnit(currentDefender);
            defenseEstusBehavior.SetUnit(currentDefender);

            dodgeButton.SetActive(false);
            blockButton.SetActive(false);
        }

        private void SetAttackerPanel()
        {
            var index = 0;
            PlaceAndDisplayModifier(attackPushContainer, attackPushText, movementRecieved.PushDamage, attackPushContainer, Vector3.zero, false, ref index);
            PlaceAndDisplayModifier(attackDodgeDiceContainer, attackDodgeDiceText, movementRecieved.DodgeLevel, attackDodgeDiceContainer, Vector3.zero, true, ref index);

            attackBleedToken.SetActive(attackerRecieved.isBleeding);
            attackPoisonToken.SetActive(attackerRecieved.isPoisoned);
            attackStaggerToken.SetActive(attackerRecieved.isStaggered);
            attackFrozenToken.SetActive(attackerRecieved.isFrozen);
        }

        private void PlaceAndDisplayModifier(GameObject container, TextMesh text, int value, GameObject anchor, Vector3 offset, bool showDefaultValue, ref int offsetIndex, int defaultValue = 0)
        {
            if (value == defaultValue && !showDefaultValue)
            {
                container.SetActive(false);
            }
            else
            {
                container.SetActive(true);
                container.transform.localPosition = anchor.transform.localPosition + (offsetIndex * offset);
                text.text = value.ToString();
                offsetIndex++;
            }
        }
        #endregion

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
