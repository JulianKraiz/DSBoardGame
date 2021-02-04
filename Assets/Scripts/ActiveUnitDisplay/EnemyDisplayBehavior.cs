﻿using UnityEngine;
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
        Material mat = enemyProperties.tile;
        var rend = GetComponent<MeshRenderer>();
        rend.material = mat;
    }


}
