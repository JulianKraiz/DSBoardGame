using BoardGame.Script.Events;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DiceBahevior : MonoBehaviour
{
    public GameObject topAnchor;
    public GameObject bottomAnchor;
    public GameObject northAnchor;
    public GameObject eastAnchor;
    public GameObject southAnchor;
    public GameObject westAnchor;

    public int topValue;
    public int bottomValue;
    public int northValue;
    public int eastValue;
    public int southValue;
    public int westValue;

    private Rigidbody body;
    private bool wasMoving;
    private bool emitedEvent;

    void Start()
    {
        ResetForThrow();
        body = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

        if (!emitedEvent && wasMoving && body.velocity == Vector3.zero)
        {
            EventManager.RaiseEvent(GameObjectEventType.DiceStoppedMoving, gameObject);
            emitedEvent = true;
        }

        if (body.velocity != Vector3.zero)
        {
            wasMoving = true;
        }
    }

    public int GetValue()
    {
        var allAnchors = new List<GameObject>() { topAnchor, bottomAnchor, northAnchor, eastAnchor, southAnchor, westAnchor };
        var highestPosition = allAnchors.Max(e => e.transform.position.y);
        if (topAnchor.transform.position.y == highestPosition)
        {
            return topValue;
        }
        else if (bottomAnchor.transform.position.y == highestPosition)
        {
            return bottomValue;
        }
        else if (northAnchor.transform.position.y == highestPosition)
        {
            return northValue;
        }
        else if (eastAnchor.transform.position.y == highestPosition)
        {
            return eastValue;
        }
        else if (southAnchor.transform.position.y == highestPosition)
        {
            return southValue;
        }
        else if (westAnchor.transform.position.y == highestPosition)
        {
            return westValue;
        }

        throw new System.Exception($"No anchor matching with position {highestPosition}");
    }

    internal void ResetForThrow()
    {
        wasMoving = false;
        emitedEvent = false;
    }
}
