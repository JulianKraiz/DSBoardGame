using Assets.Scripts.ActiveUnitDisplay;
using Assets.Scripts.Unit;
using BoardGame.Script.Events;
using UnityEngine;

public class AttackRadialDisplayBehavior : MonoBehaviour
{
    private Transform lowRowAnchor;
    private Transform highRowAnchor;

    public Transform anchor;
    public MeshRenderer radialBackgroundRenderer;
    public MeshRenderer radialHoverBackgroundRenderer;

    private Vector3 propertyPositionOffsetPerIndex;
    private Vector3 propertyTextPositionOffset;

    private Material fade_green_material;
    private Material fade_red_material;

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

    private MeshRenderer BlackDiceBackgroundRenderer;
    private MeshRenderer BlueDiceBackgroundRenderer;
    private MeshRenderer OrangeDiceBackgroundRenderer;
    private MeshRenderer MagicAttackBackgroundRenderer;
    private MeshRenderer SplashNodeBackgroundRenderer;
    private MeshRenderer RangeBackgroundRenderer;
    private MeshRenderer InifiniteRangeBackgroundRenderer;
    private MeshRenderer MinRange1BackgroundRenderer;

    private UnitBasicProperties unitProperties;
    private AttackDetail unitAttack;
    private AttackRadialDetail attackRadialDetail;

    public void Initialize(Transform anchorTransform, GameObject backgroundRadial, GameObject hoverRadial)
    {
        lowRowAnchor = transform.Find("LowRowAnchor");
        highRowAnchor = transform.Find("HighRowAnchor");
        propertyPositionOffsetPerIndex = new Vector3(1, 0, 0);
        propertyTextPositionOffset = new Vector3(-0.15f, 0, 0.32f);

        anchor = anchorTransform;
        radialBackgroundRenderer = backgroundRadial.GetComponent<MeshRenderer>();
        radialHoverBackgroundRenderer = hoverRadial.GetComponent<MeshRenderer>();

        fade_green_material = (Material)Resources.Load("Material/PlainColors/fade_green_material", typeof(Material));
        fade_red_material = (Material)Resources.Load("Material/PlainColors/fade_red_material", typeof(Material));

        StaminaTextMesh = transform.Find("StaminaCostText").GetComponent<TextMesh>();
        BlackDiceTextMesh = transform.Find("BlackDiceText").GetComponent<TextMesh>();
        BlueDiceTextMesh = transform.Find("BlueDiceText").GetComponent<TextMesh>();
        OrangeDiceTextMesh = transform.Find("OrangeDiceText").GetComponent<TextMesh>();
        RangeTextMesh = transform.Find("RangeText").GetComponent<TextMesh>();
        FlatBonusTextMesh = transform.Find("FlatBonus").GetComponent<TextMesh>();

        StaminaTextRenderer = transform.Find("StaminaCostText").GetComponent<MeshRenderer>();
        BlackDiceTextRenderer = transform.Find("BlackDiceText").GetComponent<MeshRenderer>();
        BlueDiceTextRenderer = transform.Find("BlueDiceText").GetComponent<MeshRenderer>();
        OrangeDiceTextRenderer = transform.Find("OrangeDiceText").GetComponent<MeshRenderer>();
        RangeTextRenderer = transform.Find("RangeText").GetComponent<MeshRenderer>();
        FlatBonusTextRenderer = transform.Find("FlatBonus").GetComponent<MeshRenderer>();

        BlackDiceBackgroundRenderer = transform.Find("BlackDices").GetComponent<MeshRenderer>(); ;
        BlueDiceBackgroundRenderer = transform.Find("BlueDices").GetComponent<MeshRenderer>(); ;
        OrangeDiceBackgroundRenderer = transform.Find("OrangeDices").GetComponent<MeshRenderer>(); ;
        MagicAttackBackgroundRenderer = transform.Find("MagicAttack").GetComponent<MeshRenderer>();
        SplashNodeBackgroundRenderer = transform.Find("SplashNode").GetComponent<MeshRenderer>();
        MinRange1BackgroundRenderer = transform.Find("MinRange1").GetComponent<MeshRenderer>();
        RangeBackgroundRenderer = transform.Find("Range").GetComponent<MeshRenderer>();
        InifiniteRangeBackgroundRenderer = transform.Find("InfiniteRange").GetComponent<MeshRenderer>();

        EventManager.StartListeningGameObject(EventTypes.ActiveUnitMoved, CheckEnoughStaminaLeft);
        hoverRadial.GetComponent<RaiseEventOnClicked>().PositionClicked += AttackClicked;
        hoverRadial.GetComponent<RaiseEventOnEnterExit>().PositionEnter += AttackHovered;
        hoverRadial.GetComponent<RaiseEventOnEnterExit>().PositionExit += AttackHoveredEnd;
    }

    private void Start()
    {
        transform.position = anchor.position;
    }

    void Update()
    {
    }

    public void SetupTile(AttackDetail attack, UnitBasicProperties properties)
    {
        unitProperties = properties;
        unitAttack = attack;
        CreateAttackDetail(unitAttack, unitProperties);
        SetupAttack(attackRadialDetail);
    }

