using Assets.Scripts.Tile;
using BoardGame.Script.Events;
using UnityEngine;

public class NavigateFog : MonoBehaviour
{
    public GameObject tileNorth = null;
    public GameObject tileSouth = null;
    public GameObject tileEast = null;
    public GameObject tileWest = null;

    TileManager tileNorthProperties;
    TileManager tileSouthProperties;
    TileManager tileEastProperties;
    TileManager tileWestProperties;

    public void Intialize()
    {
        tileNorthProperties = tileNorth?.GetComponent<TileManager>();
        tileSouthProperties = tileSouth?.GetComponent<TileManager>();
        tileEastProperties = tileEast?.GetComponent<TileManager>();
        tileWestProperties = tileWest?.GetComponent<TileManager>();

    }

    void Start()
    {

    }

    void Update()
    {
    }

    void OnMouseDown()
    {
        if ((
            ((tileEastProperties?.isFocused).GetValueOrDefault() || (tileWestProperties?.isFocused).GetValueOrDefault()) 
            && ((tileEastProperties?.isCleared).GetValueOrDefault() || (tileWestProperties?.isCleared).GetValueOrDefault()))
            || 
            (((tileNorthProperties?.isFocused).GetValueOrDefault() || (tileSouthProperties?.isFocused).GetValueOrDefault()) 
            && ((tileNorthProperties?.isCleared).GetValueOrDefault() || (tileSouthProperties?.isCleared).GetValueOrDefault()))
            )
        {
            if (tileWestProperties != null)
            {
                if (tileWestProperties.isFocused)
                {
                    tileEastProperties.enteredFrom = "west";
                    tileWestProperties.ExitTile();
                }
                tileWestProperties.isFocused = !tileWestProperties.isFocused;
                tileWestProperties.isHovered = false;
               
            }
            if (tileNorthProperties != null)
            {
                if (tileNorthProperties.isFocused)
                {
                    tileSouthProperties.enteredFrom = "north";
                    tileNorthProperties.ExitTile();
                }
                tileNorthProperties.isFocused = !tileNorthProperties.isFocused;
                tileNorthProperties.isHovered = false;
            }
            if (tileSouthProperties != null)
            {
                if (tileSouthProperties.isFocused)
                {
                    tileNorthProperties.enteredFrom = "south";
                    tileSouthProperties.ExitTile();
                }
                tileSouthProperties.isFocused = !tileSouthProperties.isFocused;
                tileSouthProperties.isHovered = false;
            }
            if (tileEastProperties != null)
            {
                if (tileEastProperties.isFocused)
                {
                    tileWestProperties.enteredFrom = "east";
                    tileEastProperties.ExitTile();
                }
                tileEastProperties.isFocused = !tileEastProperties.isFocused;
                tileEastProperties.isHovered = false;
                
            }
        }

        EventManager.RaiseEvent(GameObjectEventType.TileFocused, null);
    }

    public bool BothTileCleared()
    {
        return (
            ((tileEastProperties?.isCleared).GetValueOrDefault() && (tileWestProperties?.isCleared).GetValueOrDefault())
            ||
            ((tileNorthProperties?.isCleared).GetValueOrDefault() && (tileSouthProperties?.isCleared).GetValueOrDefault())
            );
    }
}
