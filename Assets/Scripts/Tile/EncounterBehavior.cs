﻿using Assets.Scripts.Unit.Model.Attacks;
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

    private GameObject attackBlackDiceContainer;
    private TextMesh attackBlackDiceText;
    private GameObject attackBlueDiceContainer;
    private TextMesh attackBlueDiceText;
    private GameObject attackOrangeDiceContainer;
    private TextMesh attackOrangeDiceText;
    private GameObject attackFlatModifierContainer;
    private TextMesh attackFlatModifierText;
    private GameObject attackDodgeDiceContainer;
    private TextMesh attackDodgeDiceText;
    private GameObject attackAnchor;

    private GameObject defenseBlackDiceContainer;
    private TextMesh defenseBlackDiceText;
    private GameObject defenseBlueDiceContainer;
    private TextMesh defenseBlueDiceText;
    private GameObject defenseOrangeDiceContainer;
    private TextMesh defenseOrangeDiceText;
    private GameObject defenseFlatModifierContainer;
    private TextMesh defenseFlatModifierText;
    private GameObject defenseDodgeDiceContainer;
    private TextMesh defenseDodgeDiceText;
    private GameObject defenseAnchor;

    private GameObject blockButton;
    private GameObject dodgeButton;
    private GameObject attackButton;
    private MeshRenderer blockButtonRenderer;
    private MeshRenderer dodgeButtonRenderer;
    private MeshRenderer attackButtonRenderer;

    public TokenBehavior attackEstusBehavior;
    public TokenBehavior attackLuckBehavior;
    public TokenBehavior defenseEstusBehavior;
    public TokenBehavior defenseLuckBehavior;
    public TokenBehavior defenseEmberBehavior;

    private List<GameObject> dices;
    private int diceResultRecieved;
    private EncounterRollType rollType;
    private GameObject confirmButton;

    private Vector3 anchorOffset;
    private Vector3 offsetDiceResultPresentation;

    private List<Encounter> encounterToResolve;
    private List<Encounter> encounterResolved;
    private bool isResolving = false;

    private Encounter currentEncounter;

    void Start()
    {
        dices = new List<GameObject>();
        anchorOffset = new Vector3(1.35f, 0f, 0f);
        offsetDiceResultPresentation = new Vector3(2, 0, 0);
        attackerPortraitRenderer = attackerPortrait.GetComponent<MeshRenderer>();
        defenderPortraitRenderer = defenderPortrait.GetComponent<MeshRenderer>();

        blockButton = defenseOptions.transform.Find("ChooseBlockButton").gameObject;
        blockButtonRenderer = blockButton.GetComponent<MeshRenderer>();
        dodgeButton = defenseOptions.transform.Find("ChooseDodgeButton").gameObject;
        dodgeButtonRenderer = dodgeButton.GetComponent<MeshRenderer>();
        attackButton = attackOptions.transform.Find("ChooseAttackButton").gameObject;
        attackButtonRenderer = attackButton.GetComponent<MeshRenderer>();

        attackLuckBehavior.gameObject.GetComponent<RaiseEventOnClicked>().PositionClicked += UseLuckEvent;
        defenseLuckBehavior.GetComponent<RaiseEventOnClicked>().PositionClicked += UseLuckEvent;

        confirmButton = transform.Find("ConfirmResult").gameObject;

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

        attackBlackDiceContainer = attackOptions.transform.Find("BlackDiceContainer").gameObject;
        attackBlackDiceText = attackBlackDiceContainer.transform.Find("BlackDiceText").GetComponent<TextMesh>();
        attackBlueDiceContainer = attackOptions.transform.Find("BlueDiceContainer").gameObject;
        attackBlueDiceText = attackBlueDiceContainer.transform.Find("BlueDiceText").GetComponent<TextMesh>();
        attackOrangeDiceContainer = attackOptions.transform.Find("OrangeDiceContainer").gameObject;
        attackOrangeDiceText = attackOrangeDiceContainer.transform.Find("OrangeDiceText").GetComponent<TextMesh>();
        attackFlatModifierContainer = attackOptions.transform.Find("FlatBonusContainer").gameObject;
        attackFlatModifierText = attackFlatModifierContainer.transform.Find("FlatBonusText").GetComponent<TextMesh>();
        attackDodgeDiceContainer = attackOptions.transform.Find("DodgeDiceContainer").gameObject;
        attackDodgeDiceText = attackDodgeDiceContainer.transform.Find("DodgeDiceText").GetComponent<TextMesh>();
        attackAnchor = attackOptions.transform.Find("Anchor").gameObject;

        defenseBlackDiceContainer = defenseOptions.transform.Find("BlackDiceContainer").gameObject;
        defenseBlackDiceText = defenseBlackDiceContainer.transform.Find("BlackDiceText").GetComponent<TextMesh>();
        defenseBlueDiceContainer = defenseOptions.transform.Find("BlueDiceContainer").gameObject;
        defenseBlueDiceText = defenseBlueDiceContainer.transform.Find("BlueDiceText").GetComponent<TextMesh>();
        defenseOrangeDiceContainer = defenseOptions.transform.Find("OrangeDiceContainer").gameObject;
        defenseOrangeDiceText = defenseOrangeDiceContainer.transform.Find("OrangeDiceText").GetComponent<TextMesh>();
        defenseFlatModifierContainer = defenseOptions.transform.Find("FlatBonusContainer").gameObject;
        defenseFlatModifierText = defenseFlatModifierContainer.transform.Find("FlatBonusText").GetComponent<TextMesh>();
        defenseDodgeDiceContainer = defenseOptions.transform.Find("DodgeDiceContainer").gameObject;
        defenseDodgeDiceText = defenseDodgeDiceContainer.transform.Find("DodgeDiceText").GetComponent<TextMesh>();
        defenseAnchor = defenseOptions.transform.Find("Anchor").gameObject;

        EventManager.StartListening(ObjectEventType.EncountersToResolve, Resolve);

        SetGlobalVisibility(false);
    }

    void Update()
    {
        if (isResolving && currentEncounter == null)
        {
            if (encounterToResolve.Any())
            {
                SetGlobalVisibility(true);
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
        PlaceAndDisplayModifier(attackOrangeDiceContainer, attackOrangeDiceText, currentEncounter.Attack.orangeAttackDices, attackAnchor, anchorOffset, false, ref index);
        PlaceAndDisplayModifier(attackFlatModifierContainer, attackFlatModifierText, currentEncounter.Attack.flatModifier, attackAnchor, anchorOffset, index == 0 ? true : false, ref index); ;
        PlaceAndDisplayModifier(attackDodgeDiceContainer, attackDodgeDiceText, currentEncounter.Attack.dodgeLevel, attackDodgeDiceContainer, Vector3.zero, currentEncounter.Attacker.side == UnitSide.Player ? false : true, ref index);

        index = 0;
        PlaceAndDisplayModifier(defenseBlackDiceContainer, defenseBlackDiceText, currentEncounter.Defense.BlackDices, defenseAnchor, anchorOffset, false, ref index);
        PlaceAndDisplayModifier(defenseBlueDiceContainer, defenseBlueDiceText, currentEncounter.Defense.BlueDices, defenseAnchor, anchorOffset, false, ref index);
        PlaceAndDisplayModifier(defenseOrangeDiceContainer, defenseOrangeDiceText, currentEncounter.Defense.OrangeDices, defenseAnchor, anchorOffset, false, ref index);
        PlaceAndDisplayModifier(defenseFlatModifierContainer, defenseFlatModifierText, currentEncounter.Defense.FlatReduce, defenseAnchor, anchorOffset, index == 0 ? true : false, ref index); ;
        PlaceAndDisplayModifier(defenseDodgeDiceContainer, defenseDodgeDiceText, currentEncounter.Defense.DodgeDices, defenseDodgeDiceContainer, Vector3.zero, true, ref index);

    
    }

    private void PlaceAndDisplayModifier(GameObject container, TextMesh text, int value, GameObject anchor, Vector3 offset, bool showZero, ref int offsetIndex)
    {
        if (value == 0 && !showZero)
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
        SpawnDices(currentEncounter.Attack.BlackDices, currentEncounter.Attack.BlueDices, currentEncounter.Attack.orangeAttackDices, 0);
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

            currentEncounter.DamageRoll = currentEncounter.Attack.flatModifier;
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

    private void ConfirmResult(GameObject position)
    {
        ApplyResult();
    }
    #endregion

    private void ApplyResult()
    {
        foreach (var dice in dices)
        {
            dice.GetComponent<RaiseEventOnClicked>().PositionClicked -= ThrowOneDice;
        }

        var damageToApply = currentEncounter.DamageRoll;

        if (rollType == EncounterRollType.Block || rollType == EncounterRollType.Attack)
        {
            damageToApply = System.Math.Max(0, damageToApply - currentEncounter.DefenseRoll);
        }
        else if (rollType == EncounterRollType.Dodge)
        {
            if (currentEncounter.DodgeRoll >= currentEncounter.Attack.dodgeLevel)
            {
                damageToApply = 0;
            }
            currentEncounter.Defender.ConsumeStamina(1);
        }

        if (damageToApply >= 3 && currentEncounter.Defender.side == UnitSide.Player && ((PlayerProperties)currentEncounter.Defender).hasEmber)
        {
            damageToApply -= 1;
        }

        currentEncounter.Defender.RecieveInjuries(damageToApply);
        currentEncounter.Attacker.ConsumeStamina(currentEncounter.Attack.staminaCost);

        encounterToResolve.Remove(currentEncounter);
        encounterResolved.Add(currentEncounter);
        currentEncounter = null;
    }

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
        diceResultRecieved--;
        ThrowOneDice(dice);
        EventManager.StartListening(GameObjectEventType.DiceStoppedMoving, AddDiceResult);
    }

    #region visibility
    private void SetGlobalVisibility(bool visible)
    {
        blockButton.SetActive(false);
        dodgeButton.SetActive(false);
        attackButton.SetActive(false);

        attackerPortrait.SetActive(visible);
        defenderPortrait.SetActive(visible);

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

    private bool CanAutoConfirmResult()
    {
        var canAutoResolve = true;
        if(currentEncounter.Defender is PlayerProperties)
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
}

public enum EncounterRollType
{
    Attack,
    Block,
    Dodge
}
