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
    private Dictionary<GameObject, GameObject> enemyTypeDisplayed = new Dictionary<GameObject, GameObject>();

    void Start()
    {
        offsetPosition = new Vector3(11f, 0f, 0f);
        offsetRotation = Quaternion.Euler(135, 0, 180);
        EventManager.StartListeningGameObject(EventTypes.EnemyCreated, AddEnemyDisplay);
        EventManager.StartListeningGameObject(EventTypes.EnemyRemoved, RemoveEnemyDisplay);
    }

    

    void Update()
    {
        
    }

    public void Initialize()
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
}
