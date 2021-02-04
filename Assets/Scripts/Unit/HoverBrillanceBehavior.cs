using BoardGame.Script.Events;
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
        EventManager.RaiseEventGameObject(EventTypes.UnitHoverEntered, transform.parent.gameObject);
    }

    private void OnMouseExit()
    {
        capsuleRenderer.material = standardBrillanceMaterial;
        EventManager.RaiseEventGameObject(EventTypes.UnitHoverExited, transform.parent.gameObject);
    }

    private void OnMouseUp()
    {
        if (capsuleRenderer.enabled)
        {
            EventManager.RaiseEventGameObject(EventTypes.AttackTargetSelected, transform.parent.gameObject);
        }
        else
        {
            EventManager.RaiseEventGameObject(EventTypes.ToggleZoomUnitDisplay, transform.parent.gameObject);
        }
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
    }
}
