using Assets.Scripts.Tile;
using BoardGame.Script.Events;
using BoardGame.Script.MovementFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.General
{
    public class CameraAutMover : MonoBehaviour
    {
        GameStateManager gameState;
        Camera mainCamera;

        TileManager focusedTile;
        public Vector3 focusedTileOffsetHover = new Vector3(-0.5f, 7, -7f);

        bool followActiveUnit;
        UnitBasicProperties activeUnit;

        bool followMidPointBetweenUnit;
        GameObject attackedUnit;

        private void Start()
        {
            gameState = FindObjectOfType<GameStateManager>();
            mainCamera = Camera.main;

            EventManager.StartListening(GameObjectEventType.TileFocused, NewTileFocused);

            EventManager.StartListening(GameObjectEventType.UnitIsActivated, NewUnitActivated);
            EventManager.StartListening(ObjectEventType.UnitMoved, TrackUnitMoved);

            EventManager.StartListening(GameObjectEventType.UnitIsCurrentlyAttacked, TrackBetweenAttackingAndDefender);
            EventManager.StartListening(GameObjectEventType.UnitIsNotCurrentlyAttacked, TrackActiveUnitIfExists);

        }

        private void TrackActiveUnitIfExists(GameObject _)
        {
            followMidPointBetweenUnit = false;
            attackedUnit = null;
            followActiveUnit = true;
        }

        private void TrackBetweenAttackingAndDefender(GameObject acttackedUnitArg)
        {
            followMidPointBetweenUnit = true;
            attackedUnit = acttackedUnitArg;
        }

        private void TrackUnitMoved(object unitObject)
        {
            var unit = ((UnitMovement)unitObject).Unit.GetComponent<UnitBasicProperties>();
            if (unit.isActive)
            {
                followActiveUnit = true;
            }
        }

        private void NewUnitActivated(GameObject unitObject)
        {
            activeUnit = unitObject.GetComponent<UnitBasicProperties>();
            followActiveUnit = true;
        }

        private void NewTileFocused(GameObject arg0)
        {
            focusedTile = gameState.GetActiveTile();
            activeUnit = null;
        }

        private void Update()
        {
            if (focusedTile != null && focusedTile.isFocused && !focusedTile.isHovered)
            {
                var targetPosition = focusedTile.transform.position + focusedTileOffsetHover;
                var lerpedProsition = Vect3LerpSnap.SnapLerp(mainCamera.transform.position, targetPosition, 0.9f, 0.2f);

                if (lerpedProsition == targetPosition)
                {
                    focusedTile.isHovered = true;
                    EventManager.RaiseEvent(GameObjectEventType.TileIsEntered);
                }

                mainCamera.transform.SetPositionAndRotation(lerpedProsition, Quaternion.Euler(45, 0, 0));
            }
            else if (followActiveUnit && activeUnit != null)
            {
                var targetPosition = activeUnit.transform.position + focusedTileOffsetHover;
                mainCamera.transform.SetPositionAndRotation(targetPosition, mainCamera.transform.rotation);
                followActiveUnit = false;
            }
            else if (followMidPointBetweenUnit && activeUnit != null && attackedUnit != null)
            {
                var targetPosition = ((activeUnit.transform.position + attackedUnit.transform.position) / 2) + focusedTileOffsetHover;
                mainCamera.transform.SetPositionAndRotation(targetPosition, mainCamera.transform.rotation);
                followMidPointBetweenUnit = false;
            }
        }


    }
}
