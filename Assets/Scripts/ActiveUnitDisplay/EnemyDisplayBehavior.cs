﻿using UnityEngine;
using BoardGame.Script.Events;

public class EnemyDisplayBehavior : MonoBehaviour
{
    private EnemyProperties enemyProperties;
    private GameObject enemy;

    public GameObject bleedToken;
    public GameObject poisonToken;
    public GameObject staggerToken;
    public GameObject frozenToken;

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
        EventManager.StartListening(GameObjectEventType.UnitHoverEntered, UnitHovered);
        EventManager.StartListening(GameObjectEventType.UnitHoverExited, UnitExited);
        EventManager.StartListening(GameObjectEventType.ZoomUnitDisplay, ZoomUnitDisplay);

        focusedLayerRenderer = transform.Find("FocusedLayer").GetComponent<MeshRenderer>();
    }

    public void Initialize()
    {
       
    }

    void Update()
    {
        if(enemyProperties != null)
        {
            bleedToken.SetActive(enemyProperties.isBleeding);
            poisonToken.SetActive(enemyProperties.isPoisoned);
            staggerToken.SetActive(enemyProperties.isStaggered);
            frozenToken.SetActive(enemyProperties.isFrozen);
        }
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
        ToggleZoom();
    }

    private void ToggleZoom()
    {
        if (zoomedIn)
        {
            ZoomOut();
        }
        else if (!zoomedIn)
        {
            ZoomIn();
        }
    }

    private void ZoomOut()
    {
        if (zoomedIn)
        {
            zoomedIn = !zoomedIn;
            transform.localPosition -= translateOffset;
            transform.localScale = smallScale;
        }
    }

    private void ZoomIn()
    {
        if (!zoomedIn)
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

    private void ZoomUnitDisplay(GameObject unit)
    {
        if (enemy == unit)
        {
            ToggleZoom();
        }
        else
        {
            ZoomOut();
        }
    }

    private void SetBackgroundMaterial()
    {
        Material mat = enemyProperties.tile;
        var rend = GetComponent<MeshRenderer>();
        rend.material = mat;
    }
}
