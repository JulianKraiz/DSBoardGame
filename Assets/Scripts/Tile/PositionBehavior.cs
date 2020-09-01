using BoardGame.Script.Events;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PositionBehavior : MonoBehaviour
{
    public delegate void PositionSelectedEvent(GameObject position);
    public event PositionSelectedEvent PositionClicked;

    public GameObject NorthPosition;
    public GameObject NorthEastPosition;
    public GameObject EastPosition;
    public GameObject SouthEastPosition;
    public GameObject SouthPosition;
    public GameObject SouthWestPosition;
    public GameObject WestPosition;
    public GameObject NorthWestPosition;

    public bool isNorth;
    public bool isSouth;
    public bool isWest; 
    public bool isEast;
    public bool isSpawnOne;
    public bool isSpawnTwo;
    public bool isGreenSlot;

    private List<GameObject> NonBossUnits = new List<GameObject>();
    private MeshRenderer render = null;
    private bool _canRender;

    void Start()
    {
        render = gameObject.GetComponent<MeshRenderer>();
    }

    void Update()
    {

    }

    public void AddNonBossUnit(GameObject unit)
    {
        NonBossUnits.Add(unit);
        OrganizeCharacterOnPosition();
    }

    public void RemoveNonBossUnit(GameObject unit)
    {
        NonBossUnits.Remove(unit);
        OrganizeCharacterOnPosition();
    }

    public bool HasUnit(GameObject unit)
    {
        return NonBossUnits.Contains(unit);
    }

    public bool HasMaxUnit()
    {
        return NonBossUnits.Count == 3;
    }

    public bool HasActiveUnit()
    {
        return NonBossUnits.Any(u => u.GetComponent<UnitBasicProperties>().isActive);
    }

    public int GetNodeCost()
    {
        return 1;
    }

    public List<PositionBehavior> GetAdjacentNodes()
    {
        var result = new List<PositionBehavior>();
        if (NorthPosition != null)
            result.Add(NorthPosition.GetComponent<PositionBehavior>());
        if (NorthEastPosition != null)
            result.Add(NorthEastPosition.GetComponent<PositionBehavior>());
        if (EastPosition != null)
            result.Add(EastPosition.GetComponent<PositionBehavior>());
        if (SouthEastPosition != null)
            result.Add(SouthEastPosition.GetComponent<PositionBehavior>());
        if (SouthPosition != null)
            result.Add(SouthPosition.GetComponent<PositionBehavior>());
        if (SouthWestPosition != null)
            result.Add(SouthWestPosition.GetComponent<PositionBehavior>());
        if (WestPosition != null)
            result.Add(WestPosition.GetComponent<PositionBehavior>());
        if (NorthWestPosition != null)
            result.Add(NorthWestPosition.GetComponent<PositionBehavior>());
        return result;
    }

    public void ResetPosition(bool canRender)
    {
        NonBossUnits.Clear();
        _canRender = canRender;
        OrganizeCharacterOnPosition();
    }

    private void OnMouseDown()
    {
        PositionClicked(gameObject);
    }

    private void OnMouseOver()
    {
        EventManager.RaiseEvent(EventTypes.PositionHovered, gameObject);
    }

    private void OnMouseExit()
    {
        EventManager.RaiseEvent(EventTypes.PositionHoveredExit, gameObject);
    }

    private void OrganizeCharacterOnPosition()
    {
        var characters = NonBossUnits.ToList();
        if (characters.Count >= 1)
        {
            var increment = 360 / characters.Count;
            var offsetMagnitude = characters.Count > 1 ? 0.5f : 0f;

            var currentIncrement = 0;
            foreach (var character in characters)
            {
                currentIncrement += increment;
                var newPosition = transform.position + (Quaternion.Euler(0, currentIncrement, 0) * (Vector3.forward * offsetMagnitude));

                character.transform.SetPositionAndRotation(newPosition, Quaternion.Euler(0, currentIncrement - 180, 0));
            }
        }

    }

    public void Show()
    {
        render.enabled = true;
    }

    public void Hide()
    {
        render.enabled = false;
    }
}
