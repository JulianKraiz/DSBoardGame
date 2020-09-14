using System.Collections.Generic;
using UnityEngine;
using BoardGame.Unit;
using BoardGame.Script.Events;

public class PlayerDisplayBehavior : MonoBehaviour
{

    private Vector3 translateOffset;
    private Vector3 translateOffsetVertical;
    private Vector3 smallScaleOffset;
    private Vector3 bigScaleOffset;

    private Vector3 equipementScaleOffset;

    private Quaternion cardAngle;

    private PlayerProperties playerProperties;

    private List<MeshRenderer> injuriesToken;
    private List<MeshRenderer> staminaToken;

    private MeshRenderer estusTokenRenderer;
    private MeshRenderer luckTokenRenderer;
    private MeshRenderer emberTokenRenderer;
    private MeshRenderer abilityTokenRenderer;

    private Material estusOnMaterial;
    private Material estusOffMaterial;
    private Material luckOnMaterial;
    private Material luckOffMaterial;
    private Material abilityOnMaterial;
    private Material abilityOffMaterial;

    private GameObject leftHandAnchor;
    private GameObject rightHandAnchor;
    private GameObject sideAnchor;
    private GameObject armourAnchor;

    private GameObject leftEquipementCopy;
    private GameObject rightEquipementCopy;
    private GameObject sideEquipementCopy;
    private GameObject armourEquipementCopy;

    private RaiseEventOnEnterExit colisionPlane;
    private MeshRenderer focusedLayerRenderer;

    private int displayIndex;

    void Start()
    {
        colisionPlane = transform.Find("ColisionPlane").GetComponent<RaiseEventOnEnterExit>();
        colisionPlane.PositionEnter += MouseEnter;
        colisionPlane.PositionExit += MouseExit;

        smallScaleOffset = new Vector3(0.65f, 0.65f, 0.65f);
        bigScaleOffset = new Vector3(2.5f, 2.5f, 1f);
        cardAngle = Quaternion.Euler(45, -180, 0);
        equipementScaleOffset = new Vector3(0.04f, 1f, 0.08f);

        estusTokenRenderer = transform.Find("EstusToken").GetComponent<MeshRenderer>();
        luckTokenRenderer = transform.Find("LuckToken").GetComponent<MeshRenderer>();
        abilityTokenRenderer = transform.Find("AbilityToken").GetComponent<MeshRenderer>();
        emberTokenRenderer = transform.Find("EmberToken").GetComponent<MeshRenderer>();

        estusOnMaterial = (Material)Resources.Load("Material/estus_on_material", typeof(Material));
        estusOffMaterial = (Material)Resources.Load("Material/estus_off_material", typeof(Material));
        luckOnMaterial = (Material)Resources.Load("Material/luck_on_material", typeof(Material));
        luckOffMaterial = (Material)Resources.Load("Material/luck_off_material", typeof(Material));
        abilityOnMaterial = (Material)Resources.Load("Material/ability_on_material", typeof(Material));
        abilityOffMaterial = (Material)Resources.Load("Material/ability_off_material", typeof(Material));

        leftHandAnchor = transform.Find("LeftHandAnchor").gameObject;
        rightHandAnchor = transform.Find("RightHandAnchor").gameObject;
        sideAnchor = transform.Find("SideAnchor").gameObject;
        armourAnchor = transform.Find("ArmourAnchor").gameObject;

        focusedLayerRenderer = transform.Find("FocusedLayer").GetComponent<MeshRenderer>();

        EventManager.StartListeningGameObject(EventTypes.UnitHoverEntered, DisplayFocusedOverlay);
        EventManager.StartListeningGameObject(EventTypes.UnitHoverExited, HideFocusedOverlay);
    }

    void Update()
    {
        if (playerProperties != null)
        {
            for (int i = 0; i < injuriesToken.Count; i++)
            {
                injuriesToken[i].enabled = i < playerProperties.injuries;
            }
            for (int i = staminaToken.Count - 1; i >= 0; i--)
            {
                staminaToken[i].enabled = staminaToken.Count - 1 - i < playerProperties.stamina;
            }

            emberTokenRenderer.enabled = playerProperties.hasEmber;
            estusTokenRenderer.material = playerProperties.hasEstus ? estusOnMaterial : estusOffMaterial;
            luckTokenRenderer.material = playerProperties.hasLuckToken ? luckOnMaterial : luckOffMaterial;
            abilityTokenRenderer.material = playerProperties.hasAbility ? abilityOnMaterial : abilityOffMaterial;

            leftEquipementCopy = CopyAndPlaceEquipement(playerProperties.leftEquipement, leftHandAnchor, leftEquipementCopy);
            rightEquipementCopy = CopyAndPlaceEquipement(playerProperties.rightEquipement, rightHandAnchor, rightEquipementCopy);
            sideEquipementCopy = CopyAndPlaceEquipement(playerProperties.sideEquipement, sideAnchor, sideEquipementCopy);
            armourEquipementCopy = CopyAndPlaceEquipement(playerProperties.armourEquipement, armourAnchor, armourEquipementCopy);
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
            copy.transform.rotation = cardAngle;
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

        translateOffset = new Vector3(2.8f, 0f, -0.5f);
        translateOffsetVertical = new Vector3(0f, -2f, 0f);
        // find a less hardcoded way of setting the big pictur translation vertical offset.
        translateOffset = translateOffset + (index == 1 || index == 2 ? 0 : 1) * translateOffsetVertical;

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


    }

    private void SetBackgroundMaterial()
    {
        Material mat = null;
        if (playerProperties.playerType == PlayerClassEnum.Herald)
        {
            mat = (Material)Resources.Load("Material/UnitBackground/herald_tile_material", typeof(Material));
        }
        else if (playerProperties.playerType == PlayerClassEnum.Warrior)
        {
            mat = (Material)Resources.Load("Material/UnitBackground/warrior_tile_material", typeof(Material));
        }
        else if (playerProperties.playerType == PlayerClassEnum.Assassin)
        {
            mat = (Material)Resources.Load("Material/UnitBackground/assassin_tile_material", typeof(Material));
        }
        else if (playerProperties.playerType == PlayerClassEnum.Knight)
        {
            mat = (Material)Resources.Load("Material/UnitBackground/knight_tile_material", typeof(Material));
        }

        var plane = transform.Find("BackgroundTile");
        var rend = plane.GetComponent<MeshRenderer>();
        rend.material = mat;

       
    }

    private void MouseEnter(GameObject args)
    {
        transform.localPosition += translateOffset;
        transform.localScale = bigScaleOffset;
    }
    private void MouseExit(GameObject args)
    {
        transform.localPosition -= translateOffset;
        transform.localScale = smallScaleOffset;
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
}
