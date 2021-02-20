using System.Collections.Generic;
using UnityEngine;
using BoardGame.Script.Events;
using System;

public class PlayerDisplayBehavior : MonoBehaviour
{
    private Vector3 translateOffset;
    private Vector3 smallScaleOffset;
    private Vector3 bigScaleOffset;

    private Vector3 translateOffsetEnterPlane;
    private Vector3 equipementScaleOffset;

    private PlayerProperties playerProperties;

    private List<MeshRenderer> injuriesToken;
    private List<MeshRenderer> staminaToken;

    public TokenBehavior estusTokenBehavior;
    public TokenBehavior luckTokenBehavior;
    public TokenBehavior emberTokenBehavior;
    public TokenBehavior abilityTokenBehavior;

    public GameObject leftHandAnchor;
    public GameObject rightHandAnchor;
    public GameObject sideAnchor;
    public GameObject armourAnchor;

    public GameObject bleedToken;
    public GameObject poisonToken;
    public GameObject staggerToken;
    public GameObject frozenToken;

    public GameObject aggroToken;
    public GameObject activationToken;

    private GameObject leftEquipementCopy;
    private GameObject rightEquipementCopy;
    private GameObject sideEquipementCopy;
    private GameObject armourEquipementCopy;

    private GameObject enterCollisionPlane;
    private RaiseEventOnClicked enterColisionPlaneBehavior;

    private MeshRenderer focusedLayerRenderer;

    private int displayIndex;
    private bool zoomedIn;
    void Start()
    {
        zoomedIn = false;
        translateOffsetEnterPlane = new Vector3(0f, 0.0f, 0f);
        translateOffset = new Vector3(10f, 0.2f, 0f);
        translateOffset = translateOffset + (displayIndex == 1 || displayIndex == 0 ? 
            new Vector3(0f, 0f, -8f) 
            : new Vector3(0f, 0f, 6.7f));

        enterCollisionPlane = transform.Find("EnterColisionPlane").gameObject;
        enterColisionPlaneBehavior = enterCollisionPlane.GetComponent<RaiseEventOnClicked>();
        enterColisionPlaneBehavior.PositionClicked += EmitPlayerSheetClicked;

        abilityTokenBehavior.gameObject.GetComponent<RaiseEventOnClicked>().PositionClicked += UseAbilityEvent;

        smallScaleOffset = transform.localScale;
        bigScaleOffset = 3 * smallScaleOffset;

        equipementScaleOffset = new Vector3(0.15f, 1f, 0.25f);

        focusedLayerRenderer = transform.Find("FocusedLayer").GetComponent<MeshRenderer>();

        EventManager.StartListening(GameObjectEventType.PlayerSheetClicked, ZoomToggle);
        EventManager.StartListening(GameObjectEventType.ZoomUnitDisplay, ToggleZoomUnitDisplay);
        EventManager.StartListening(GameObjectEventType.UnitHoverEntered, DisplayFocusedOverlay);
        EventManager.StartListening(GameObjectEventType.UnitHoverExited, HideFocusedOverlay);
    }

    void Update()
    {
        if (playerProperties != null)
        {
            for (int i = 0; i < injuriesToken.Count; i++)
            {
                injuriesToken[i].enabled = i < playerProperties.injuries;
            }
            for (int i = 0; i < staminaToken.Count; i++)
            {
                staminaToken[i].enabled = i < playerProperties.stamina;
            }

            leftEquipementCopy = CopyAndPlaceEquipement(playerProperties.leftEquipement, leftHandAnchor, leftEquipementCopy);
            rightEquipementCopy = CopyAndPlaceEquipement(playerProperties.rightEquipement, rightHandAnchor, rightEquipementCopy);
            sideEquipementCopy = CopyAndPlaceEquipement(playerProperties.sideEquipement, sideAnchor, sideEquipementCopy);
            armourEquipementCopy = CopyAndPlaceEquipement(playerProperties.armourEquipement, armourAnchor, armourEquipementCopy);

            bleedToken.SetActive(playerProperties.isBleeding);
            poisonToken.SetActive(playerProperties.isPoisoned);
            staggerToken.SetActive(playerProperties.isStaggered);
            frozenToken.SetActive(playerProperties.isFrozen);

            aggroToken.SetActive(playerProperties.hasAggroToken);
            activationToken.SetActive(playerProperties.hasActivationToken);
        }
    }

