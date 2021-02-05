using BoardGame.Script.Events;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyDisplayContainer : MonoBehaviour
{
    public GameObject displayAsset;

    private Vector3 offsetPosition;
    private Dictionary<GameObject, GameObject> enemyTypeDisplayed = new Dictionary<GameObject, GameObject>();

    void Start()
    {
        offsetPosition = new Vector3(11f, 0f, 0f);
        EventManager.StartListening(GameObjectEventType.EnemyCreated, AddEnemyDisplay);
        EventManager.StartListening(GameObjectEventType.UnitDestroyed, RemoveEnemyDisplay);
        EventManager.StartListening(GameObjectEventType.ResetAndHideEnemyDisplays, ResetAndHideEnemyDisplays);
    }



    void Update()
    {
        
    }

    private void AddEnemyDisplay(GameObject enemy)
    {
        if (!enemyTypeDisplayed.Keys.Contains(enemy))
        {
            var display = Instantiate(displayAsset, transform);
            display.transform.localPosition = enemyTypeDisplayed.Keys.Count * offsetPosition;
            var behavior = display.GetComponent<EnemyDisplayBehavior>();
            behavior.SetUnit(enemy);
            behavior.Initialize();
            enemyTypeDisplayed.Add(enemy, display);
        }
    }

    private void RemoveEnemyDisplay(GameObject enemy)
    {
        if(enemyTypeDisplayed.TryGetValue(enemy, out var display))
        {
            Destroy(display);
            enemyTypeDisplayed.Remove(enemy);
        }
    }

    private void ResetAndHideEnemyDisplays(GameObject arg0)
    {
        foreach(var display in enemyTypeDisplayed.ToList())
        {
            Destroy(display.Value);
            enemyTypeDisplayed.Remove(display.Key);
        }
    }
}
