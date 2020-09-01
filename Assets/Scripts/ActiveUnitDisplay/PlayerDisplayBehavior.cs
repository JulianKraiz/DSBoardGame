using System.Collections.Generic;
using UnityEngine;
using BoardGame.Unit;
using System.Linq;

public class PlayerDisplayBehavior : MonoBehaviour
{
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

    void Start()
    {
        cardAngle = Quaternion.Euler(45, -180, 0);

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
    }

    public void Initialize()
    {
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

    // Update is called once per frame
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

            if (playerProperties.leftEquipement != null)
            {
                playerProperties.leftEquipement.transform.position = leftHandAnchor.transform.position;
                playerProperties.leftEquipement.transform.rotation = cardAngle;
            }
            if (playerProperties.rightEquipement != null)
            {
                playerProperties.rightEquipement.transform.position = rightHandAnchor.transform.position;
                playerProperties.rightEquipement.transform.rotation = cardAngle;
            }
            if (playerProperties.sideEquipement != null)
            {
                playerProperties.sideEquipement.transform.position = sideAnchor.transform.position;
                playerProperties.sideEquipement.transform.rotation = cardAngle;
            }
            if (playerProperties.armourEquipement != null)
            {
                playerProperties.armourEquipement.transform.position = armourAnchor.transform.position;
                playerProperties.armourEquipement.transform.rotation = cardAngle;
            }
        }
    }

    public void SetUnit(GameObject unit)
    {
        if (unit != null)
        {
            playerProperties = unit.GetComponent<PlayerProperties>();
            SetBackgroundMaterial();
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
}
