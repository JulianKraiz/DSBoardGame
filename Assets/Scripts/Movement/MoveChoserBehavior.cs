using Assets.Scripts.Tile;
using BoardGame.Script.Events;
using System;
using System.Linq;
using UnityEngine;

public class MoveChoserBehavior : MonoBehaviour
{
    public MoveChoserArrowBehavior NorthArrowBehavior;
    public MoveChoserArrowBehavior NorthEastArrowBehavior;
    public MoveChoserArrowBehavior EastArrowBehavior;
    public MoveChoserArrowBehavior SouthEastArrowBehavior;
    public MoveChoserArrowBehavior SouthArrowBehavior;
    public MoveChoserArrowBehavior SouthWestArrowBehavior;
    public MoveChoserArrowBehavior WestArrowBehavior;
    public MoveChoserArrowBehavior NorthWestArrowBehavior;
    public GameObject NoneButton;
    public TextMesh DescriptionText;

    public GameObject OcclusionBackground;

    public delegate void PositionEvent(PositionBehavior position);
    public event PositionEvent PositionClicked;

    private PositionBehavior currentPosition;
    private UnitBasicProperties movingUnit;

    void Start()
    {
        NorthArrowBehavior.PositionClicked += NotifyPositionSelected;
        NorthEastArrowBehavior.PositionClicked += NotifyPositionSelected;
        EastArrowBehavior.PositionClicked += NotifyPositionSelected;
        SouthEastArrowBehavior.PositionClicked += NotifyPositionSelected;
        SouthArrowBehavior.PositionClicked += NotifyPositionSelected;
        SouthWestArrowBehavior.PositionClicked += NotifyPositionSelected;
        WestArrowBehavior.PositionClicked += NotifyPositionSelected;
        NorthWestArrowBehavior.PositionClicked += NotifyPositionSelected;

        NoneButton.GetComponent<RaiseEventOnClicked>().PositionClicked += NonePositionSelected;
    }

    private void NonePositionSelected(GameObject position)
    {
        NotifyPositionSelected(null);
    }

    private void NotifyPositionSelected(PositionBehavior position)
    {
        if (PositionClicked != null)
        {
            var moveCommand = new UnitMovement()
            {
                MoveFrom = currentPosition,
                MoveTo = position ?? currentPosition,
                Unit = movingUnit.gameObject,
            };
            EventManager.RaiseEvent(ObjectEventType.UnitMoved, moveCommand);
            PositionClicked(position);
        }
    }

    void Update()
    {
    }

    public void Hide()
    {
        HideAll();
    }

    public void SetupAndShow(UnitBasicProperties unit, MoveChoserType moveType, PositionBehavior awayFromPosition)
    {
        movingUnit = unit;
        DescriptionText.text = $"Choose {moveType.ToString()} {Environment.NewLine} Direction";

        OcclusionBackground.SetActive(true);
        DescriptionText.gameObject.SetActive(true);
        ShowValidDirections(currentPosition, awayFromPosition);

        if (moveType == MoveChoserType.Shift)
        {
            NoneButton.SetActive(true);
        }
    }

    public void SetupAndShow(UnitBasicProperties unit, MoveChoserType moveType, UnitBasicProperties awayFrom = null)
    {
        var gameState = FindObjectOfType<GameStateManager>();
        var tile = gameState.GetActiveTile();
        var positions = tile.GetPositions();

        currentPosition = positions.First(p => p.HasUnit(unit.gameObject));
        var awayFromPosition = awayFrom != null ? positions.First(p => p.HasUnit(awayFrom.gameObject)) : null;
        SetupAndShow(unit, moveType, awayFromPosition);
    }

