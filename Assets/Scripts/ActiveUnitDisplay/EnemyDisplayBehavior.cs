using UnityEngine;
using BoardGame.Unit;
using BoardGame.Script.Events;

public class EnemyDisplayBehavior : MonoBehaviour
{
    private EnemyProperties enemyProperties;
    private GameObject enemy;

    private MeshRenderer focusedLayerRenderer;

    public Vector3 translateOffset;
    public Vector3 smallScaleOffset;
    public Vector3 bigScaleOffset;

    void Start()
    {
        translateOffset = new Vector3(0f, -1.4f, 0f);
        smallScaleOffset = new Vector3(0.1f, 1f, 0.15f);
        bigScaleOffset = new Vector3(0.3f, 1f, 0.4f);
        EventManager.StartListeningGameObject(EventTypes.UnitHoverEntered, UnitHovered);
        EventManager.StartListeningGameObject(EventTypes.UnitHoverExited, UnitExited);

        focusedLayerRenderer = transform.Find("FocusedLayer").GetComponent<MeshRenderer>();
    }

    public void Initialize()
    {
       
    }

    void Update()
    {
    }

    public void SetUnit(GameObject unit)
    {
        if (unit != null)
        {
            enemy = unit;
            enemyProperties = unit.GetComponent<EnemyProperties>();
            SetBackgroundMaterial();
        }
    }

    public void OnMouseEnter()
    {
        transform.localPosition += translateOffset;
        transform.localScale = bigScaleOffset;
    }
    public void OnMouseExit()
    {
        transform.localPosition -= translateOffset;
        transform.localScale = smallScaleOffset;
    }

    private void UnitHovered(GameObject unit)
    {
        if(unit == enemy)
        {
            focusedLayerRenderer.enabled = true;
        }
    }

    private void UnitExited(GameObject unit)
    {
        if (unit == enemy)
        {
            focusedLayerRenderer.enabled = false;
        }
    }

    private void SetBackgroundMaterial()
    {
        Material mat = null;
        if (enemyProperties.enemyType == EnemyClassEnum.HollowSoldier)
        {
            mat = (Material)Resources.Load("Material/UnitBackground/hollow_arbalest_soldier_tile_material", typeof(Material));
        }
        else if (enemyProperties.enemyType == EnemyClassEnum.ArbalestHollowSoldier)
        {
            mat = (Material)Resources.Load("Material/UnitBackground/hollow_soldier_tile_material", typeof(Material));
        }
        else if (enemyProperties.enemyType == EnemyClassEnum.LargeHollowSoldier)
        {
            mat = (Material)Resources.Load("Material/UnitBackground/hollow_large_soldier_tile_material", typeof(Material));
        }
        else if (enemyProperties.enemyType == EnemyClassEnum.Sentinel)
        {
            mat = (Material)Resources.Load("Material/UnitBackground/sentinel_tile_material", typeof(Material));
        }
        else if (enemyProperties.enemyType == EnemyClassEnum.SilverKnightBowman)
        {
            mat = (Material)Resources.Load("Material/UnitBackground/silver_knight_great_bowman_tile_material", typeof(Material));
        }
        else if (enemyProperties.enemyType == EnemyClassEnum.SilverKnightSwordman)
        {
            mat = (Material)Resources.Load("Material/UnitBackground/silver_knight_swordman_tile_material", typeof(Material));
        }

        var rend = GetComponent<MeshRenderer>();
        rend.material = mat;

       
    }
}
