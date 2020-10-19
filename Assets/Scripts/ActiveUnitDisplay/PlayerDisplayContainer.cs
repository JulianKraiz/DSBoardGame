using BoardGame.Script.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDisplayContainer : MonoBehaviour
{
    public GameObject displayAsset;

    private Vector3 offsetPosition;
    private List<GameObject> characterDisplays = new List<GameObject>();

    void Start()
    {
        offsetPosition = new Vector3(0f, 0f, -8.4f);
    }



    void Update()
    {

    }

    public void Initialize()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        var buffPositionOffset = offsetPosition;
        var index = 0;
        foreach (var player in players)
        {
            buffPositionOffset = index * offsetPosition;
            var display = Instantiate(displayAsset, transform);
            display.transform.localPosition = buffPositionOffset;
            var behavior = display.GetComponent<PlayerDisplayBehavior>();
            behavior.SetUnit(player, index);
            characterDisplays.Add(display);

            index++;
        }
    }


}
