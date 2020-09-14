using UnityEngine;

public class RaiseEventOnEnterExit : MonoBehaviour
{
    public delegate void PositionMouseEvent(GameObject position);
    public event PositionMouseEvent PositionEnter;
    public event PositionMouseEvent PositionExit;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnMouseEnter()
    {
        if (PositionEnter != null)
        {
            PositionEnter(gameObject);
        }
    }

    private void OnMouseExit()
    {
        if (PositionExit != null)
        {
            PositionExit(gameObject);
        }
    }
}
