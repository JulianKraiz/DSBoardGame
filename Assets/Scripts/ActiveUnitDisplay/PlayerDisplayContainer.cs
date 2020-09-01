using BoardGame.Script.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDisplayContainer : MonoBehaviour
{
    public GameObject displayAsset;

    private Vector3 offsetPosition = new Vector3(0f,-1.1f,0f);
    private List<GameObject> characterDisplays = new List<GameObject>();

    void Start()
    {
    }

    

    void Update()
    {
        
    }

    public void Initialize()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");

        var index = 0;
        foreach (var player in players)
        {
            var display = Instantiate(displayAsset, transform);
            display.transform.Translate(index * offsetPosition);
            var behavior = display.GetComponent<PlayerDisplayBehavior>();
            behavior.SetUnit(player,index);
            characterDisplays.Add(display);

            index++;
        }
    }

 
}