    private GameObject CopyAndPlaceEquipement(GameObject playerEquipement, GameObject anchor, GameObject copy)
    {
        if (playerEquipement != null && (copy == null || playerEquipement?.name != copy?.name))
        {
            copy = Instantiate(playerEquipement, transform);
            copy.name = playerEquipement.name;
            copy.transform.localScale = equipementScaleOffset;
            copy.transform.position = anchor.transform.position;
            copy.transform.rotation = transform.rotation; 
            copy.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        else if(playerEquipement == null && copy != null)
        {
            Destroy(copy);
        }

        return copy;
    }

    public void SetUnit(GameObject unit, int index)
    {
        if (unit != null)
        {
            playerProperties = unit.GetComponent<PlayerProperties>();
            SetBackgroundMaterial();
            InitializeDisplay(index);
        }
    }

    private void InitializeDisplay(int index)
    {
        displayIndex = index;

        var injuriesParent = transform.Find("InjuriesToken");
        injuriesToken = new List<MeshRenderer>();
        for (int i = 0; i < injuriesParent.childCount; i++)
        {
            injuriesToken.Add(injuriesParent.GetChild(i).GetComponent<MeshRenderer>());
        }

        var staminaParent = transform.Find("StaminaToken");
        staminaToken = new List<MeshRenderer>();
        for (int i = 0; i < staminaParent.childCount; i++)
        {
            staminaToken.Add(staminaParent.GetChild(i).GetComponent<MeshRenderer>());
        }

        estusTokenBehavior.SetUnit(playerProperties);
        luckTokenBehavior.SetUnit(playerProperties);
        emberTokenBehavior.SetUnit(playerProperties);
        abilityTokenBehavior.SetUnit(playerProperties);
    }

    private void SetBackgroundMaterial()
    {
        Material mat = playerProperties.tile;
        var plane = transform.Find("BackgroundTile");
        var rend = plane.GetComponent<MeshRenderer>();
        rend.material = mat;
    }

    private void EmitPlayerSheetClicked(GameObject source)
    {
        EventManager.RaiseEvent(GameObjectEventType.PlayerSheetClicked, gameObject);
    }

    private void ZoomToggle(GameObject args)
    {
        if (!zoomedIn && args == gameObject)
        {
            enterCollisionPlane.transform.localPosition += translateOffsetEnterPlane;
            transform.localPosition += translateOffset;
            transform.localScale = bigScaleOffset;
            zoomedIn = !zoomedIn;
        }
        else if (zoomedIn)
        {
            enterCollisionPlane.transform.localPosition -= translateOffsetEnterPlane;
            transform.localPosition -= translateOffset;
            transform.localScale = smallScaleOffset;
            zoomedIn = !zoomedIn;
        }
    }

    private void DisplayFocusedOverlay(GameObject unit)
    {
        if (unit == playerProperties.gameObject)
        {
            focusedLayerRenderer.enabled = true;
        }
    }

    private void HideFocusedOverlay(GameObject unit)
    {
        if (unit == playerProperties.gameObject)
        {
            focusedLayerRenderer.enabled = false;
        }
    }

    private void ToggleZoomUnitDisplay(GameObject unit)
    {
        if(playerProperties.gameObject == unit)
        {
            ZoomToggle(gameObject);
        }
        else
        {
            ZoomToggle(null);
        }
    }

    private void UseAbilityEvent(GameObject position)
    {
        if(playerProperties.isActive)
        {

        }
    }
}
