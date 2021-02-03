﻿using Assets.Scripts.ActiveUnitDisplay;
using Assets.Scripts.Unit;
using BoardGame.Script.Events;
using UnityEngine;

public class AttackRadialDisplayBehavior : MonoBehaviour
{
    private Transform lowRowAnchor;
    private Transform highRowAnchor;

    public MeshRenderer backgroundLayerRenderer;
    public MeshRenderer hoverLayerRenderer;
    public MeshRenderer disableLayerRenderer;

    private Vector3 propertyPositionOffsetPerIndex;

    private Material fade_green_material;
    private Material fade_red_material;


    private TextMesh StaminaTextMesh;
    private TextMesh BlackDiceTextMesh;
    private TextMesh BlueDiceTextMesh;
    private TextMesh OrangeDiceTextMesh;
    private TextMesh RangeTextMesh;
    private TextMesh FlatBonusTextMesh;

    private Transform BlackContainer;
    private Transform BlueContainer;
    private Transform OrangeContainer;
    private Transform FlatContainer;

    private Transform RangeContainer;
    private Transform InifiniteRangeContainer;
    private Transform MinRange1Container;
    private Transform MagicContainer;
    private Transform SplashNodeContainer;

    private AttackSide Side;
    private UnitBasicProperties UnitProperties;
    private AttackDetail AttackDetail;

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
        AttackDetail.side = side;
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

        fade_green_material = (Material)Resources.Load("Material/PlainColors/fade_green_material", typeof(Material));
        fade_red_material = (Material)Resources.Load("Material/PlainColors/fade_red_material", typeof(Material));

        lowRowAnchor = backgroundLayerRenderer.transform.Find("LowRowAnchor");
        highRowAnchor = backgroundLayerRenderer.transform.Find("HighRowAnchor");

        BlueContainer = backgroundLayerRenderer.transform.Find("BlueDiceContainer");
        BlackContainer= backgroundLayerRenderer.transform.Find("BlackDiceContainer");
        OrangeContainer = backgroundLayerRenderer.transform.Find("OrangeDiceContainer");
        FlatContainer = backgroundLayerRenderer.transform.Find("FlatBonusContainer");

        RangeContainer = backgroundLayerRenderer.transform.Find("RangeContainer");
        InifiniteRangeContainer = backgroundLayerRenderer.transform.Find("InfiniteRangeContainer");
        MinRange1Container = backgroundLayerRenderer.transform.Find("MinRange1Container");
        MagicContainer = backgroundLayerRenderer.transform.Find("MagicContainer");
        SplashNodeContainer = backgroundLayerRenderer.transform.Find("SplashNodeContainer");

        StaminaTextMesh = backgroundLayerRenderer.transform.Find("StaminaCostText").GetComponent<TextMesh>();
        BlackDiceTextMesh = BlackContainer.transform.Find("BlackDiceText").GetComponent<TextMesh>();
        BlueDiceTextMesh = BlueContainer.transform.Find("BlueDiceText").GetComponent<TextMesh>();
        OrangeDiceTextMesh = OrangeContainer.transform.Find("OrangeDiceText").GetComponent<TextMesh>();

        RangeTextMesh = RangeContainer.transform.Find("RangeText").GetComponent<TextMesh>();
        FlatBonusTextMesh = FlatContainer.transform.Find("FlatBonusText").GetComponent<TextMesh>();

        IsSelected = false;
        IsDisabled = false;

        backgroundLayerRenderer.gameObject.SetActive(true);
        hoverLayerRenderer.enabled = IsSelected;
        hoverLayerRenderer.gameObject.SetActive(true);
        disableLayerRenderer.enabled = true;
        disableLayerRenderer.gameObject.SetActive(false);

        RegisterEvents();
    }

    private void RegisterEvents()
    {
        if (!EventsRegistered)
        {
            EventManager.StartListeningGameObject(EventTypes.ActiveUnitMoved, CheckEnoughStaminaLeft);
            EventManager.StartListeningObject(EventTypes.AttackApplied, DeselectAttackAndNotifySideExausted);
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

    private void SetupAttack()
    {
        var lowRowIndex = 0;
        SetTextOrHide(lowRowAnchor, AttackDetail.blackAttackDices, BlackContainer, BlackDiceTextMesh, ref lowRowIndex);
        SetTextOrHide(lowRowAnchor, AttackDetail.blueAttackDices, BlueContainer, BlueDiceTextMesh, ref lowRowIndex);
        SetTextOrHide(lowRowAnchor, AttackDetail.orangeAttackDices, OrangeContainer, OrangeDiceTextMesh, ref lowRowIndex);
        SetTextOrHide(lowRowAnchor, AttackDetail.flatModifier, FlatContainer, FlatBonusTextMesh, ref lowRowIndex);

        var highRowIndex = 0;
        SetTextOrHide(highRowAnchor, AttackDetail.range, RangeContainer, RangeTextMesh, ref highRowIndex, !AttackDetail.infiniteRange);
        SetTextOrHide(highRowAnchor, AttackDetail.infiniteRange ? 1 : 0, InifiniteRangeContainer, null, ref highRowIndex);
        SetTextOrHide(highRowAnchor, AttackDetail.minimumRange, MinRange1Container, null, ref highRowIndex);
        SetTextOrHide(highRowAnchor, AttackDetail.magicAttack ? 1 : 0, MagicContainer, null, ref highRowIndex);
        SetTextOrHide(highRowAnchor, AttackDetail.nodeSplash ? 1 : 0, SplashNodeContainer, null, ref highRowIndex);

        hoverLayerRenderer.material = AttackDetail.notEnoughStamina(UnitProperties.StaminaLeft()) ? fade_red_material : fade_green_material;
        hoverLayerRenderer.enabled = IsSelected;

        StaminaTextMesh.text = $"[{AttackDetail.staminaCost}]";
        RangeTextMesh.text = AttackDetail.range.ToString();

        if (IsSelected)
        {
            EventManager.RaiseEventObject(EventTypes.AttackSelected, AttackDetail);
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


    //handle deselect, hover while selected, reset/selection memory
    private void AttackClicked(GameObject _)
    {
        if (!IsSelected)
        {
            IsSelected = true;
            EventManager.RaiseEventObject(EventTypes.AttackSelected, AttackDetail);
        }
        else
        {
            IsSelected = false;
            EventManager.RaiseEventObject(EventTypes.AttackDeselected, AttackDetail);
            hoverLayerRenderer.enabled = false;
        }
    }

    private void AttackHoveredEnd(GameObject _)
    {
        EventManager.RaiseEventObject(EventTypes.AttackHoverEnded, AttackDetail);
        hoverLayerRenderer.enabled = IsSelected;
    }

    private void AttackHovered(GameObject _)
    {
        EventManager.RaiseEventObject(EventTypes.AttackHovered, AttackDetail);
        hoverLayerRenderer.enabled = true;
    }

    private void DeselectAttackAndNotifySideExausted(object attackApplied)
    {
        if (AttackDetail == attackApplied)
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
