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
    }

    public class EventManager
    {
        private Dictionary<EventTypes, UnityEventWithObject> _eventRegister = new Dictionary<EventTypes, UnityEventWithObject>();

        static EventManager()
        {
            Instance = new EventManager();
        }

        private EventManager()
        {
        }

        public static EventManager Instance { get; private set; }

        public static void StartListening(EventTypes eventType, UnityAction<GameObject> action)
        {
            if (!Instance._eventRegister.Keys.Contains(eventType))
            {
                Instance._eventRegister.Add(eventType, new UnityEventWithObject());
            }

            var eventListener = Instance._eventRegister[eventType];
            eventListener.AddListener(action);
        }

        public static void StopListening(EventTypes eventType, UnityAction<GameObject> action)
        {
            if (Instance._eventRegister.TryGetValue(eventType, out var listener))
            {
                listener.RemoveListener(action);
            }
        }

        public static void RaiseEvent(EventTypes eventType, GameObject arg = null)
        {
            if (Instance._eventRegister.TryGetValue(eventType, out var listener))
            {
                listener.Invoke(arg);
            }
        }
    }
}