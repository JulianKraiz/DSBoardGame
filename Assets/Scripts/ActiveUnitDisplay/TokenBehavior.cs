using Assets.Scripts.Tile.Model;
using UnityEngine;

public class TokenBehavior : MonoBehaviour
{
    public TokenType tokenType;
    private PlayerProperties unit;

    private MeshRenderer Renderer;
    private MeshCollider Collider;

    private Material onMaterial;
    private Material offMaterial;

    void Start()
    {
        Renderer = GetComponent<MeshRenderer>();
        Collider = GetComponent<MeshCollider>();

        if (tokenType == TokenType.Estus)
        {
            onMaterial = (Material)Resources.Load("Material/tokens/estus_on_material", typeof(Material));
            offMaterial = (Material)Resources.Load("Material/tokens/estus_off_material", typeof(Material));
        }
        else if (tokenType == TokenType.Luck)
        {
            onMaterial = (Material)Resources.Load("Material/tokens/luck_on_material", typeof(Material));
            offMaterial = (Material)Resources.Load("Material/tokens/luck_off_material", typeof(Material));

        }
        else if (tokenType == TokenType.Ability)
        {
            onMaterial = (Material)Resources.Load("Material/tokens/ability_on", typeof(Material));
            offMaterial = (Material)Resources.Load("Material/tokens/ability_off", typeof(Material));
        }
        else if (tokenType == TokenType.Ember)
        {
            onMaterial = (Material)Resources.Load("Material/tokens/ember_material", typeof(Material));
            offMaterial = (Material)Resources.Load("Material/tokens/ember_material", typeof(Material));
        }
    }

    public void SetUnit(UnitBasicProperties unitBasic)
    {
        if(unitBasic != null && unitBasic is PlayerProperties)
        {
            unit = (PlayerProperties)unitBasic;
        }
        else
        {
            unit = null;
        }
    }

    void Update()
    {
        
        if(unit != null)
        {
            Renderer.enabled = true;
            Collider.enabled = true;

            if (tokenType == TokenType.Ability)
            {
                Renderer.material = unit.hasAbility ? onMaterial : offMaterial;
            }
            else if (tokenType == TokenType.Luck)
            {
                Renderer.material = unit.hasLuckToken ? onMaterial : offMaterial;
            }
            else if (tokenType == TokenType.Estus)
            {
                Renderer.material = unit.hasEstus ? onMaterial : offMaterial;
            }
            else if (tokenType == TokenType.Ember)
            {
                Renderer.material = unit.hasEmber ? onMaterial : offMaterial;
                Renderer.enabled = unit.hasEmber;
                Collider.enabled = unit.hasEmber;
            }
        }
        else
        {
            Renderer.enabled = false;
            Collider.enabled = false;
        }
    }

    private void OnMouseUp()
    {
        if(tokenType == TokenType.Estus)
        {
            if (unit.isActive && unit.hasEstus)
            {
                unit.ResetStaminaAndInjuries();
                unit.hasEstus = false;
            }
        }
    }
}
