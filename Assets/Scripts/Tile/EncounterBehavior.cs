using Assets.Scripts.Unit.Model.Attacks;
using BoardGame.Script.Events;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EncounterBehavior : MonoBehaviour
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

    private MeshRenderer defenderPortraitRenderer;
    private MeshRenderer attackerPortraitRenderer;

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
    private MeshRenderer blockButtonRenderer;
    private MeshRenderer dodgeButtonRenderer;
    private MeshRenderer attackButtonRenderer;

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
    private int attackRepeatCounter;

    private Vector3 anchorOffset;
    private Vector3 offsetDiceResultPresentation;

    private Encounter encounterRecieved;
    private Encounter currentEncounter;
    private List<Encounter> defensesToResolve;
    private bool resolvingEncounter = false;

    private int shiftBeforeResolved = 0;
    private int shiftAfterResolved = 0;
    private bool attackResolved = false;
    private bool defenseResolved = false;
    private EncounterStep currentStep;


    void Start()
    {
        defensesToResolve = new List<Encounter>();
        dices = new List<GameObject>();

        anchorOffset = new Vector3(1.35f, 0f, 0f);
        offsetDiceResultPresentation = new Vector3(2, 0, 0);

        attackerPortraitRenderer = attackerPortrait.GetComponent<MeshRenderer>();
        defenderPortraitRenderer = defenderPortrait.GetComponent<MeshRenderer>();

        blockButtonRenderer = blockButton.GetComponent<MeshRenderer>();
        dodgeButtonRenderer = dodgeButton.GetComponent<MeshRenderer>();
        attackButtonRenderer = attackButton.GetComponent<MeshRenderer>();

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
            else if (currentStep == EncounterStep.ShiftBefore && shiftBeforeResolved == currentEncounter.Attack.ShiftBefore)
            {

                currentStep = EncounterStep.Attack;
                SetupForAttack();
            }
            else if (currentStep == EncounterStep.Attack && attackResolved)
            {
                currentStep = EncounterStep.Defense;
                defenseResolved = false;
                FindAllPotentialTargets();
                currentEncounter = null;
            }
            else if (currentStep == EncounterStep.Defense)
            {
                if (defenseResolved) // all defenses resolved for this same attack.
                {
                    currentStep = EncounterStep.ShiftAfter;
                    SetupForShift();
                }
                else if (currentEncounter == null) // load next defender against same attack.
                {
                    SetupForDefender();
                }
            }
            else if (currentStep == EncounterStep.ShiftAfter && shiftAfterResolved == encounterRecieved.Attack.ShiftAfter)
            {
                if (attackRepeatCounter < encounterRecieved.Attack.Repeat)
                {
                    attackRepeatCounter++;
                    currentStep = EncounterStep.None;
                    attackResolved = false;
                    attackResolved = defenseResolved = false;
                    shiftBeforeResolved = 0;
                    shiftAfterResolved = 0;
                    currentEncounter = encounterRecieved.Clone();
                    SetupForAttack();
                }
                else
                {
                    currentStep = EncounterStep.Resolved;
                }
                
            }
            else if(currentStep == EncounterStep.Resolved)
            {
                encounterRecieved.Attacker.ConsumeStamina(encounterRecieved.Attack.StaminaCost);
                EventManager.RaiseEvent(ObjectEventType.EncountersResolved);
                SetGlobalVisibility(false);
                resolvingEncounter = false;
            }
        }
    }

    public void Resolve(object eventLoad)
    {
        encounterRecieved = (Encounter)eventLoad;
        ApplyStagger(encounterRecieved);

        currentEncounter = encounterRecieved.Clone();
        resolvingEncounter = true;
        shiftBeforeResolved = 0;
        shiftAfterResolved = 0;
        attackRepeatCounter = 1;
        currentStep = EncounterStep.None;
    }

    private void SetupForShift()
    {
        if (currentStep == EncounterStep.ShiftBefore && shiftBeforeResolved >= encounterRecieved.Attack.ShiftBefore)
        {
            return;
        }
        if (currentStep == EncounterStep.ShiftAfter && shiftAfterResolved >= encounterRecieved.Attack.ShiftAfter)
        {
            return;
        }
        HideButtonAndMoverAndDices();
        shiftMover.SetupAndShow(encounterRecieved.Attacker, MoveChoserType.Shift);
    }

    private void SetupForAttack()
    {
        HideButtonAndMoverAndDices();
        if (currentEncounter.Attacker.side == UnitSide.Player)
        {
            attackButton.SetActive(true);
        }
        else
        {
            attackResolved = true;
        }
    }

    private void ResetShowAttacker()
    {
        SetGlobalVisibility(true);
        ShowAttackerSide();
    }

    private void SetupForDefender()
    {
        currentEncounter = defensesToResolve.First();
        ShowDefenderSide();
        if (currentEncounter.Defender.side == UnitSide.Player)
        {
            blockButton.SetActive(true);
            dodgeButton.SetActive(true);
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
        var currentPosition = tile.GetUnitPosition(currentEncounter.Attacker.gameObject);
        var positions = tile.GetPositions();
        var allTargets = currentEncounter.Attack.FindTargetsInRange(currentEncounter.Attacker, currentPosition, positions, false);

        if (allTargets.Contains(currentEncounter.Defender))
        {
            var targetPosition = tile.GetUnitPosition(currentEncounter.Defender.gameObject);
            var finalTargets = currentEncounter.Attack.FindTargetsOnNode(currentEncounter.Attacker, targetPosition, currentEncounter.Defender);

            foreach (var target in finalTargets)
            {
                var encounter = currentEncounter.Clone();
                encounter.Defender = target;
                defensesToResolve.Add(encounter);
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
        PlaceAndDisplayModifier(attackBlackDiceContainer, attackBlackDiceText, currentEncounter.Attack.BlackDices, attackAnchor, anchorOffset, false, ref index);
        PlaceAndDisplayModifier(attackBlueDiceContainer, attackBlueDiceText, currentEncounter.Attack.BlueDices, attackAnchor, anchorOffset, false, ref index);
        PlaceAndDisplayModifier(attackOrangeDiceContainer, attackOrangeDiceText, currentEncounter.Attack.OrangeAttackDices, attackAnchor, anchorOffset, false, ref index);
        PlaceAndDisplayModifier(attackFlatModifierContainer, attackFlatModifierText, currentEncounter.Attack.FlatModifier, attackAnchor, anchorOffset, index == 0 ? true : false, ref index); ;
        PlaceAndDisplayModifier(attackDodgeDiceContainer, attackDodgeDiceText, currentEncounter.Attack.DodgeLevel, attackDodgeDiceContainer, Vector3.zero, currentEncounter.Attacker.side == UnitSide.Player ? false : true, ref index);
        PlaceAndDisplayModifier(attackRepeatContainer, attackRepeatText, currentEncounter.Attack.Repeat - attackRepeatCounter + 1, attackRepeatContainer, Vector3.zero, currentEncounter.Attack.Repeat > 1 ? true : false, ref index, 1); ;

        attackBleedToken.SetActive(currentEncounter.Attack.Bleed);
        attackPoisonToken.SetActive(currentEncounter.Attack.Poison);
        attackStaggerToken.SetActive(currentEncounter.Attack.Stagger);
        attackFrozenToken.SetActive(currentEncounter.Attack.Frozen);
        attackPushToken.SetActive(currentEncounter.Attack.Push);
    }

    private void HideButtonAndMoverAndDices()
    {
        confirmButton.SetActive(false);
        dodgeMover.SetupAndShow(null, MoveChoserType.None);
        pushMover.SetupAndShow(null, MoveChoserType.None);
        shiftMover.SetupAndShow(null, MoveChoserType.None);
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
        PlaceAndDisplayModifier(defenseBlackDiceContainer, defenseBlackDiceText, currentEncounter.Defense.BlackDices, defenseAnchor, anchorOffset, false, ref index);
        PlaceAndDisplayModifier(defenseBlueDiceContainer, defenseBlueDiceText, currentEncounter.Defense.BlueDices, defenseAnchor, anchorOffset, false, ref index);
        PlaceAndDisplayModifier(defenseOrangeDiceContainer, defenseOrangeDiceText, currentEncounter.Defense.OrangeDices, defenseAnchor, anchorOffset, false, ref index);
        PlaceAndDisplayModifier(defenseFlatModifierContainer, defenseFlatModifierText, currentEncounter.Defense.FlatReduce, defenseAnchor, anchorOffset, index == 0 ? true : false, ref index); ;
        PlaceAndDisplayModifier(defenseDodgeDiceContainer, defenseDodgeDiceText, currentEncounter.Defense.DodgeDices, defenseDodgeDiceContainer, Vector3.zero, true, ref index);

        defenseBleedToken.SetActive(currentEncounter.Defender.isBleeding);
        defensePoisonToken.SetActive(currentEncounter.Defender.isPoisoned);
        defenseStaggerToken.SetActive(currentEncounter.Defender.isStaggered);
        defenseFrozenToken.SetActive(currentEncounter.Defender.isFrozen);
    }

    private void ApplyStagger(Encounter encounter)
    {
        if (encounter.Attacker.side == UnitSide.Player)
        {
            encounter.Attack.StaminaCost += encounter.Attacker.isStaggered ? 1 : 0;
        }
        else if (encounter.Attacker.side == UnitSide.Hollow)
        {
            encounter.Attack.FlatModifier = encounter.Attack.FlatModifier - (encounter.Attacker.isStaggered ? 1 : 0);
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
        SpawnDices(0, 0, 0, currentEncounter.Defense.DodgeDices);
    }

    private void DodgeSelected(GameObject position)
    {
        dodgeMover.SetupAndShow(currentEncounter.Defender, MoveChoserType.Dodge);
    }

    private void DodgeMoveSelected(PositionBehavior _)
    {
        dodgeMover.SetupAndShow(null, MoveChoserType.None);
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
        SpawnDices(currentEncounter.Defense.BlackDices, currentEncounter.Defense.BlueDices, currentEncounter.Defense.OrangeDices, 0);
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
        SpawnDices(currentEncounter.Attack.BlackDices, currentEncounter.Attack.BlueDices, currentEncounter.Attack.OrangeAttackDices, 0);
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
        pushMover.SetupAndShow(null, MoveChoserType.None);
        ApplyResultFinalize();
    }

    private void ShiftMoveSelected(PositionBehavior position)
    {
        if (shiftBeforeResolved < encounterRecieved.Attack.ShiftBefore)
        {
            shiftBeforeResolved++;
        }
        else if (shiftAfterResolved < encounterRecieved.Attack.ShiftAfter)
        {
            shiftAfterResolved++;
        }

        if (shiftBeforeResolved < encounterRecieved.Attack.ShiftBefore || shiftAfterResolved < encounterRecieved.Attack.ShiftAfter)
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
            currentEncounter.DefenseRoll = 0;
            currentEncounter.DamageRoll = 0;
            currentEncounter.DodgeRoll = 0;

            currentEncounter.DamageRoll = currentEncounter.Attack.FlatModifier;
            currentEncounter.DefenseRoll = currentEncounter.Defense.FlatReduce;

            foreach (var dice in dices)
            {
                var behavior = dice.GetComponent<DiceBahevior>();
                if (rollType == EncounterRollType.Block)
                {
                    currentEncounter.DefenseRoll += behavior.GetValue();
                }
                else if (rollType == EncounterRollType.Dodge)
                {
                    currentEncounter.DodgeRoll += behavior.GetValue();
                }
                else if (rollType == EncounterRollType.Attack)
                {
                    currentEncounter.DamageRoll += behavior.GetValue();
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
        if (currentStep == EncounterStep.Attack && currentEncounter.Attacker is PlayerProperties)
        {
            var properties = (PlayerProperties)currentEncounter.Attacker;
            if ((properties.isActive && properties.hasEstus) || properties.hasLuckToken)
                canAutoResolve = false;
        }

        else if (currentStep == EncounterStep.Defense && currentEncounter.Defender is PlayerProperties)
        {
            var properties = (PlayerProperties)currentEncounter.Defender;
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
        var damageToApply = currentEncounter.DamageRoll;
        var hit = true;

        if (rollType == EncounterRollType.Block || rollType == EncounterRollType.Attack)
        {
            damageToApply = System.Math.Max(0, damageToApply - currentEncounter.DefenseRoll);
        }
        else if (rollType == EncounterRollType.Dodge)
        {
            if (currentEncounter.DodgeRoll >= currentEncounter.Attack.DodgeLevel)
            {
                damageToApply = 0;
                hit = false;
            }
            currentEncounter.Defender.ConsumeStamina(1 + (currentEncounter.Defender.isFrozen ? 1 : 0));
        }

        currentEncounter.Defender.RecieveInjuries(damageToApply);

        if (hit)
        {
            currentEncounter.Defender.isBleeding = currentEncounter.Defender.isBleeding || currentEncounter.Attack.Bleed;
            currentEncounter.Defender.isPoisoned = currentEncounter.Defender.isPoisoned || currentEncounter.Attack.Poison;
            currentEncounter.Defender.isStaggered = currentEncounter.Defender.isStaggered || currentEncounter.Attack.Stagger;
            currentEncounter.Defender.isFrozen = currentEncounter.Defender.isFrozen || currentEncounter.Attack.Frozen;
        }

        if (hit && currentEncounter.Attack.Push)
        {
            confirmButton.SetActive(false);
            pushMover.SetupAndShow(currentEncounter.Defender, MoveChoserType.Push, currentEncounter.Attacker);
        }
        else
        {
            ApplyResultFinalize();
        }

    }

    private void ApplyResultFinalize()
    {
        currentEncounter = null;
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
        var unit = currentEncounter.Attacker.side == UnitSide.Player ? (PlayerProperties)currentEncounter.Attacker : (PlayerProperties)currentEncounter.Defender;
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

        dodgeMover.SetupAndShow(null, MoveChoserType.None);
        pushMover.SetupAndShow(null, MoveChoserType.None);
        shiftMover.SetupAndShow(null, MoveChoserType.None);

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
            attackerPortraitRenderer.material = encounterRecieved.Attacker.portrait;
            attackEstusBehavior.SetUnit(encounterRecieved.Attacker);
            attackLuckBehavior.SetUnit(encounterRecieved.Attacker);
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
            defenderPortraitRenderer.material = currentEncounter.Defender.portrait;
            defenseEstusBehavior.SetUnit(currentEncounter.Defender);
            defenseLuckBehavior.SetUnit(currentEncounter.Defender);
            defenseEmberBehavior.SetUnit(currentEncounter.Defender);
        }
    }

    private void SetConfirmButtonVisibility(bool visibility)
    {
        confirmButton.SetActive(visibility);
    }
    #endregion
}

public enum EncounterRollType
{
    Attack,
    Block,
    Dodge
}

public enum EncounterStep
{
    None,
    ShiftBefore,
    Attack,
    Defense,
    ShiftAfter,
    Resolved
}