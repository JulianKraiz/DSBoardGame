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

    public GameObject attackBleedToken;
    public GameObject attackPoisonToken;
    public GameObject attackStaggerToken;
    public GameObject attackFrozenToken;

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

    private List<Encounter> encounterToResolve;
    private List<Encounter> encounterResolved;
    private bool isResolving = false;

    private Encounter currentEncounter;

    void Start()
    {
        encounterResolved = new List<Encounter>();
        encounterToResolve = new List<Encounter>();
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

        EventManager.StartListening(ObjectEventType.EncountersToResolve, Resolve);

        SetGlobalVisibility(false);
    }

    void Update()
    {
        if (isResolving && currentEncounter == null)
        {
            if (encounterToResolve.Any())
            {
                attackRepeatCounter = 1;
                currentEncounter = encounterToResolve.First();
                SetupEncounterDisplay();
            }
            else if (encounterToResolve.Count == 0)
            {
                isResolving = false;
                EventManager.RaiseEvent(ObjectEventType.EncountersResolved, encounterResolved.ToList());
                encounterResolved = new List<Encounter>();
                SetGlobalVisibility(false);
            }
        }
    }

    public void Resolve(object eventLoad)
    {
        encounterToResolve = (List<Encounter>)eventLoad; ;
        encounterResolved = new List<Encounter>();
        isResolving = true;
    }

    private void SetupEncounterDisplay()
    {
        SetGlobalVisibility(true);

        attackerPortraitRenderer.material = currentEncounter.Attacker.portrait;
        defenderPortraitRenderer.material = currentEncounter.Defender.portrait;

        attackEstusBehavior.SetUnit(currentEncounter.Attacker);
        attackLuckBehavior.SetUnit(currentEncounter.Attacker);
        defenseEstusBehavior.SetUnit(currentEncounter.Defender);
        defenseLuckBehavior.SetUnit(currentEncounter.Defender);
        defenseEmberBehavior.SetUnit(currentEncounter.Defender);

        if (currentEncounter.Attacker.side == UnitSide.Hollow)
        {
            blockButton.SetActive(true);
            dodgeButton.SetActive(true);
            attackButton.SetActive(false);
        }
        else
        {
            blockButton.SetActive(false);
            dodgeButton.SetActive(false);
            attackButton.SetActive(true);
        }

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

        index = 0;
        PlaceAndDisplayModifier(defenseBlackDiceContainer, defenseBlackDiceText, currentEncounter.Defense.BlackDices, defenseAnchor, anchorOffset, false, ref index);
        PlaceAndDisplayModifier(defenseBlueDiceContainer, defenseBlueDiceText, currentEncounter.Defense.BlueDices, defenseAnchor, anchorOffset, false, ref index);
        PlaceAndDisplayModifier(defenseOrangeDiceContainer, defenseOrangeDiceText, currentEncounter.Defense.OrangeDices, defenseAnchor, anchorOffset, false, ref index);
        PlaceAndDisplayModifier(defenseFlatModifierContainer, defenseFlatModifierText, currentEncounter.Defense.FlatReduce, defenseAnchor, anchorOffset, index == 0 ? true : false, ref index); ;
        PlaceAndDisplayModifier(defenseDodgeDiceContainer, defenseDodgeDiceText, currentEncounter.Defense.DodgeDices, defenseDodgeDiceContainer, Vector3.zero, true, ref index);
        defenseBleedToken.SetActive(currentEncounter.Defender.bleedToken);
        defensePoisonToken.SetActive(currentEncounter.Defender.poisonToken);
        defenseStaggerToken.SetActive(currentEncounter.Defender.staggerToken);
        defenseFrozenToken.SetActive(currentEncounter.Defender.frozenToken);


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

    #region option select
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
        dodgeMover.SetupAndShow(currentEncounter.Defender);
    }

    private void DodgeMoveSelected(PositionBehavior _)
    {
        dodgeMover.SetupAndShow(null);
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
    #endregion

    #region dice
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
        foreach (var dice in dices)
        {
            ThrowOneDice(dice);
        }
        EventManager.StartListening(GameObjectEventType.DiceStoppedMoving, AddDiceResult);
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
                Invoke(nameof(ApplyResult), 2f);
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
        if (currentEncounter.Defender is PlayerProperties)
        {
            var properties = (PlayerProperties)currentEncounter.Defender;
            if ((properties.isActive && properties.hasEstus) || properties.hasLuckToken)
                canAutoResolve = false;
        }
        if (currentEncounter.Attacker is PlayerProperties)
        {
            var properties = (PlayerProperties)currentEncounter.Attacker;
            if ((properties.isActive && properties.hasEstus) || properties.hasLuckToken)
                canAutoResolve = false;
        }
        return canAutoResolve;
    }

    private void ConfirmResult(GameObject position)
    {
        ApplyResult();
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
            currentEncounter.Defender.ConsumeStamina(1 + (currentEncounter.Defender.frozenToken ? 1 : 0));
        }

        currentEncounter.Defender.RecieveInjuries(damageToApply);
        currentEncounter.Attacker.ConsumeStamina(currentEncounter.Attack.StaminaCost);

        currentEncounter.Defender.bleedToken = hit && (currentEncounter.Defender.bleedToken || currentEncounter.Attack.Bleed);
        currentEncounter.Defender.poisonToken = hit && (currentEncounter.Defender.poisonToken || currentEncounter.Attack.Poison);
        currentEncounter.Defender.staggerToken = hit && (currentEncounter.Defender.staggerToken || currentEncounter.Attack.Stagger);
        currentEncounter.Defender.frozenToken = hit && (currentEncounter.Defender.frozenToken || currentEncounter.Attack.Frozen);

        encounterToResolve.Remove(currentEncounter);
        encounterResolved.Add(currentEncounter);

        if (attackRepeatCounter < currentEncounter.Attack.Repeat)
        {
            attackRepeatCounter++;
            SetupEncounterDisplay();
        }
        else
        {
            currentEncounter = null;

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

        attackerPortrait.SetActive(visible);
        defenderPortrait.SetActive(visible);

        dodgeMover.SetupAndShow(null);

        SetConfirmButtonVisibility(false);

        attackOptions.SetActive(visible);
        attackIcon.SetActive(visible);
        defenseOptions.SetActive(visible);
        defenseIcon.SetActive(visible);

        foreach (var dice in dices)
        {
            Destroy(dice);
        }
        dices.Clear();

        attackEstusBehavior.SetUnit(null);
        attackLuckBehavior.SetUnit(null);
        defenseEstusBehavior.SetUnit(null);
        defenseLuckBehavior.SetUnit(null);
        defenseEmberBehavior.SetUnit(null);

        attackBleedToken.SetActive(false);
        attackPoisonToken.SetActive(false);
        attackStaggerToken.SetActive(false);
        attackFrozenToken.SetActive(false);
    }

    private void SetConfirmButtonVisibility(bool visibility)
    {
        confirmButton.SetActive(visibility);
    }
    #endregion

    private void NotifyEncountersResolved()
    {
        EventManager.RaiseEvent(ObjectEventType.EncountersResolved, encounterResolved);
    }


}

public enum EncounterRollType
{
    Attack,
    Block,
    Dodge
}
