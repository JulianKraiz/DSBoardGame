using BoardGame.Script.Events;
using BoardGame.Unit;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyDisplayContainer : MonoBehaviour
{
    public GameObject displayAsset;

    private Vector3 offsetPosition;
    private Quaternion offsetRotation;
    private Dictionary<EnemyClassEnum,GameObject> enemyTypeDisplayed = new Dictionary<EnemyClassEnum, GameObject>();

    void Start()
    {
        offsetPosition = new Vector3(1.4f, 0f, 0f);
        offsetRotation = Quaternion.Euler(135, 0, 180);
        EventManager.StartListening(EventTypes.EnemyCreated, AddEnemyDisplay);
        EventManager.StartListening(EventTypes.EnemyRemoved, RemoveEnemyDisplay);
    }

    

    void Update()
    {
        
    }

    public void Initialize()
    {
    }

    private void AddEnemyDisplay(GameObject enemy)
    {
        var properties = enemy.GetComponent<EnemyProperties>();
        if (!enemyTypeDisplayed.Keys.Contains(properties.enemyType))
        {
            var display = Instantiate(displayAsset, transform);
            display.transform.Translate(enemyTypeDisplayed.Keys.Count * offsetPosition);
            display.transform.rotation = offsetRotation;
            var behavior = display.GetComponent<EnemyDisplayBehavior>();
            behavior.SetUnit(enemy);
            behavior.Initialize();
            enemyTypeDisplayed.Add(properties.enemyType, display);
        }
    }

    private void RemoveEnemyDisplay(GameObject enemy)
    {
        var properties = enemy.GetComponent<EnemyProperties>();
        if(enemyTypeDisplayed.TryGetValue(properties.enemyType, out var display))
        {
            Destroy(display);
            enemyTypeDisplayed.Remove(properties.enemyType);
        }
    }
}
