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

    void Start()
    {
        capsuleRenderer = GetComponent<MeshRenderer>();
        capsuleRenderer.material = standardBrillanceMaterial;
        gameObject.SetActive(false);
    }

    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        if (PositionSelected != null)
        {
            PositionSelected(gameObject);
        }
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
        gameObject.SetActive(true);
        OnMouseExit();
    }

    internal void Deactivate()
    {
        gameObject.SetActive(false);
        OnMouseExit();
    }
}
