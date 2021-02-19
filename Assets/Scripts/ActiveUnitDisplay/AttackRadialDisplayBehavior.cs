using Assets.Scripts.Unit.Model.Attacks;
using BoardGame.Script.Events;
using System;
using UnityEngine;

public class AttackRadialDisplayBehavior : MonoBehaviour
{
    private Transform lowRowAnchor;
    private Transform highRowAnchor;

    public MeshRenderer backgroundLayerRenderer;
    public MeshRenderer hoverLayerRenderer;
    public MeshRenderer disableLayerRenderer;
    public MeshRenderer selectedLayerRenderer;

    private Vector3 propertyPositionOffsetPerIndex;

    private Material fade_green_material;
    private Material fade_red_material;


    private TextMesh StaminaTextMesh;
    private TextMesh BlackDiceTextMesh;
    private TextMesh BlueDiceTextMesh;
    private TextMesh OrangeDiceTextMesh;
    private TextMesh RangeTextMesh;
    private TextMesh FlatBonusTextMesh;
    private TextMesh RepeatTextMesh;
    private TextMesh ShiftBeforeTextMesh;
    private TextMesh ShiftAfterTextMesh;

    private TextMesh MovementForwardTextMesh;
    private TextMesh MovementBackwardTextMesh;
    private TextMesh MovementLeftTextMesh;
    private TextMesh MovementRightTextMesh;

    private Transform BlackContainer;
    private Transform BlueContainer;
    private Transform OrangeContainer;
    private Transform FlatContainer;
    private Transform ShiftBeforeContainer;
    private Transform ShiftAfterContainer;
    private Transform MovementContainer;

    private Transform RangeContainer;
    private Transform InifiniteRangeContainer;
    private Transform MinRange1Container;
    private Transform MagicContainer;
    private Transform SplashNodeContainer;
    private Transform RepeatContainer;
    private Transform PushTokenContainer;

    private UnitBasicProperties UnitProperties;
    private AttackAction attack;
    private MovementAction movement;

    private bool IsSelected;
    private bool IsDisabled;
    private bool EventsRegistered;

    public void Start()
    {
    }

    void Update()
    {
    }

    public void SetupTile(BehaviorAction action, UnitBasicProperties properties, AttackSide side)
    {
        action.Side = side;
        if (action is AttackAction)
        {
            attack = (AttackAction)action;
        }
        else if (action is MovementAction)
        {
            movement = (MovementAction)action;
        }

        UnitProperties = properties;

        IsSelected = false;
        IsDisabled = false;

        SetupAttack();
    }

