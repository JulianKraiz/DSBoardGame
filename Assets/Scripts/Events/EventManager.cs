using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace BoardGame.Script.Events
{

    public enum GameObjectEventType
    {
        TileIsEntered,
        TileCleared,
        UnitIsActivated,
        PositionHovered,
        PositionHoveredExit,
        EnemyCreated,
        UnitHoverEntered,
        UnitHoverExited,
        ActiveUnitMoved,
        UnitSelected,
        ResetAndHideAttackDial,
        UnitDestroyed,
        PlayerSheetClicked,
        ZoomUnitDisplay,
        ResetAndHideEnemyDisplays,
        DiceStoppedMoving,
        AbilityTrigger,
        LuckTrigger,
        EstusTrigger,
        EndUnitTurn,
        TileFocused,
        UnitIsCurrentlyAttacked,
        UnitIsNotCurrentlyAttacked,
    }

    public enum ObjectEventType
    {
        AttackApplied,
        AttackSelected,
        AttackDeselected,
        AttackHovered,
        AttackHoverEnded,
        PlayerSheetClicked,
        AddSoulsToCache,
        EncounterToResolve,
        EncountersResolved,
        UnitMoved,
    }

    public class EventManager
    {
        private Dictionary<GameObjectEventType, UnityEventWithGameObject> _eventRegisterGameObject = new Dictionary<GameObjectEventType, UnityEventWithGameObject>();
        private Dictionary<ObjectEventType, UnityEventWithObject> _eventRegisterObject = new Dictionary<ObjectEventType, UnityEventWithObject>();

        static EventManager()
        {
            Instance = new EventManager();
        }

        private EventManager()
        {
        }

        public static EventManager Instance { get; private set; }

        public static void StartListening(GameObjectEventType eventType, UnityAction<GameObject> action)
        {
            if (!Instance._eventRegisterGameObject.Keys.Contains(eventType))
            {
                Instance._eventRegisterGameObject.Add(eventType, new UnityEventWithGameObject());
            }

            var eventListener = Instance._eventRegisterGameObject[eventType];
            eventListener.AddListener(action);
        }

        public static void StopListening(GameObjectEventType eventType, UnityAction<GameObject> action)
        {
            if (Instance._eventRegisterGameObject.TryGetValue(eventType, out var listener))
            {
                listener.RemoveListener(action);
            }
        }

        public static void RaiseEvent(GameObjectEventType eventType, GameObject arg = null)
        {
            if (Instance._eventRegisterGameObject.TryGetValue(eventType, out var listener))
            {
                listener.Invoke(arg);
            }
        }

        public static void StartListening(ObjectEventType eventType, UnityAction<object> action)
        {
            if (!Instance._eventRegisterObject.Keys.Contains(eventType))
            {
                Instance._eventRegisterObject.Add(eventType, new UnityEventWithObject());
            }

            var eventListener = Instance._eventRegisterObject[eventType];
            eventListener.AddListener(action);
        }

        public static void StopListening(ObjectEventType eventType, UnityAction<object> action)
        {
            if (Instance._eventRegisterObject.TryGetValue(eventType, out var listener))
            {
                listener.RemoveListener(action);
            }
        }

        public static void RaiseEvent(ObjectEventType eventType, object arg = null)
        {
            if (Instance._eventRegisterObject.TryGetValue(eventType, out var listener))
            {
                listener.Invoke(arg);
            }
        }


    }
}