using UnityEngine;
using BoardGame.Script.MovementFunctions;
using BoardGame.Script.Events;

public class HoverFocusedTile : MonoBehaviour
{
    public Vector3 offsetCenterHover = new Vector3(-0.5f, 7, -7f);

    TileManager tileProperties;
    Camera mainCamera;

    private void Awake()
    {
        tileProperties = gameObject.GetComponent<TileManager>();
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (tileProperties.isFocused && !tileProperties.isHovered)
        {
            var targetPosition = transform.position + offsetCenterHover;
            var lerpedProsition = Vect3LerpSnap.SnapLerp(mainCamera.transform.position, targetPosition, 0.9f, 0.2f);

            if (lerpedProsition == targetPosition)
            {
                tileProperties.isHovered = true;
                EventManager.RaiseEvent(GameObjectEventType.TileIsEntered);
            }

            mainCamera.transform.SetPositionAndRotation(lerpedProsition, Quaternion.Euler(45, 0, 0));
        }
    }
}