    public void Reset()
    {
        propertyPositionOffsetPerIndex = new Vector3(1.5f, 0, 0);

        backgroundLayerRenderer = transform.Find("background_layer").GetComponent<MeshRenderer>();
        hoverLayerRenderer = transform.Find("hover_layer").GetComponent<MeshRenderer>();
        disableLayerRenderer = transform.Find("disable_layer").GetComponent<MeshRenderer>();
        selectedLayerRenderer = transform.Find("selected_layer").GetComponent<MeshRenderer>();

        fade_green_material = (Material)Resources.Load("Material/PlainColors/fade_yellow_material", typeof(Material));
        fade_red_material = (Material)Resources.Load("Material/PlainColors/fade_red_material", typeof(Material));

        lowRowAnchor = backgroundLayerRenderer.transform.Find("LowRowAnchor");
        highRowAnchor = backgroundLayerRenderer.transform.Find("HighRowAnchor");

        BlueContainer = backgroundLayerRenderer.transform.Find("BlueDiceContainer");
        BlackContainer = backgroundLayerRenderer.transform.Find("BlackDiceContainer");
        OrangeContainer = backgroundLayerRenderer.transform.Find("OrangeDiceContainer");
        FlatContainer = backgroundLayerRenderer.transform.Find("FlatBonusContainer");
        ShiftBeforeContainer = backgroundLayerRenderer.transform.Find("ShiftBeforeContainer");
        ShiftAfterContainer = backgroundLayerRenderer.transform.Find("ShiftAfterContainer");
        MovementContainer = backgroundLayerRenderer.transform.Find("MovementContainer");

        RangeContainer = backgroundLayerRenderer.transform.Find("RangeContainer");
        InifiniteRangeContainer = backgroundLayerRenderer.transform.Find("InfiniteRangeContainer");
        MinRange1Container = backgroundLayerRenderer.transform.Find("MinRange1Container");
        MagicContainer = backgroundLayerRenderer.transform.Find("MagicContainer");
        SplashNodeContainer = backgroundLayerRenderer.transform.Find("SplashNodeContainer");
        RepeatContainer = backgroundLayerRenderer.transform.Find("RepeatContainer");
        PushTokenContainer = backgroundLayerRenderer.transform.Find("PushTokenContainer");

        StaminaTextMesh = backgroundLayerRenderer.transform.Find("StaminaCostText").GetComponent<TextMesh>();
        BlackDiceTextMesh = BlackContainer.transform.Find("BlackDiceText").GetComponent<TextMesh>();
        BlueDiceTextMesh = BlueContainer.transform.Find("BlueDiceText").GetComponent<TextMesh>();
        OrangeDiceTextMesh = OrangeContainer.transform.Find("OrangeDiceText").GetComponent<TextMesh>();
        ShiftBeforeTextMesh = ShiftBeforeContainer.transform.Find("ShiftBeforeText").GetComponent<TextMesh>();
        ShiftAfterTextMesh = ShiftAfterContainer.transform.Find("ShiftAfterText").GetComponent<TextMesh>();

        MovementForwardTextMesh = MovementContainer.transform.Find("MovementTextForward").GetComponent<TextMesh>();
        MovementBackwardTextMesh = MovementContainer.transform.Find("MovementTextBackward").GetComponent<TextMesh>();
        MovementLeftTextMesh = MovementContainer.transform.Find("MovementTextLeft").GetComponent<TextMesh>();
        MovementRightTextMesh = MovementContainer.transform.Find("MovementTextRight").GetComponent<TextMesh>();

        RangeTextMesh = RangeContainer.transform.Find("RangeText").GetComponent<TextMesh>();
        FlatBonusTextMesh = FlatContainer.transform.Find("FlatBonusText").GetComponent<TextMesh>();
        RepeatTextMesh = RepeatContainer.transform.Find("RepeatText").GetComponent<TextMesh>();

        IsSelected = false;
        IsDisabled = false;

        backgroundLayerRenderer.gameObject.SetActive(true);
        hoverLayerRenderer.enabled = !IsSelected;
        hoverLayerRenderer.gameObject.SetActive(true);
        selectedLayerRenderer.enabled = IsSelected;
        selectedLayerRenderer.gameObject.SetActive(true);
        disableLayerRenderer.enabled = true;
        disableLayerRenderer.gameObject.SetActive(false);

        attack = null;
        movement = null;

        RegisterEvents();
    }

    private void RegisterEvents()
    {
        if (!EventsRegistered)
        {
            EventManager.StartListening(ObjectEventType.AttackSelected, OtherAttackSelected);
            EventManager.StartListening(ObjectEventType.AttackApplied, DeselectAttack);
            EventManager.StartListening(GameObjectEventType.ActiveUnitMoved, CheckEnoughStaminaLeft);
            hoverLayerRenderer.gameObject.GetComponent<RaiseEventOnClicked>().PositionClicked += AttackClicked;
            hoverLayerRenderer.gameObject.GetComponent<RaiseEventOnEnterExit>().PositionEnter += AttackHovered;
            hoverLayerRenderer.gameObject.GetComponent<RaiseEventOnEnterExit>().PositionExit += AttackHoveredEnd;
            EventsRegistered = true;
        }
    }

    private BehaviorAction GetCurrentAction()
    {
        if (attack != null)
        {
            return attack;
        }
        else if (movement != null)
        {
            return movement;
        }
        return null;
    }

    private void CheckEnoughStaminaLeft(GameObject unit)
    {
        UnitProperties = unit.GetComponent<UnitBasicProperties>();
        SetupAttack();
    }



