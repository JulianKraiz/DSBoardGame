using Assets.Scripts.Tile;
using BoardGame.Script.Events;
using System;
using System.Collections;
using System.Collections.Generic;
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
    }

    private void NotifyPositionSelected(PositionBehavior position)
    {
        var moveCommand = new UnitMovement()
        {
            MoveFrom = currentPosition,
            MoveTo = position,
            Unit = movingUnit.gameObject,
        };
        EventManager.RaiseEvent(ObjectEventType.UnitMoved, moveCommand);

        if (PositionClicked != null)
        {
            PositionClicked(position);
        }
    }

    void Update()
    {
    }

    public void SetupAndShow(UnitBasicProperties unit, MoveChoserType moveType, UnitBasicProperties awayFrom = null)
    {
        DescriptionText.text = $"Choose {moveType.ToString()} {Environment.NewLine} Direction";

        this.movingUnit = unit;
        if (this.movingUnit == null)
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
        }
        else
        {
            var gameState = FindObjectOfType<GameStateManager>();
            var tile = gameState.GetActiveTile();
            var positions = tile.GetPositions();

            currentPosition = positions.First(p => p.HasUnit(unit.gameObject));
            var awayFromPosition = awayFrom != null ? positions.First(p => p.HasUnit(awayFrom.gameObject)) : null;

            OcclusionBackground.SetActive(true);
            DescriptionText.gameObject.SetActive(true);
            ShowValidDirections(currentPosition, awayFromPosition);
        }
    }

    private void ShowValidDirections(PositionBehavior currentPosition, PositionBehavior awayFromPosition = null)
    {
        if (awayFromPosition == null || currentPosition == awayFromPosition)
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
        else
        {
            var currentDistance = PathFinder.GetPath(awayFromPosition, currentPosition).Count;
            if (currentPosition.NorthPosition != null && PathFinder.GetPath(awayFromPosition, currentPosition.NorthPosition.GetComponent<PositionBehavior>()).Count > currentDistance)
            {
                NorthArrowBehavior.SetTarget(currentPosition.NorthPosition);
            }
            if (currentPosition.NorthEastPosition != null && PathFinder.GetPath(awayFromPosition, currentPosition.NorthEastPosition.GetComponent<PositionBehavior>()).Count > currentDistance)
            {
                NorthEastArrowBehavior.SetTarget(currentPosition.NorthEastPosition);
            }
            if (currentPosition.EastPosition != null && PathFinder.GetPath(awayFromPosition, currentPosition.EastPosition.GetComponent<PositionBehavior>()).Count > currentDistance)
            {
                EastArrowBehavior.SetTarget(currentPosition.EastPosition);
            }
            if (currentPosition.SouthEastPosition != null && PathFinder.GetPath(awayFromPosition, currentPosition.SouthEastPosition.GetComponent<PositionBehavior>()).Count > currentDistance)
            {
                SouthEastArrowBehavior.SetTarget(currentPosition.SouthEastPosition);
            }
            if (currentPosition.SouthPosition != null && PathFinder.GetPath(awayFromPosition, currentPosition.SouthPosition.GetComponent<PositionBehavior>()).Count > currentDistance)
            {
                SouthArrowBehavior.SetTarget(currentPosition.SouthPosition);
            }
            if (currentPosition.SouthWestPosition != null && PathFinder.GetPath(awayFromPosition, currentPosition.SouthWestPosition.GetComponent<PositionBehavior>()).Count > currentDistance)
            {
                SouthWestArrowBehavior.SetTarget(currentPosition.SouthWestPosition);
            }
            if (currentPosition.WestPosition != null && PathFinder.GetPath(awayFromPosition, currentPosition.WestPosition.GetComponent<PositionBehavior>()).Count > currentDistance)
            {
                WestArrowBehavior.SetTarget(currentPosition.WestPosition);
            }
            if (currentPosition.NorthWestPosition != null && PathFinder.GetPath(awayFromPosition, currentPosition.NorthWestPosition.GetComponent<PositionBehavior>()).Count > currentDistance)
            {
                NorthWestArrowBehavior.SetTarget(currentPosition.NorthWestPosition);
            }
        }
    }

}

public enum MoveChoserType
{
    None,
    Dodge,
    Push
}