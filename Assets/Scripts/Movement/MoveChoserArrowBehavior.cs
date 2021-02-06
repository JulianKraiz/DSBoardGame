using UnityEngine;

public class MoveChoserArrowBehavior : MonoBehaviour
{
    private MeshRenderer arrowRenderer;
    private MeshCollider arrowCollider;


    private Material default_arrow;
    private Material highlighted_arrow;

    public delegate void PositionEvent(PositionBehavior position);
    public event PositionEvent PositionClicked;

    public PositionBehavior TargetPosition { get; set; }

    void Start()
    {
        arrowRenderer = GetComponent<MeshRenderer>();
        arrowCollider = GetComponent<MeshCollider>();
        default_arrow = (Material)Resources.Load("Material/attackProperties/arrow", typeof(Material));
        highlighted_arrow = (Material)Resources.Load("Material/attackProperties/arrow_highlighted", typeof(Material));
    }

    public void SetTarget(GameObject positionObject)
    {
        TargetPosition = positionObject != null ? positionObject.GetComponent<PositionBehavior>() : null;
    }

    void Update()
    {
        arrowRenderer.enabled = TargetPosition != null;
        arrowCollider.enabled = TargetPosition != null;
    }

    private void OnMouseEnter()
    {
        arrowRenderer.material = highlighted_arrow;
    }

    private void OnMouseExit()
    {
        arrowRenderer.material = default_arrow;
    }

    private void OnMouseUp()
    {
        if(PositionClicked != null)
        {
            PositionClicked(TargetPosition);
        }
    }
}
