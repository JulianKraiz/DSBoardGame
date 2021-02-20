using BoardGame.Script.Events;
using System;
using UnityEngine;

public class HoverBrillanceBehavior : MonoBehaviour
{
    public delegate void PositionMouseEvent(GameObject position);

    public Material activeMaterial;
    public Material targetableMaterial;
    public Material hoveredMaterial;
    public Material attackedMaterial;

    private bool isUnitActive;
    private bool isTargetable;
    private bool isHovered;
    private bool isAttacked;
    public MeshRenderer capsuleRenderer;

    void Start()
    {
        capsuleRenderer.material = targetableMaterial;
        isUnitActive = false;
        isTargetable = false;
        isHovered = false;
        isAttacked = false;
        SetCircle();

        EventManager.StartListening(GameObjectEventType.UnitHoverEntered, DisplayUnitHovered);
        EventManager.StartListening(GameObjectEventType.UnitHoverExited, DisplayUnitHoverEnded);
    }

    private void DisplayUnitHoverEnded(GameObject unit)
    {
        if (unit == transform.parent.gameObject)
        {
            isHovered = false;
        }
        SetCircle();
    }

    private void DisplayUnitHovered(GameObject unit)
    {
        isHovered = unit == transform.parent.gameObject;
        SetCircle();
    }

    void Update()
    {
        transform.Rotate(Vector3.up, 10 * Time.deltaTime);
    }



    private void OnMouseEnter()
    {
        EventManager.RaiseEvent(GameObjectEventType.UnitHoverEntered, transform.parent.gameObject);
    }

    private void OnMouseExit()
    {
        EventManager.RaiseEvent(GameObjectEventType.UnitHoverExited, transform.parent.gameObject);
    }

    private void OnMouseUp()
    {
        if(isAttacked)
        {
            EventManager.RaiseEvent(GameObjectEventType.ZoomUnitDisplay, transform.parent.gameObject);
        }
        else if (isTargetable)
        {
            EventManager.RaiseEvent(GameObjectEventType.UnitSelected, transform.parent.gameObject);
        }
        else
        {
            EventManager.RaiseEvent(GameObjectEventType.ZoomUnitDisplay, transform.parent.gameObject);
        }
    }

    internal void ShowAttacked()
    {
        isAttacked = true;
        SetCircle();
    }

    internal void HideAttacked()
    {
        isAttacked = false;
        SetCircle();
    }

    internal void ShowTargetable()
    {
        isTargetable = true;
        SetCircle();
    }

    internal void HideTargetable()
    {
        isTargetable = false;
        SetCircle();
    }

    internal void ShowActiveUnit()
    {
        isUnitActive = true;
        SetCircle();
    }

    internal void HideActiveUnit()
    {
        isUnitActive = false;
        SetCircle();
    }

    private void SetCircle()
    {
        var material = GetMaterial();
        if (material != null)
        {
            capsuleRenderer.material = material;
        }
        capsuleRenderer.enabled = material != null;
    }

    private Material GetMaterial()
    {
        if(isAttacked)
        {
            return attackedMaterial;
        }
        else if (isHovered)
        {
            return hoveredMaterial;
        }
        else if (isTargetable)
        {
            return targetableMaterial;
        }
        else if (isUnitActive)
        {
            return activeMaterial;
        }
        return null;
    }
}
