using UnityEngine;
using BoardGame.Unit;
using BoardGame.Script.Events;

public class EnemyDisplayBehavior : MonoBehaviour
{
    private EnemyProperties enemyProperties;
    private GameObject enemy;

    private MeshRenderer focusedLayerRenderer;

    private Vector3 translateOffset;
    private Vector3 smallScale;
    private Vector3 bigScale;

    private bool zoomedIn;

    void Start()
    {
        zoomedIn = false;
        translateOffset = new Vector3(-10f, .5f, -10f);
        smallScale = transform.localScale;
        bigScale = 3 * smallScale;
        EventManager.StartListeningGameObject(EventTypes.UnitHoverEntered, UnitHovered);
        EventManager.StartListeningGameObject(EventTypes.UnitHoverExited, UnitExited);
        EventManager.StartListeningGameObject(EventTypes.ToggleZoomUnitDisplay, ToggleZoomUnitDisplay);

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

    public void OnMouseExit()
    {
        if (zoomedIn)
        {
            zoomedIn = !zoomedIn;
            transform.localPosition -= translateOffset;
            transform.localScale = smallScale;
        }
    }

    public void OnMouseUp()
    {
        if (zoomedIn)
        {
            zoomedIn = !zoomedIn;
            transform.localPosition -= translateOffset;
            transform.localScale = smallScale;
        }
        else if (!zoomedIn)
        {
            zoomedIn = !zoomedIn;
            transform.localPosition += translateOffset;
            transform.localScale = bigScale;
        }
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

    private void ToggleZoomUnitDisplay(GameObject unit)
    {
        if (enemy == unit)
        {
            if (focusedLayerRenderer.enabled)
            {
                UnitExited(unit);
            }
            else 
            {
                UnitHovered(unit);
            }
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
