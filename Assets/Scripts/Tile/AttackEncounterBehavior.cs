using Assets.Scripts.Unit.Model.Attacks;
using BoardGame.Script.Events;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Tile
{
    public class AttackEncounterBehavior : MonoBehaviour
    {
        public GameObject blackDiceTemplate;
        public GameObject blueDiceTemplate;
        public GameObject orangeDiceTemplate;
        public GameObject dodgeDiceTemplate;

        public GameObject throwPoint;
        public GameObject attackerPortrait;
        public GameObject defenderPortrait;
        public GameObject attackOptions;
        public GameObject defenseOptions;
        public GameObject attackIcon;
        public GameObject defenseIcon;

        public MeshRenderer defenderPortraitRenderer;
        public MeshRenderer attackerPortraitRenderer;

        public GameObject attackBlackDiceContainer;
        public TextMesh attackBlackDiceText;
        public GameObject attackBlueDiceContainer;
        public TextMesh attackBlueDiceText;
        public GameObject attackOrangeDiceContainer;
        public TextMesh attackOrangeDiceText;
        public GameObject attackFlatModifierContainer;
        public TextMesh attackFlatModifierText;
        public GameObject attackDodgeDiceContainer;
        public TextMesh attackDodgeDiceText;
        public GameObject attackRepeatContainer;
        public TextMesh attackRepeatText;
        public GameObject attackAnchor;

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

        public GameObject blockButton;
        public GameObject dodgeButton;
        public GameObject attackButton;
        public MeshRenderer blockButtonRenderer;
        public MeshRenderer dodgeButtonRenderer;
        public MeshRenderer attackButtonRenderer;

        public TokenBehavior attackEstusBehavior;
        public TokenBehavior attackLuckBehavior;
        public TokenBehavior defenseEstusBehavior;
        public TokenBehavior defenseLuckBehavior;
        public TokenBehavior defenseEmberBehavior;

        public GameObject confirmButton;
        public MoveChoserBehavior dodgeMover;
        public MoveChoserBehavior pushMover;
        public MoveChoserBehavior shiftMover;

        public GameObject attackBleedToken;
        public GameObject attackPoisonToken;
        public GameObject attackStaggerToken;
        public GameObject attackFrozenToken;
        public GameObject attackPushToken;

        public GameObject defenseBleedToken;
        public GameObject defensePoisonToken;
        public GameObject defenseStaggerToken;
        public GameObject defenseFrozenToken;

        private List<GameObject> dices;
        private int diceResultRecieved;
        private EncounterRollType rollType;

        private Vector3 anchorOffset;
        private Vector3 offsetDiceResultPresentation;

        private AttackAction attackRecieved;
        private UnitBasicProperties attackerRecieved;
        private UnitBasicProperties defenderRecieved;

        private List<UnitBasicProperties> defensesToResolve;
        private bool resolvingEncounter = false;

        private DefenseDices defenseCurrent;
        private UnitBasicProperties defenderCurrent;
        private int attackRepeatCounter;
        private int damageRoll;
        private int blockRoll;
        private int dodgeRoll;
        private int shiftBeforeResolved;
        private int shiftAfterResolved;
        private bool attackResolved = false;
        private bool defenseResolved = false;
        private EncounterStep currentStep;

        void Start()
        {
            defensesToResolve = new List<UnitBasicProperties>();
            dices = new List<GameObject>();

            anchorOffset = new Vector3(1.35f, 0f, 0f);
            offsetDiceResultPresentation = new Vector3(2, 0, 0);

            attackLuckBehavior.gameObject.GetComponent<RaiseEventOnClicked>().PositionClicked += UseLuckEvent;
            defenseLuckBehavior.GetComponent<RaiseEventOnClicked>().PositionClicked += UseLuckEvent;

            blockButton.GetComponent<RaiseEventOnClicked>().PositionClicked += BlockSelected;
            blockButton.GetComponent<RaiseEventOnEnterExit>().PositionEnter += BlockHovered;
            blockButton.GetComponent<RaiseEventOnEnterExit>().PositionExit += BlockHoverEnded;

            dodgeButton.GetComponent<RaiseEventOnClicked>().PositionClicked += DodgeSelected;
            dodgeButton.GetComponent<RaiseEventOnEnterExit>().PositionEnter += DodgeHovered;
            dodgeButton.GetComponent<RaiseEventOnEnterExit>().PositionExit += DodgeHoverEnded;

            attackButton.GetComponent<RaiseEventOnClicked>().PositionClicked += AttackSelected;
            attackButton.GetComponent<RaiseEventOnEnterExit>().PositionEnter += AttackHovered;
            attackButton.GetComponent<RaiseEventOnEnterExit>().PositionExit += AttackHoverEnded;

            confirmButton.transform.Find("BackgroundButton").GetComponent<RaiseEventOnClicked>().PositionClicked += ConfirmResult;
            dodgeMover.PositionClicked += DodgeMoveSelected;
            pushMover.PositionClicked += PushMoveSelected;
            shiftMover.PositionClicked += ShiftMoveSelected;

            EventManager.StartListening(ObjectEventType.EncounterToResolve, Resolve);

            SetGlobalVisibility(false);
        }

        public void Resolve(object eventLoad)
        {
            var castLoad = (Encounter)eventLoad;
            if (castLoad.Action.GetType() != typeof(AttackAction))
            {
                return;
            }

            resolvingEncounter = true;

            attackRecieved = (AttackAction)castLoad.Action.Clone();
            attackerRecieved = castLoad.Attacker;
            defenderRecieved = castLoad.Defender;

            ApplyStagger(attackRecieved, attackerRecieved);
            CloneRecievedToCurrent();
            currentStep = EncounterStep.None;
            attackRepeatCounter = 1;
        }

        void Update()
        {
            if (resolvingEncounter)
            {
                if (currentStep == EncounterStep.None)
                {
                    ResetShowAttacker();
                    currentStep = EncounterStep.ShiftBefore;
                    SetupForShift();

                }
                else if (currentStep == EncounterStep.ShiftBefore && shiftBeforeResolved == attackRecieved.ShiftBefore)
                {

                    currentStep = EncounterStep.Attack;
                    SetupForAttack();
                }
                else if (currentStep == EncounterStep.Attack && attackResolved)
                {
                    currentStep = EncounterStep.Defense;
                    defenseResolved = false;
                    FindAllPotentialTargets();
                    ClearCurrent();
                }
                else if (currentStep == EncounterStep.Defense)
                {
                    if (defenseResolved) // all defenses resolved for this same attack.
                    {
                        currentStep = EncounterStep.ShiftAfter;
                        SetupForShift();
                    }
                    else if (IsCurrentCleared()) // load next defender against same attack.
                    {
                        SetupForDefender();
                    }
                }
                else if (currentStep == EncounterStep.ShiftAfter && shiftAfterResolved == attackRecieved.ShiftAfter)
                {
                    if (attackRepeatCounter < attackRecieved.Repeat)
                    {
                        attackRepeatCounter++;
                        currentStep = EncounterStep.None;
                        CloneRecievedToCurrent();
                        SetupForAttack();
                    }
                    else
                    {
                        currentStep = EncounterStep.Resolved;
                    }

                }
                else if (currentStep == EncounterStep.Resolved)
                {
                    attackerRecieved.ConsumeStamina(attackRecieved.StaminaCost);
                    EventManager.RaiseEvent(ObjectEventType.EncountersResolved);
                    SetGlobalVisibility(false);
                    resolvingEncounter = false;
                }
            }
        }



        private void SetupForShift()
        {
            if (currentStep == EncounterStep.ShiftBefore && shiftBeforeResolved >= attackRecieved.ShiftBefore)
            {
                return;
            }
            if (currentStep == EncounterStep.ShiftAfter && shiftAfterResolved >= attackRecieved.ShiftAfter)
            {
                return;
            }
            HideButtonAndMoverAndDices();
            shiftMover.SetupAndShow(attackerRecieved, MoveChoserType.Shift);
        }

        private void SetupForAttack()
        {
            damageRoll = 0;
            HideButtonAndMoverAndDices();
            if (attackerRecieved.side == UnitSide.Player)
            {
                attackButton.SetActive(true);
            }
            else
            {
                damageRoll = attackRecieved.FlatModifier;
                SetCurrentStepResolved();
            }
        }

        private void ResetShowAttacker()
        {
            SetGlobalVisibility(true);
            ShowAttackerSide();
        }

        private void SetupForDefender()
        {
            SetNextToCurrent();
            ShowDefenderSide();
            if (defenderCurrent.side == UnitSide.Player)
            {
                blockButton.SetActive(true);
                dodgeButton.SetActive(true);
            }
            else
            {
                blockRoll = defenseCurrent.FlatReduce;
                SetCurrentStepResolved();
            }
        }

        private void SetCurrentStepResolved()
        {
            if (currentStep == EncounterStep.Attack)
            {
                attackResolved = true;
            }
            else if (currentStep == EncounterStep.Defense)
            {
                ApplyResult();
            }
        }

        private void FindAllPotentialTargets()
        {

            var tile = GameStateManager.Instance.GetActiveTile();
            var currentPosition = tile.GetUnitPosition(attackerRecieved.gameObject);
            var positions = tile.GetPositions();
            var allTargets = attackRecieved.FindTargetsInWeaponRange(attackerRecieved, currentPosition, positions);

            if (allTargets.Contains(defenderCurrent))
            {
                var targetPosition = tile.GetUnitPosition(defenderCurrent.gameObject);
                var finalTargets = attackRecieved.FindTargetsOnNode(attackerRecieved, targetPosition, defenderCurrent);

                foreach (var target in finalTargets)
                {
                    defensesToResolve.Add(target);
                }
            }
            else
            {
                throw new System.Exception();
            }
        }

        private void ShowAttackerSide()
        {
            var index = 0;
            PlaceAndDisplayModifier(attackBlackDiceContainer, attackBlackDiceText, attackRecieved.BlackDices, attackAnchor, anchorOffset, false, ref index);
            PlaceAndDisplayModifier(attackBlueDiceContainer, attackBlueDiceText, attackRecieved.BlueDices, attackAnchor, anchorOffset, false, ref index);
            PlaceAndDisplayModifier(attackOrangeDiceContainer, attackOrangeDiceText, attackRecieved.OrangeAttackDices, attackAnchor, anchorOffset, false, ref index);
            PlaceAndDisplayModifier(attackFlatModifierContainer, attackFlatModifierText, attackRecieved.FlatModifier, attackAnchor, anchorOffset, index == 0 ? true : false, ref index); ;
            PlaceAndDisplayModifier(attackDodgeDiceContainer, attackDodgeDiceText, attackRecieved.DodgeLevel, attackDodgeDiceContainer, Vector3.zero, attackerRecieved.side == UnitSide.Player ? false : true, ref index);
            PlaceAndDisplayModifier(attackRepeatContainer, attackRepeatText, attackRecieved.Repeat - attackRepeatCounter + 1, attackRepeatContainer, Vector3.zero, attackRecieved.Repeat > 1 ? true : false, ref index, 1); ;

            attackBleedToken.SetActive(attackRecieved.Bleed);
            attackPoisonToken.SetActive(attackRecieved.Poison);
            attackStaggerToken.SetActive(attackRecieved.Stagger);
            attackFrozenToken.SetActive(attackRecieved.Frozen);
            attackPushToken.SetActive(attackRecieved.Push);
        }

        private void HideButtonAndMoverAndDices()
        {
            confirmButton.SetActive(false);
            dodgeMover.Hide();
            pushMover.Hide();
            shiftMover.Hide();
            foreach (var dice in dices)
            {
                Destroy(dice);
            }
            dices.Clear();
        }

        private void ShowDefenderSide()
        {
            SetDefenseSideVisibility(true);

            var index = 0;
            PlaceAndDisplayModifier(defenseBlackDiceContainer, defenseBlackDiceText, defenseCurrent.BlackDices, defenseAnchor, anchorOffset, false, ref index);
            PlaceAndDisplayModifier(defenseBlueDiceContainer, defenseBlueDiceText, defenseCurrent.BlueDices, defenseAnchor, anchorOffset, false, ref index);
            PlaceAndDisplayModifier(defenseOrangeDiceContainer, defenseOrangeDiceText, defenseCurrent.OrangeDices, defenseAnchor, anchorOffset, false, ref index);
            PlaceAndDisplayModifier(defenseFlatModifierContainer, defenseFlatModifierText, defenseCurrent.FlatReduce, defenseAnchor, anchorOffset, index == 0 ? true : false, ref index); ;
            PlaceAndDisplayModifier(defenseDodgeDiceContainer, defenseDodgeDiceText, defenseCurrent.DodgeDices, defenseDodgeDiceContainer, Vector3.zero, true, ref index);

            defenseBleedToken.SetActive(defenderCurrent.isBleeding);
            defensePoisonToken.SetActive(defenderCurrent.isPoisoned);
            defenseStaggerToken.SetActive(defenderCurrent.isStaggered);
            defenseFrozenToken.SetActive(defenderCurrent.isFrozen);
        }

        private void ApplyStagger(AttackAction attack, UnitBasicProperties attacker)
        {
            if (attacker.side == UnitSide.Player)
            {
                attack.StaminaCost += attacker.isStaggered ? 1 : 0;
            }
            else if (attacker.side == UnitSide.Hollow)
            {
                attack.FlatModifier = attack.FlatModifier - (attacker.isStaggered ? 1 : 0);
            }
        }

        private void PlaceAndDisplayModifier(GameObject container, TextMesh text, int value, GameObject anchor, Vector3 offset, bool showDefaultValue, ref int offsetIndex, int defaultValue = 0)
        {
            if (value == defaultValue && !showDefaultValue)
            {
                container.SetActive(false);
                return;
            }

            container.SetActive(true);
            container.transform.localPosition = anchor.transform.localPosition + (offsetIndex * offset);
            text.text = value.ToString();
            offsetIndex++;
            return;
        }

        #region Option Select
        private void DodgeHoverEnded(GameObject position)
        {
            dodgeButtonRenderer.enabled = false;
        }

        private void DodgeHovered(GameObject position)
        {
            dodgeButtonRenderer.enabled = true;
            SpawnDices(0, 0, 0, defenseCurrent.DodgeDices);
        }

        private void DodgeSelected(GameObject position)
        {
            dodgeMover.SetupAndShow(defenderCurrent, MoveChoserType.Dodge);
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
            SpawnDices(defenseCurrent.BlackDices, defenseCurrent.BlueDices, defenseCurrent.OrangeDices, 0);
        }

        private void BlockSelected(GameObject position)
        {
            blockButton.SetActive(false);
            dodgeButton.SetActive(false);
            rollType = EncounterRollType.Block;
            diceResultRecieved = 0;
            ThrowDices();
        }

        private void AttackHoverEnded(GameObject position)
        {
            attackButtonRenderer.enabled = false;
        }

        private void AttackHovered(GameObject position)
        {
            attackButtonRenderer.enabled = true;
            SpawnDices(attackRecieved.BlackDices, attackRecieved.BlueDices, attackRecieved.OrangeAttackDices, 0);
        }

        private void AttackSelected(GameObject position)
        {
            attackButton.SetActive(false);
            rollType = EncounterRollType.Attack;
            diceResultRecieved = 0;
            ThrowDices();
        }

        private void PushMoveSelected(PositionBehavior position)
        {
            pushMover.Hide();
            ApplyResultFinalize();
        }

        private void ShiftMoveSelected(PositionBehavior position)
        {
            if (shiftBeforeResolved < attackRecieved.ShiftBefore)
            {
                shiftBeforeResolved++;
            }
            else if (shiftAfterResolved < attackRecieved.ShiftAfter)
            {
                shiftAfterResolved++;
            }

            if (shiftBeforeResolved < attackRecieved.ShiftBefore || shiftAfterResolved < attackRecieved.ShiftAfter)
            {
                SetupForShift();
            }
        }

        #endregion

        #region Dice
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
            var offsetPoint = new Vector3(Random.Range(-1.5f, 1.5f), 0, 0);
            dice.transform.position = throwPoint.transform.position + offsetPoint;
            dice.transform.localRotation = Quaternion.Euler(Random.Range(0, 90), Random.Range(0, 90), Random.Range(0, 90));

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
            dice.GetComponent<Rigidbody>().AddForceAtPosition(new Vector3(0, Random.Range(1f, 1.5f), Random.Range(1.5f, 2f)), dice.transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0, 0.5f), Random.Range(-0.5f, 0.5f)), ForceMode.Impulse);
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
                    blockRoll = defenseCurrent.FlatReduce;
                }
                else if (rollType == EncounterRollType.Dodge)
                {
                    dodgeRoll = 0;
                }
                else if (rollType == EncounterRollType.Attack)
                {
                    damageRoll = attackRecieved.FlatModifier;
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
                    else if (rollType == EncounterRollType.Attack)
                    {
                        damageRoll += behavior.GetValue();
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
                    Invoke(nameof(SetCurrentStepResolved), 2f);
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
            if (currentStep == EncounterStep.Attack && attackerRecieved is PlayerProperties)
            {
                var properties = (PlayerProperties)attackerRecieved;
                if ((properties.isActive && properties.hasEstus) || properties.hasLuckToken)
                    canAutoResolve = false;
            }

            else if (currentStep == EncounterStep.Defense && defenderCurrent is PlayerProperties)
            {
                var properties = (PlayerProperties)defenderCurrent;
                if ((properties.isActive && properties.hasEstus) || properties.hasLuckToken)
                    canAutoResolve = false;
            }
            return canAutoResolve;
        }

        private void ConfirmResult(GameObject position)
        {
            SetCurrentStepResolved();
        }
        #endregion

        private void ApplyResult()
        {
            var damageToApply = damageRoll;
            var hit = true;

            if (rollType == EncounterRollType.Block || rollType == EncounterRollType.Attack)
            {
                damageToApply = System.Math.Max(0, damageToApply - blockRoll);
            }
            else if (rollType == EncounterRollType.Dodge)
            {
                if (dodgeRoll >= attackRecieved.DodgeLevel)
                {
                    damageToApply = 0;
                    hit = false;
                }
                defenderCurrent.ConsumeStamina(1 + (defenderCurrent.isFrozen ? 1 : 0));
            }

            defenderCurrent.RecieveInjuries(damageToApply);

            if (hit)
            {
                defenderCurrent.isBleeding = defenderCurrent.isBleeding || attackRecieved.Bleed;
                defenderCurrent.isPoisoned = defenderCurrent.isPoisoned || attackRecieved.Poison;
                defenderCurrent.isStaggered = defenderCurrent.isStaggered || attackRecieved.Stagger;
                defenderCurrent.isFrozen = defenderCurrent.isFrozen || attackRecieved.Frozen;
            }

            if (hit && attackRecieved.Push)
            {
                confirmButton.SetActive(false);
                pushMover.SetupAndShow(defenderCurrent, MoveChoserType.Push, attackerRecieved);
            }
            else
            {
                ApplyResultFinalize();
            }

        }

        private void ApplyResultFinalize()
        {
            ClearCurrent();
            if (defensesToResolve.Any())
            {
                defensesToResolve.RemoveAt(0);
            }

            if (!defensesToResolve.Any())
            {
                defenseResolved = true;
            }
        }

        #region Token Effect
        private void UseLuckEvent(GameObject position)
        {
            var unit = attackerRecieved.side == UnitSide.Player ? (PlayerProperties)attackerRecieved : (PlayerProperties)defenderCurrent;
            if (unit.hasLuckToken)
            {
                foreach (var dice in dices)
                {
                    dice.GetComponent<RaiseEventOnClicked>().PositionClicked += DiceSelectedForRethrow;
                }
                unit.hasLuckToken = false;
            }
            SetConfirmButtonVisibility(false);
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
        #endregion

        #region visibility
        private void SetGlobalVisibility(bool visible)
        {
            blockButton.SetActive(false);
            dodgeButton.SetActive(false);
            attackButton.SetActive(false);

            dodgeMover.Hide();
            pushMover.Hide();
            shiftMover.Hide();

            SetConfirmButtonVisibility(false);

            foreach (var dice in dices)
            {
                Destroy(dice);
            }
            dices.Clear();

            SetAttackSideVisibility(visible);
            SetDefenseSideVisibility(false);
        }

        private void SetAttackSideVisibility(bool visible)
        {
            attackerPortrait.SetActive(visible);
            attackOptions.SetActive(visible);
            attackIcon.SetActive(visible);

            attackEstusBehavior.SetUnit(null);
            attackLuckBehavior.SetUnit(null);

            attackBleedToken.SetActive(false);
            attackPoisonToken.SetActive(false);
            attackStaggerToken.SetActive(false);
            attackFrozenToken.SetActive(false);
            attackPushToken.SetActive(false);

            if (visible)
            {
                attackerPortraitRenderer.material = attackerRecieved.portrait;
                attackEstusBehavior.SetUnit(attackerRecieved);
                attackLuckBehavior.SetUnit(attackerRecieved);
            }
        }

        private void SetDefenseSideVisibility(bool visible)
        {
            defenderPortrait.SetActive(visible);
            defenseOptions.SetActive(visible);
            defenseIcon.SetActive(visible);

            defenseEstusBehavior.SetUnit(null);
            defenseLuckBehavior.SetUnit(null);
            defenseEmberBehavior.SetUnit(null);

            if (visible)
            {
                defenderPortraitRenderer.material = defenderCurrent.portrait;
                defenseEstusBehavior.SetUnit(defenderCurrent);
                defenseLuckBehavior.SetUnit(defenderCurrent);
                defenseEmberBehavior.SetUnit(defenderCurrent);
            }
        }

        private void SetConfirmButtonVisibility(bool visibility)
        {
            confirmButton.SetActive(visibility);
        }
        #endregion

        #region State Management
        private void CloneRecievedToCurrent()
        {
            defenderCurrent = defenderRecieved;
            attackResolved = false;
            shiftBeforeResolved = 0;
            shiftAfterResolved = 0;
        }

        private void SetNextToCurrent()
        {
            var next = defensesToResolve.First();
            defenderCurrent = next;
            defenseCurrent = defenderCurrent.GetDefenseDices(attackRecieved.MagicAttack);
            blockRoll = 0;
            dodgeRoll = 0;
        }

        private void ClearCurrent()
        {
            defenderCurrent = null;
            defenseCurrent = null;
            blockRoll = 0;
            dodgeRoll = 0;
        }

        private bool IsCurrentCleared()
        {
            return defenderCurrent == null
            && defenseCurrent == null;
        }
        #endregion

        private enum EncounterStep
        {
            None,
            ShiftBefore,
            Attack,
            Defense,
            ShiftAfter,
            Resolved
        }

    }

    public enum EncounterRollType
    {
        Attack,
        Block,
        Dodge
    }
}