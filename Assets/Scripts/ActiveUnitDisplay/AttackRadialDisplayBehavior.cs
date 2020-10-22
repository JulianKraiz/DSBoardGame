using Assets.Scripts.ActiveUnitDisplay;
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
    private Vector3 propertyTextPositionOffset;
    private Vector3 propertyTextRangePositionOffset;

    private Material fade_green_material;
    private Material fade_red_material;

    private AttackRadialSide Side;
    private TextMesh StaminaTextMesh;
    private TextMesh BlackDiceTextMesh;
    private TextMesh BlueDiceTextMesh;
    private TextMesh OrangeDiceTextMesh;
    private TextMesh RangeTextMesh;
    private TextMesh FlatBonusTextMesh;

    private MeshRenderer StaminaTextRenderer;
    private MeshRenderer BlackDiceTextRenderer;
    private MeshRenderer BlueDiceTextRenderer;
    private MeshRenderer OrangeDiceTextRenderer;
    private MeshRenderer RangeTextRenderer;
    private MeshRenderer FlatBonusTextRenderer;

    private MeshRenderer FlatBonusBackgroundRenderer;
    private MeshRenderer BlackDiceBackgroundRenderer;
    private MeshRenderer BlueDiceBackgroundRenderer;
    private MeshRenderer OrangeDiceBackgroundRenderer;
    private MeshRenderer MagicAttackBackgroundRenderer;
    private MeshRenderer SplashNodeBackgroundRenderer;
    private MeshRenderer RangeBackgroundRenderer;
    private MeshRenderer InifiniteRangeBackgroundRenderer;
    private MeshRenderer MinRange1BackgroundRenderer;

    private UnitBasicProperties UnitProperties;
    private AttackDetail RawAttackDetail;
    private AttackRadialDetail ComputedAttackDetail;

    private bool IsSelected;
    private bool IsDisabled;
    private bool EventsRegistered;

    public void Start()
    {
    }

    void Update()
    {
    }

    public void SetupTile(AttackDetail attack, UnitBasicProperties properties, AttackRadialSide side)
    {
        RawAttackDetail = attack;
        UnitProperties = properties;
        Side = side;
        
        IsSelected = false;
        IsDisabled = false;
        ComputeAttackDetail(RawAttackDetail, UnitProperties);
        SetupAttack(ComputedAttackDetail);
        
    }

    public void Reset()
    {
        propertyPositionOffsetPerIndex = new Vector3(1, 0, 0);
        propertyTextPositionOffset = new Vector3(-0.15f, 0, 0.32f);
        propertyTextRangePositionOffset = new Vector3(-0.2f, 0, 0.12f);

        backgroundLayerRenderer = transform.Find("background_layer").GetComponent<MeshRenderer>();
        hoverLayerRenderer = transform.Find("hover_layer").GetComponent<MeshRenderer>();
        disableLayerRenderer = transform.Find("disable_layer").GetComponent<MeshRenderer>();

        fade_green_material = (Material)Resources.Load("Material/PlainColors/fade_green_material", typeof(Material));
        fade_red_material = (Material)Resources.Load("Material/PlainColors/fade_red_material", typeof(Material));

        lowRowAnchor = backgroundLayerRenderer.transform.Find("LowRowAnchor");
        highRowAnchor = backgroundLayerRenderer.transform.Find("HighRowAnchor");

        StaminaTextMesh = backgroundLayerRenderer.transform.Find("StaminaCostText").GetComponent<TextMesh>();
        BlackDiceTextMesh = backgroundLayerRenderer.transform.Find("BlackDiceText").GetComponent<TextMesh>();
        BlueDiceTextMesh = backgroundLayerRenderer.transform.Find("BlueDiceText").GetComponent<TextMesh>();
        OrangeDiceTextMesh = backgroundLayerRenderer.transform.Find("OrangeDiceText").GetComponent<TextMesh>();
        RangeTextMesh = backgroundLayerRenderer.transform.Find("RangeText").GetComponent<TextMesh>();
        FlatBonusTextMesh = backgroundLayerRenderer.transform.Find("FlatBonusText").GetComponent<TextMesh>();

        StaminaTextRenderer = backgroundLayerRenderer.transform.Find("StaminaCostText").GetComponent<MeshRenderer>();
        BlackDiceTextRenderer = backgroundLayerRenderer.transform.Find("BlackDiceText").GetComponent<MeshRenderer>();
        BlueDiceTextRenderer = backgroundLayerRenderer.transform.Find("BlueDiceText").GetComponent<MeshRenderer>();
        OrangeDiceTextRenderer = backgroundLayerRenderer.transform.Find("OrangeDiceText").GetComponent<MeshRenderer>();
        RangeTextRenderer = backgroundLayerRenderer.transform.Find("RangeText").GetComponent<MeshRenderer>();
        FlatBonusTextRenderer = backgroundLayerRenderer.transform.Find("FlatBonusText").GetComponent<MeshRenderer>();

        FlatBonusBackgroundRenderer = backgroundLayerRenderer.transform.Find("FlatBonus").GetComponent<MeshRenderer>(); ;
        BlackDiceBackgroundRenderer = backgroundLayerRenderer.transform.Find("BlackDices").GetComponent<MeshRenderer>(); ;
        BlueDiceBackgroundRenderer = backgroundLayerRenderer.transform.Find("BlueDices").GetComponent<MeshRenderer>(); ;
        OrangeDiceBackgroundRenderer = backgroundLayerRenderer.transform.Find("OrangeDices").GetComponent<MeshRenderer>(); ;
        MagicAttackBackgroundRenderer = backgroundLayerRenderer.transform.Find("MagicAttack").GetComponent<MeshRenderer>();
        SplashNodeBackgroundRenderer = backgroundLayerRenderer.transform.Find("SplashNode").GetComponent<MeshRenderer>();
        MinRange1BackgroundRenderer = backgroundLayerRenderer.transform.Find("MinRange1").GetComponent<MeshRenderer>();
        RangeBackgroundRenderer = backgroundLayerRenderer.transform.Find("Range").GetComponent<MeshRenderer>();
        InifiniteRangeBackgroundRenderer = backgroundLayerRenderer.transform.Find("InfiniteRange").GetComponent<MeshRenderer>();

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
        ComputeAttackDetail(RawAttackDetail, UnitProperties);
        SetupAttack(ComputedAttackDetail);
    }

    private void SetupAttack(AttackRadialDetail detail)
    {
        var lowRowIndex = 0;
        SetTextOrHide(lowRowAnchor, detail.blackAttackDices, BlackDiceTextMesh, BlackDiceTextRenderer, BlackDiceBackgroundRenderer, propertyTextPositionOffset, ref lowRowIndex);
        SetTextOrHide(lowRowAnchor, detail.blueAttackDices, BlueDiceTextMesh, BlueDiceTextRenderer, BlueDiceBackgroundRenderer, propertyTextPositionOffset, ref lowRowIndex);
        SetTextOrHide(lowRowAnchor, detail.orangeAttackDices, OrangeDiceTextMesh, OrangeDiceTextRenderer, OrangeDiceBackgroundRenderer, propertyTextPositionOffset, ref lowRowIndex);
        SetTextOrHide(lowRowAnchor, detail.flatModifier, FlatBonusTextMesh, FlatBonusTextRenderer, FlatBonusBackgroundRenderer, propertyTextPositionOffset, ref lowRowIndex);

        var highRowIndex = 0;
        SetTextOrHide(highRowAnchor, detail.range, RangeTextMesh, RangeTextRenderer, RangeBackgroundRenderer, propertyTextRangePositionOffset,ref highRowIndex, !detail.infiniteRange);
        SetTextOrHide(highRowAnchor, detail.infiniteRange ? 1 : 0, null, null, InifiniteRangeBackgroundRenderer, propertyTextPositionOffset, ref highRowIndex);
        SetTextOrHide(highRowAnchor, detail.minimumRange, null, null, MinRange1BackgroundRenderer, propertyTextPositionOffset, ref highRowIndex);
        SetTextOrHide(highRowAnchor, detail.magicAttack ? 1 : 0, null, null, MagicAttackBackgroundRenderer, propertyTextPositionOffset, ref highRowIndex);
        SetTextOrHide(highRowAnchor, detail.nodeSplash ? 1 : 0, null, null, SplashNodeBackgroundRenderer, propertyTextPositionOffset, ref highRowIndex);

        hoverLayerRenderer.material = detail.notEnoughStamina ? fade_red_material : fade_green_material;
        hoverLayerRenderer.enabled = IsSelected;

        StaminaTextMesh.text = $"[{detail.staminaCost}]";
        RangeTextMesh.text = detail.range.ToString();

        if(IsSelected)
        {
            EventManager.RaiseEventObject(EventTypes.AttackSelected, ComputedAttackDetail);
        }
    }

    private void ComputeAttackDetail(AttackDetail attack, UnitBasicProperties unitProperties)
    {
        ComputedAttackDetail = new AttackRadialDetail();
        ComputedAttackDetail.blackAttackDices = attack.blackAttackDices;
        ComputedAttackDetail.blueAttackDices = attack.blueAttackDices;
        ComputedAttackDetail.orangeAttackDices = attack.orangeAttackDices;
        ComputedAttackDetail.nodeSplash = attack.nodeSplash;
        ComputedAttackDetail.range = attack.range;
        ComputedAttackDetail.infiniteRange = attack.infiniteRange;
        ComputedAttackDetail.minimumRange = attack.minimumRange;
        ComputedAttackDetail.magicAttack = attack.magicAttack;
        ComputedAttackDetail.staminaCost = attack.staminaCost;
        ComputedAttackDetail.notEnoughStamina = unitProperties.StaminaLeft() - attack.staminaCost <= 0;
        ComputedAttackDetail.flatModifier = attack.flatModifier;
        ComputedAttackDetail.targetPlayers = attack.targetPlayers;
        ComputedAttackDetail.side = Side;
    }

    private void SetTextOrHide(Transform anchor, int value, TextMesh textMesh, MeshRenderer textRenderer, MeshRenderer backgroundRenderer, Vector3 textOffset, ref int index, bool showAnyValue = false)
    {
        var isVisible = value != 0 || showAnyValue;

        if (textRenderer != null)
        {
            textRenderer.enabled = isVisible;
            textMesh.text = value.ToString();

            var offset = index * propertyPositionOffsetPerIndex + textOffset;
            textRenderer.transform.localPosition = anchor.localPosition + offset;
        }

        if (backgroundRenderer != null)
        {
            backgroundRenderer.enabled = isVisible;

            var offset = index * propertyPositionOffsetPerIndex;
            backgroundRenderer.transform.localPosition = anchor.localPosition + offset;
        }

        index += isVisible ? 1 : 0;
    }


    //handle deselect, hover while selected, reset/selection memory
    private void AttackClicked(GameObject _)
    {
        if (!IsSelected)
        {
            IsSelected = true;
            EventManager.RaiseEventObject(EventTypes.AttackSelected, ComputedAttackDetail);
        }
        else
        {
            IsSelected = false;
            EventManager.RaiseEventObject(EventTypes.AttackDeselected, ComputedAttackDetail);
            hoverLayerRenderer.enabled = false;
        }
    }

    private void AttackHoveredEnd(GameObject _)
    {
        EventManager.RaiseEventObject(EventTypes.AttackHoverEnded, ComputedAttackDetail);
        hoverLayerRenderer.enabled = IsSelected; 
    }

    private void AttackHovered(GameObject _)
    {
        EventManager.RaiseEventObject(EventTypes.AttackHovered, ComputedAttackDetail);
        hoverLayerRenderer.enabled = true;
    }

    private void DeselectAttackAndNotifySideExausted(object attackApplied)
    {
        if(ComputedAttackDetail == attackApplied)
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
