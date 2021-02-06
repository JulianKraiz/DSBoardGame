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

    public GameObject OcclusionBackground;

    public delegate void PositionEvent(PositionBehavior position);
    public event PositionEvent PositionClicked;

    PositionBehavior currentPosition;
    UnitBasicProperties unit;

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
            Unit = unit.gameObject,
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

    public void SetupAndShow(UnitBasicProperties unit)
    {
        this.unit = unit;
        if (this.unit == null)
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
        }
        else
        {
            var gameState = FindObjectOfType<GameStateManager>();
            var tile = gameState.GetActiveTile();
            var positions = tile.GetPositions();

            currentPosition = positions.First(p => p.HasUnit(this.unit.gameObject));

            OcclusionBackground.SetActive(true);
            NorthArrowBehavior.SetTarget(currentPosition.NorthPosition);
            NorthEastArrowBehavior.SetTarget(currentPosition.NorthEastPosition);
            EastArrowBehavior.SetTarget(currentPosition.EastPosition);
            SouthEastArrowBehavior.SetTarget(currentPosition.SouthEastPosition);
            SouthArrowBehavior.SetTarget(currentPosition.SouthPosition);
            SouthWestArrowBehavior.SetTarget(currentPosition.SouthWestPosition);
            WestArrowBehavior.SetTarget(currentPosition.WestPosition);
            NorthWestArrowBehavior.SetTarget(currentPosition.NorthWestPosition);
        }
    } 
}