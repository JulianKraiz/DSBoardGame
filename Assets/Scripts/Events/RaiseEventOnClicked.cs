using UnityEngine;

public class RaiseEventOnClicked : MonoBehaviour
{
    public delegate void PositionMouseEvent(GameObject position);
    public event PositionMouseEvent PositionClicked;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnMouseUp()
    {
        if (PositionClicked != null)
        {
            PositionClicked(gameObject);
        }
    }
}
