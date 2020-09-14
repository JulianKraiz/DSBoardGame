using UnityEngine;
using UnityEngine.Events;

namespace BoardGame.Script.Events
{
    [System.Serializable]
    public class UnityEventWithObject : UnityEvent<object>
    {
    }
}