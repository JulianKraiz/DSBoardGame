using UnityEngine;
using UnityEngine.Events;

namespace BoardGame.Script.Events
{
    [System.Serializable]
    public class UnityEventWithGameObject : UnityEvent<GameObject>
    {
    }
}