    private void CheckEnoughStaminaLeft(GameObject unit)
    {
        unitProperties = unit.GetComponent<UnitBasicProperties>();
        CreateAttackDetail(unitAttack, unitProperties);
        SetupAttack(attackRadialDetail);
    }

    private void SetupAttack(AttackRadialDetail detail)
    {
        var lowRowIndex = 0;
        SetTextOrHide(lowRowAnchor, detail.blackAttackDices, BlackDiceTextMesh, BlackDiceTextRenderer, BlackDiceBackgroundRenderer, ref lowRowIndex);
        SetTextOrHide(lowRowAnchor, detail.blueAttackDices, BlueDiceTextMesh, BlueDiceTextRenderer, BlueDiceBackgroundRenderer, ref lowRowIndex);
        SetTextOrHide(lowRowAnchor, detail.orangeAttackDices, OrangeDiceTextMesh, OrangeDiceTextRenderer, OrangeDiceBackgroundRenderer, ref lowRowIndex);
        SetTextOrHide(lowRowAnchor, detail.flatModifier, FlatBonusTextMesh, FlatBonusTextRenderer, null, ref lowRowIndex);

        var highRowIndex = 0;
        SetTextOrHide(highRowAnchor, detail.range, RangeTextMesh, RangeTextRenderer, RangeBackgroundRenderer, ref highRowIndex, !detail.infiniteRange);
        SetTextOrHide(highRowAnchor, detail.infiniteRange ? 1 : 0, null, null, InifiniteRangeBackgroundRenderer, ref highRowIndex);
        SetTextOrHide(highRowAnchor, detail.minimumRange, null, null, MinRange1BackgroundRenderer, ref highRowIndex);
        SetTextOrHide(highRowAnchor, detail.magicAttack ? 1 : 0, null, null, MagicAttackBackgroundRenderer, ref highRowIndex);
        SetTextOrHide(highRowAnchor, detail.nodeSplash ? 1 : 0, null, null, SplashNodeBackgroundRenderer, ref highRowIndex);

        radialHoverBackgroundRenderer.material = detail.notEnoughStamina ? fade_red_material : fade_green_material;
        radialHoverBackgroundRenderer.enabled = false;

        radialBackgroundRenderer.gameObject.SetActive(true);
        radialHoverBackgroundRenderer.gameObject.SetActive(true);

        StaminaTextMesh.text = $"[{detail.staminaCost}]";
        RangeTextMesh.text = detail.range.ToString();
    }

    private void CreateAttackDetail(AttackDetail attack, UnitBasicProperties unitProperties)
    {
        attackRadialDetail = new AttackRadialDetail();
        attackRadialDetail.blackAttackDices = attack.blackAttackDices;
        attackRadialDetail.blueAttackDices = attack.blueAttackDices;
        attackRadialDetail.orangeAttackDices = attack.orangeAttackDices;
        attackRadialDetail.nodeSplash = attack.nodeSplash;
        attackRadialDetail.range = attack.range;
        attackRadialDetail.infiniteRange = attack.infiniteRange;
        attackRadialDetail.minimumRange = attack.minimumRange;
        attackRadialDetail.magicAttack = attack.magicAttack;
        attackRadialDetail.staminaCost = attack.staminaCost;
        attackRadialDetail.notEnoughStamina = unitProperties.StaminaLeft() - attack.staminaCost <= 0;
        attackRadialDetail.flatModifier = attack.flatModifier;
        attackRadialDetail.targetPlayers = attack.targetPlayers;
    }

    private void SetTextOrHide(Transform anchor, int value, TextMesh textMesh, MeshRenderer textRenderer, MeshRenderer backgroundRenderer, ref int index, bool showAnyValue = false)
    {
        var isVisible = value != 0 || showAnyValue;

        if (textRenderer != null)
        {
            textRenderer.enabled = isVisible;
            textMesh.text = value.ToString();

            var offset = index * propertyPositionOffsetPerIndex + propertyTextPositionOffset;
            offset.Scale(transform.localScale);

            textRenderer.transform.position = anchor.position + offset;
        }

        if (backgroundRenderer != null)
        {
            backgroundRenderer.enabled = isVisible;

            var offset = index * propertyPositionOffsetPerIndex;
            offset.Scale(transform.localScale);

            backgroundRenderer.transform.position = anchor.position + offset;
        }

        index += isVisible ? 1 : 0;
    }

    private void AttackClicked(GameObject _)
    {
        EventManager.RaiseEventGameObject(EventTypes.CloseAttackDial);
    }

    private void AttackHoveredEnd(GameObject _)
    {
        EventManager.RaiseEventObject(EventTypes.AttackDeselected, attackRadialDetail);
        radialHoverBackgroundRenderer.enabled = false;
    }

    private void AttackHovered(GameObject _)
    {
        EventManager.RaiseEventObject(EventTypes.AttackSelected, attackRadialDetail);
        radialHoverBackgroundRenderer.enabled = true;
    }
}