    private void SetupAttack()
    {
        var lowRowIndex = 0;
        var highRowIndex = 0;
        if (attack is AttackAction)
        {
            var attackDetailModified = (AttackAction)attack.Clone();
            ApplyStagger(attackDetailModified, UnitProperties);

            SetTextOrHide(lowRowAnchor, attackDetailModified.ShiftBefore, ShiftBeforeContainer, ShiftBeforeTextMesh, ref lowRowIndex, false);
            SetTextOrHide(lowRowAnchor, attackDetailModified.BlackDices, BlackContainer, BlackDiceTextMesh, ref lowRowIndex);
            SetTextOrHide(lowRowAnchor, attackDetailModified.BlueDices, BlueContainer, BlueDiceTextMesh, ref lowRowIndex);
            SetTextOrHide(lowRowAnchor, attackDetailModified.OrangeAttackDices, OrangeContainer, OrangeDiceTextMesh, ref lowRowIndex);
            SetTextOrHide(lowRowAnchor, attackDetailModified.FlatModifier, FlatContainer, FlatBonusTextMesh, ref lowRowIndex);
            SetTextOrHide(lowRowAnchor, attackDetailModified.ShiftAfter, ShiftAfterContainer, ShiftAfterTextMesh, ref lowRowIndex, false);

            SetTextOrHide(highRowAnchor, attackDetailModified.Range, RangeContainer, RangeTextMesh, ref highRowIndex, !attackDetailModified.InfiniteRange);
            SetTextOrHide(highRowAnchor, attackDetailModified.InfiniteRange ? 1 : 0, InifiniteRangeContainer, null, ref highRowIndex);
            SetTextOrHide(highRowAnchor, attackDetailModified.MinimumRange, MinRange1Container, null, ref highRowIndex);
            SetTextOrHide(highRowAnchor, attackDetailModified.MagicAttack ? 1 : 0, MagicContainer, null, ref highRowIndex);
            SetTextOrHide(highRowAnchor, attackDetailModified.NodeSplash ? 1 : 0, SplashNodeContainer, null, ref highRowIndex);
            SetTextOrHide(highRowAnchor, attackDetailModified.Repeat == 1 ? 0 : attackDetailModified.Repeat, RepeatContainer, RepeatTextMesh, ref highRowIndex);
            SetTextOrHide(highRowAnchor, attackDetailModified.Push ? 1 : 0, PushTokenContainer, null, ref highRowIndex);

            var unitStaminaLeft = UnitProperties is PlayerProperties ? UnitProperties.StaminaLeft() : 10;
            hoverLayerRenderer.material = attackDetailModified.HasEnoughStamina(unitStaminaLeft) ? fade_green_material : fade_red_material;
            hoverLayerRenderer.enabled = IsSelected;

            StaminaTextMesh.text = $"[{attackDetailModified.StaminaCost}]";
            RangeTextMesh.text = attackDetailModified.Range.ToString();

            MovementContainer.gameObject.SetActive(false);
        }
        else
        {
            TextMesh activeText = null;
            MovementLeftTextMesh.text = "";
            MovementRightTextMesh.text = "";
            MovementForwardTextMesh.text = "";
            MovementBackwardTextMesh.text = "";

            if (movement.Direction == MovementDirection.Left)
            {
                activeText = MovementLeftTextMesh;
                MovementLeftTextMesh.gameObject.SetActive(true);
            }
            if (movement.Direction == MovementDirection.Right)
            {
                activeText = MovementRightTextMesh;
                MovementRightTextMesh.gameObject.SetActive(true);
            }
            if (movement.Direction == MovementDirection.Forward)
            {
                activeText = MovementForwardTextMesh;
                MovementForwardTextMesh.gameObject.SetActive(true);
            }
            if (movement.Direction == MovementDirection.Backward)
            {
                activeText = MovementBackwardTextMesh;
                MovementBackwardTextMesh.gameObject.SetActive(true);
            }

            SetTextOrHide(lowRowAnchor, movement.MoveDistance, MovementContainer, activeText, ref lowRowIndex);

            SetTextOrHide(lowRowAnchor, 0, ShiftBeforeContainer, ShiftBeforeTextMesh, ref lowRowIndex);
            SetTextOrHide(lowRowAnchor, 0, ShiftAfterContainer, ShiftAfterTextMesh, ref lowRowIndex);
            SetTextOrHide(lowRowAnchor, 0, BlackContainer, BlackDiceTextMesh, ref lowRowIndex);
            SetTextOrHide(lowRowAnchor, 0, BlueContainer, BlueDiceTextMesh, ref lowRowIndex);
            SetTextOrHide(lowRowAnchor, 0, OrangeContainer, OrangeDiceTextMesh, ref lowRowIndex);
            SetTextOrHide(lowRowAnchor, 0, FlatContainer, FlatBonusTextMesh, ref lowRowIndex);

            SetTextOrHide(highRowAnchor, 0, RangeContainer, RangeTextMesh, ref highRowIndex);
            SetTextOrHide(highRowAnchor, 0, InifiniteRangeContainer, null, ref highRowIndex);
            SetTextOrHide(highRowAnchor, 0, MinRange1Container, null, ref highRowIndex);
            SetTextOrHide(highRowAnchor, 0, SplashNodeContainer, null, ref highRowIndex);
            SetTextOrHide(highRowAnchor, 0, RepeatContainer, RepeatTextMesh, ref highRowIndex);

            SetTextOrHide(highRowAnchor, movement.MagicAttack ? 1 : 0, MagicContainer, null, ref highRowIndex);
            SetTextOrHide(highRowAnchor, movement.Push ? 1 : 0, PushTokenContainer, null, ref highRowIndex);

            StaminaTextMesh.text = "";
            RangeTextMesh.text = "";

            hoverLayerRenderer.material = fade_green_material;
            hoverLayerRenderer.enabled = IsSelected;
        }

        if (IsSelected)
        {
            EventManager.RaiseEvent(ObjectEventType.AttackSelected, GetCurrentAction());
        }
    }
    private void ApplyStagger(AttackAction attack, UnitBasicProperties unit)
    {
        attack.StaminaCost += unit.isStaggered ? 1 : 0;
        if (unit is EnemyProperties)
        {
            attack.FlatModifier = System.Math.Max(0, attack.FlatModifier - (unit.isStaggered ? 1 : 0));
        }
    }

