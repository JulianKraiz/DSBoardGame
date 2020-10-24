using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;

namespace BoardGame.Script.Events
{

    public enum EventTypes
    {
        TileIsEntered,
        TileCleared,
        PositionClicked,
        UnitIsActivated,
        PositionHovered,
        PositionHoveredExit,
        EnemyCreated,
        UnitHoverEntered,
        UnitHoverExited,
        ActiveUnitSelected,
        ActiveUnitMoved,
        UnitSelected,
        AttackSelected,
        AttackDeselected,
        AttackHovered,
        AttackHoverEnded,
        ResetAndHideAttackDial,
        AttackTargetSelected,
        UnitDestroyed,
        PlayerSheetClicked,
        AttackApplied,
        ToggleZoomUnitDisplay,
        VictoryTileCleared,
        ResetAndHideEnemyDisplays,
        AddSoulsToCache,
    }

    public class EventManager
    {
        private Dictionary<EventTypes, UnityEventWithGameObject> _eventRegisterGameObject = new Dictionary<EventTypes, UnityEventWithGameObject>();
        private Dictionary<EventTypes, UnityEventWithObject> _eventRegisterObject = new Dictionary<EventTypes, UnityEventWithObject>();

        static EventManager()
        {
            Instance = new EventManager();
        }

        private EventManager()
        {
        }

        public static EventManager Instance { get; private set; }

        public static void StartListeningGameObject(EventTypes eventType, UnityAction<GameObject> action)
        {
            if (!Instance._eventRegisterGameObject.Keys.Contains(eventType))
            {
                Instance._eventRegisterGameObject.Add(eventType, new UnityEventWithGameObject());
            }

            var eventListener = Instance._eventRegisterGameObject[eventType];
            eventListener.AddListener(action);
        }

        public static void StopListeningGameObject(EventTypes eventType, UnityAction<GameObject> action)
        {
            if (Instance._eventRegisterGameObject.TryGetValue(eventType, out var listener))
            {
                listener.RemoveListener(action);
            }
        }

        public static void RaiseEventGameObject(EventTypes eventType, GameObject arg = null)
        {
            if (Instance._eventRegisterGameObject.TryGetValue(eventType, out var listener))
            {
                listener.Invoke(arg);
            }
        }

        public static void StartListeningObject(EventTypes eventType, UnityAction<object> action)
        {
            if (!Instance._eventRegisterObject.Keys.Contains(eventType))
            {
                Instance._eventRegisterObject.Add(eventType, new UnityEventWithObject());
            }

            var eventListener = Instance._eventRegisterObject[eventType];
            eventListener.AddListener(action);
        }

        public static void StopListeningObject(EventTypes eventType, UnityAction<object> action)
        {
            if (Instance._eventRegisterObject.TryGetValue(eventType, out var listener))
            {
                listener.RemoveListener(action);
            }
        }

        public static void RaiseEventObject(EventTypes eventType, object arg = null)
        {
            if (Instance._eventRegisterObject.TryGetValue(eventType, out var listener))
            {
                listener.Invoke(arg);
            }
        }
    }
}