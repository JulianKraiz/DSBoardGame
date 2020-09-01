using UnityEngine;

public class DisplayWhenHover : MonoBehaviour
{
    MeshRenderer render = null;
    private bool _canRender;
    // Start is called before the first frame update
    void Start()
    {
        render = gameObject.GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CanRender(bool canRender)
    {
        _canRender = canRender;
    }

    private void OnMouseOver()
    {
        render.enabled = _canRender && true;
    }

    private void OnMouseExit()
    {
        render.enabled = false;
    }


}