    private void SetTextOrHide(Transform anchor, int value, Transform container, TextMesh textMesh, ref int index, bool showAnyValue = false)
    {
        var isVisible = value != 0 || showAnyValue;

        if (textMesh != null)
        {
            textMesh.text = value.ToString();
        }

        var offset = index * propertyPositionOffsetPerIndex;
        container.transform.localPosition = anchor.localPosition + offset;
        container.gameObject.SetActive(isVisible);

        index += isVisible ? 1 : 0;
    }


    private void OtherAttackSelected(object selectedLoad)
    {
        if (selectedLoad is AttackAction && (AttackAction)selectedLoad != attack && IsSelected)
        {
            AttackClicked(null);
            hoverLayerRenderer.enabled = false;
        }
    }

    private void AttackClicked(GameObject eventArg)
    {
        if (IsSelected || eventArg == null)
        {
            IsSelected = false;
            EventManager.RaiseEvent(ObjectEventType.AttackDeselected, GetCurrentAction());
        }
        else if (!IsSelected)
        {
            IsSelected = true;
            EventManager.RaiseEvent(ObjectEventType.AttackSelected, GetCurrentAction());
        }

        selectedLayerRenderer.enabled = IsSelected;
        hoverLayerRenderer.enabled = !IsSelected;
    }

    private void AttackHoveredEnd(GameObject _)
    {
        EventManager.RaiseEvent(ObjectEventType.AttackHoverEnded, GetCurrentAction());
        hoverLayerRenderer.enabled = false;
        selectedLayerRenderer.enabled = IsSelected;
    }

    private void AttackHovered(GameObject _)
    {
        EventManager.RaiseEvent(ObjectEventType.AttackHovered, GetCurrentAction());
        hoverLayerRenderer.enabled = !IsSelected;
        selectedLayerRenderer.enabled = IsSelected;
    }

    private void DeselectAttack(object attackApplied)
    {
        if (attack == attackApplied)
        {
            AttackClicked(null);
        }
    }

    public void Disable()
    {
        IsDisabled = true;
        if (disableLayerRenderer != null)
        {
            disableLayerRenderer.gameObject.SetActive(IsDisabled);
        }
    }

}