    private void ShowValidDirections(PositionBehavior currentPosition, PositionBehavior awayFromPosition = null)
    {
        if (awayFromPosition == null || currentPosition == awayFromPosition)
        {
            EnableAll();
        }
        else
        {
            var currentDistance = PathFinder.GetPath(awayFromPosition, currentPosition).Count;
            var validCount = 0;
            if (currentPosition.NorthPosition != null && PathFinder.GetPath(awayFromPosition, currentPosition.NorthPosition.GetComponent<PositionBehavior>()).Count > currentDistance)
            {
                validCount++;
                NorthArrowBehavior.SetTarget(currentPosition.NorthPosition);
            }
            if (currentPosition.NorthEastPosition != null && PathFinder.GetPath(awayFromPosition, currentPosition.NorthEastPosition.GetComponent<PositionBehavior>()).Count > currentDistance)
            {
                validCount++;
                NorthEastArrowBehavior.SetTarget(currentPosition.NorthEastPosition);
            }
            if (currentPosition.EastPosition != null && PathFinder.GetPath(awayFromPosition, currentPosition.EastPosition.GetComponent<PositionBehavior>()).Count > currentDistance)
            {
                validCount++;
                EastArrowBehavior.SetTarget(currentPosition.EastPosition);
            }
            if (currentPosition.SouthEastPosition != null && PathFinder.GetPath(awayFromPosition, currentPosition.SouthEastPosition.GetComponent<PositionBehavior>()).Count > currentDistance)
            {
                validCount++;
                SouthEastArrowBehavior.SetTarget(currentPosition.SouthEastPosition);
            }
            if (currentPosition.SouthPosition != null && PathFinder.GetPath(awayFromPosition, currentPosition.SouthPosition.GetComponent<PositionBehavior>()).Count > currentDistance)
            {
                validCount++;
                SouthArrowBehavior.SetTarget(currentPosition.SouthPosition);
            }
            if (currentPosition.SouthWestPosition != null && PathFinder.GetPath(awayFromPosition, currentPosition.SouthWestPosition.GetComponent<PositionBehavior>()).Count > currentDistance)
            {
                validCount++;
                SouthWestArrowBehavior.SetTarget(currentPosition.SouthWestPosition);
            }
            if (currentPosition.WestPosition != null && PathFinder.GetPath(awayFromPosition, currentPosition.WestPosition.GetComponent<PositionBehavior>()).Count > currentDistance)
            {
                validCount++;
                WestArrowBehavior.SetTarget(currentPosition.WestPosition);
            }
            if (currentPosition.NorthWestPosition != null && PathFinder.GetPath(awayFromPosition, currentPosition.NorthWestPosition.GetComponent<PositionBehavior>()).Count > currentDistance)
            {
                validCount++;
                NorthWestArrowBehavior.SetTarget(currentPosition.NorthWestPosition);
            }

            if (validCount == 0)
            {
                EnableAll();
            }
        }


    }

    private void EnableAll()
    {
        NorthArrowBehavior.SetTarget(currentPosition.NorthPosition);
        NorthEastArrowBehavior.SetTarget(currentPosition.NorthEastPosition);
        EastArrowBehavior.SetTarget(currentPosition.EastPosition);
        SouthEastArrowBehavior.SetTarget(currentPosition.SouthEastPosition);
        SouthArrowBehavior.SetTarget(currentPosition.SouthPosition);
        SouthWestArrowBehavior.SetTarget(currentPosition.SouthWestPosition);
        WestArrowBehavior.SetTarget(currentPosition.WestPosition);
        NorthWestArrowBehavior.SetTarget(currentPosition.NorthWestPosition);
    }

    private void HideAll()
    {
        NorthArrowBehavior.SetTarget(null);
        NorthEastArrowBehavior.SetTarget(null);
        EastArrowBehavior.SetTarget(null);
        SouthEastArrowBehavior.SetTarget(null);
        SouthArrowBehavior.SetTarget(null);
        SouthWestArrowBehavior.SetTarget(null);
        WestArrowBehavior.SetTarget(null);
        NorthWestArrowBehavior.SetTarget(null);
        OcclusionBackground.SetActive(false);
        DescriptionText.gameObject.SetActive(false);
        NoneButton.SetActive(false);
    }

}

public enum MoveChoserType
{
    None,
    Dodge,
    Push,
    Shift
}