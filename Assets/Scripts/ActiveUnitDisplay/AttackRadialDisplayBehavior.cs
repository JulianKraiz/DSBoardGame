using Assets.Scripts.Unit.Model.Attacks;
using BoardGame.Script.Events;
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

    private Transform BlackContainer;
    private Transform BlueContainer;
    private Transform OrangeContainer;
    private Transform FlatContainer;

    private Transform RangeContainer;
    private Transform InifiniteRangeContainer;
    private Transform MinRange1Container;
    private Transform MagicContainer;
    private Transform SplashNodeContainer;
    private Transform RepeatContainer;

    private UnitBasicProperties UnitProperties;
    private AttackDetail AttackDetail;
    private AttackDetail AttackDetailModified;

    private bool IsSelected;
    private bool IsDisabled;
    private bool EventsRegistered;

    public void Start()
    {
    }

    void Update()
    {
    }

    public void SetupTile(AttackDetail attack, UnitBasicProperties properties, AttackSide side)
    {
        AttackDetail = attack;
        AttackDetail.Side = side;
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

        RangeContainer = backgroundLayerRenderer.transform.Find("RangeContainer");
        InifiniteRangeContainer = backgroundLayerRenderer.transform.Find("InfiniteRangeContainer");
        MinRange1Container = backgroundLayerRenderer.transform.Find("MinRange1Container");
        MagicContainer = backgroundLayerRenderer.transform.Find("MagicContainer");
        SplashNodeContainer = backgroundLayerRenderer.transform.Find("SplashNodeContainer");
        RepeatContainer = backgroundLayerRenderer.transform.Find("RepeatContainer");

        StaminaTextMesh = backgroundLayerRenderer.transform.Find("StaminaCostText").GetComponent<TextMesh>();
        BlackDiceTextMesh = BlackContainer.transform.Find("BlackDiceText").GetComponent<TextMesh>();
        BlueDiceTextMesh = BlueContainer.transform.Find("BlueDiceText").GetComponent<TextMesh>();
        OrangeDiceTextMesh = OrangeContainer.transform.Find("OrangeDiceText").GetComponent<TextMesh>();

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

        RegisterEvents();
    }

    private void RegisterEvents()
    {
        if (!EventsRegistered)
        {
            EventManager.StartListening(ObjectEventType.AttackSelected, AttackSelectedEventRecieved);
            EventManager.StartListening(ObjectEventType.AttackApplied, DeselectAttackAndNotifySideExausted);
            EventManager.StartListening(GameObjectEventType.ActiveUnitMoved, CheckEnoughStaminaLeft);
            hoverLayerRenderer.gameObject.GetComponent<RaiseEventOnClicked>().PositionClicked += AttackClicked;
            hoverLayerRenderer.gameObject.GetComponent<RaiseEventOnEnterExit>().PositionEnter += AttackHovered;
            hoverLayerRenderer.gameObject.GetComponent<RaiseEventOnEnterExit>().PositionExit += AttackHoveredEnd;
            EventsRegistered = true;
        }
    }

    private void CheckEnoughStaminaLeft(GameObject unit)
    {
        UnitProperties = unit.GetComponent<UnitBasicProperties>();
        SetupAttack();
    }

    private void ApplyStagger()
    {
        AttackDetailModified.StaminaCost += UnitProperties.staggerToken ? 1 : 0;
        if (UnitProperties is EnemyProperties)
        {
            AttackDetailModified.FlatModifier = System.Math.Max(0, AttackDetailModified.FlatModifier - (UnitProperties.staggerToken ? 1 : 0));
        }
    }

    private void SetupAttack()
    {
        AttackDetailModified = AttackDetail.Clone();
        ApplyStagger();

        var lowRowIndex = 0;
        SetTextOrHide(lowRowAnchor, AttackDetailModified.BlackDices, BlackContainer, BlackDiceTextMesh, ref lowRowIndex);
        SetTextOrHide(lowRowAnchor, AttackDetailModified.BlueDices, BlueContainer, BlueDiceTextMesh, ref lowRowIndex);
        SetTextOrHide(lowRowAnchor, AttackDetailModified.OrangeAttackDices, OrangeContainer, OrangeDiceTextMesh, ref lowRowIndex);
        SetTextOrHide(lowRowAnchor, AttackDetailModified.FlatModifier, FlatContainer, FlatBonusTextMesh, ref lowRowIndex);

        var highRowIndex = 0;
        SetTextOrHide(highRowAnchor, AttackDetailModified.Range, RangeContainer, RangeTextMesh, ref highRowIndex, !AttackDetailModified.InfiniteRange);
        SetTextOrHide(highRowAnchor, AttackDetailModified.InfiniteRange ? 1 : 0, InifiniteRangeContainer, null, ref highRowIndex);
        SetTextOrHide(highRowAnchor, AttackDetailModified.MinimumRange, MinRange1Container, null, ref highRowIndex);
        SetTextOrHide(highRowAnchor, AttackDetailModified.MagicAttack ? 1 : 0, MagicContainer, null, ref highRowIndex);
        SetTextOrHide(highRowAnchor, AttackDetailModified.NodeSplash ? 1 : 0, SplashNodeContainer, null, ref highRowIndex);
        SetTextOrHide(highRowAnchor, AttackDetailModified.Repeat == 1 ? 0 : AttackDetailModified.Repeat, RepeatContainer, RepeatTextMesh, ref highRowIndex);

        var unitStaminaLeft = UnitProperties is PlayerProperties ? UnitProperties.StaminaLeft() : 10;
        hoverLayerRenderer.material = AttackDetailModified.notEnoughStamina(unitStaminaLeft) ? fade_red_material : fade_green_material;
        hoverLayerRenderer.enabled = IsSelected;

        StaminaTextMesh.text = $"[{AttackDetailModified.StaminaCost}]";
        RangeTextMesh.text = AttackDetailModified.Range.ToString();

        if (IsSelected)
        {
            EventManager.RaiseEvent(ObjectEventType.AttackSelected, AttackDetailModified);
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


    private void AttackSelectedEventRecieved(object selectedLoad)
    {
        if ((AttackDetail)selectedLoad != AttackDetailModified && IsSelected)
        {
            AttackClicked(null);
            hoverLayerRenderer.enabled = false;
        }
    }

    private void AttackClicked(GameObject _)
    {
        if (!IsSelected)
        {
            IsSelected = true;
            EventManager.RaiseEvent(ObjectEventType.AttackSelected, AttackDetailModified);
        }
        else
        {
            IsSelected = false;
            EventManager.RaiseEvent(ObjectEventType.AttackDeselected, AttackDetailModified);
        }
        selectedLayerRenderer.enabled = IsSelected;
        hoverLayerRenderer.enabled = !IsSelected;
    }

    private void AttackHoveredEnd(GameObject _)
    {
        EventManager.RaiseEvent(ObjectEventType.AttackHoverEnded, AttackDetailModified);
        hoverLayerRenderer.enabled = false;
        selectedLayerRenderer.enabled = IsSelected;
    }

    private void AttackHovered(GameObject _)
    {
        EventManager.RaiseEvent(ObjectEventType.AttackHovered, AttackDetailModified);
        hoverLayerRenderer.enabled = !IsSelected;
        selectedLayerRenderer.enabled = IsSelected;
    }

    private void DeselectAttackAndNotifySideExausted(object attackApplied)
    {
        if (AttackDetailModified == attackApplied)
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
