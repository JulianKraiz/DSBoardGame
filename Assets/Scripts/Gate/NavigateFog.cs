using System.Linq;
using Assets.Scripts.Tile;
using BoardGame.Script.Events;
using UnityEngine;

public class NavigateFog : MonoBehaviour
{
    public GameObject tileNorth = null;
    public GameObject tileSouth = null;
    public GameObject tileEast = null;
    public GameObject tileWest = null;

    public GameObject AggroSelectionCanvas;
    public MeshRenderer Portrait1;
    public MeshRenderer Portrait2;
    public MeshRenderer Portrait3;
    public MeshRenderer Portrait4;

    private UnitBasicProperties Char1;
    private UnitBasicProperties Char2;
    private UnitBasicProperties Char3;
    private UnitBasicProperties Char4;

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
        AggroSelectionCanvas.SetActive(false);
        AggroSelectionCanvas.GetComponent<Canvas>().worldCamera  = Camera.main;
        AggroSelectionCanvas.GetComponent<Canvas>().planeDistance = 5;

        Portrait1.gameObject.GetComponent<RaiseEventOnClicked>().PositionClicked += Char1Selected;
        Portrait2.gameObject.GetComponent<RaiseEventOnClicked>().PositionClicked += Char2Selected;
        Portrait3.gameObject.GetComponent<RaiseEventOnClicked>().PositionClicked += Char3Selected;
        Portrait4.gameObject.GetComponent<RaiseEventOnClicked>().PositionClicked += Char4Selected;
    }

    void Update()
    {
    }

    private void OnMouseUp()
    {
        TileManager enteredTile = null;
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
                    enteredTile = tileEastProperties;
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
                    enteredTile = tileSouthProperties;
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
                    enteredTile = tileNorthProperties;
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
                    enteredTile = tileWestProperties;
                    tileWestProperties.enteredFrom = "east";
                    tileEastProperties.ExitTile();
                }
                tileEastProperties.isFocused = !tileEastProperties.isFocused;
                tileEastProperties.isHovered = false;
                
            }

            if (!enteredTile.isCleared)
            {
                ShowAggroSeleciton();
            }
            else
            {
                EventManager.RaiseEvent(GameObjectEventType.TileFocused, null);
            }
        }
    }

    public bool BothTileCleared()
    {
        return (
            ((tileEastProperties?.isCleared).GetValueOrDefault() && (tileWestProperties?.isCleared).GetValueOrDefault())
            ||
            ((tileNorthProperties?.isCleared).GetValueOrDefault() && (tileSouthProperties?.isCleared).GetValueOrDefault())
            );
    }

    private void ShowAggroSeleciton()
    {
        AggroSelectionCanvas.SetActive(true);
        Portrait1.enabled = false;
        Portrait2.enabled = false;
        Portrait3.enabled = false;
        Portrait4.enabled = false;

        var i = 0;
        foreach (var player in GameStateManager.Instance.players.Select(p => p.GetComponent<UnitBasicProperties>()))
        {
            if (i == 0)
            {
                Portrait1.material = player.portrait;
                Portrait1.enabled = true;
                Char1 = player;
                Char1.hasAggroToken = false;
            }
            else if (i == 1)
            {
                Portrait2.material = player.portrait;
                Portrait2.enabled = true;
                Char2 = player;
                Char1.hasAggroToken = false;
            }
            else if (i == 2)
            {
                Portrait3.material = player.portrait;
                Portrait3.enabled = true;
                Char3 = player;
                Char1.hasAggroToken = false;
            }
            else if (i == 3)
            {
                Portrait4.material = player.portrait;
                Portrait4.enabled = true;
                Char4 = player;
                Char1.hasAggroToken = false;
            }

            i++;
        }
    }

    private void HideAggroSeleciton()
    {
        AggroSelectionCanvas.SetActive(false);
        Portrait1.enabled = false;
        Portrait2.enabled = false;
        Portrait3.enabled = false;
        Portrait4.enabled = false;
    }

    private void Char1Selected(GameObject _)
    {
        Char1.hasAggroToken = true;
        HideAggroSeleciton();
        EventManager.RaiseEvent(GameObjectEventType.TileFocused, null);
    }

    private void Char2Selected(GameObject _)
    {
        Char2.hasAggroToken = true;
        HideAggroSeleciton();
        EventManager.RaiseEvent(GameObjectEventType.TileFocused, null);
    }

    private void Char3Selected(GameObject _)
    {
        Char3.hasAggroToken = true;
        HideAggroSeleciton();
        EventManager.RaiseEvent(GameObjectEventType.TileFocused, null);
    }

    private void Char4Selected(GameObject _)
    {
        Char4.hasAggroToken = true;
        HideAggroSeleciton();
        EventManager.RaiseEvent(GameObjectEventType.TileFocused, null);
    }
}
