using BoardGame.Script.Events;
using System;
using UnityEngine;

public class HoverBrillanceBehavior : MonoBehaviour
{
    public delegate void PositionMouseEvent(GameObject position);
    public event PositionMouseEvent PositionSelected;

    public Material standardBrillanceMaterial;
    public Material hoverBrillanceMaterial;

    private MeshRenderer capsuleRenderer;
    private CapsuleCollider capsuleCollider;

    void Start()
    {
        capsuleRenderer = GetComponent<MeshRenderer>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        capsuleRenderer.material = standardBrillanceMaterial;
        setState(false);
    }

    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        PositionSelected?.Invoke(gameObject);
    }

    private void OnMouseEnter()
    {
        capsuleRenderer.material = hoverBrillanceMaterial;
    }

    private void OnMouseExit()
    {
        capsuleRenderer.material = standardBrillanceMaterial;
    }

    private void OnMouseUp()
    {
        EventManager.RaiseEventGameObject(EventTypes.AttackTargetSelected, transform.parent.gameObject);
    }

    internal void Activate()
    {
        setState(true);
        OnMouseExit();
    }

    internal void Deactivate()
    {
        setState(false);
        OnMouseExit();
    }
    private void setState(bool state)
    {
        capsuleRenderer.enabled = state;
        capsuleCollider.enabled = state;
    }
}
