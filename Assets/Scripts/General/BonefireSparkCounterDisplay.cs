using BoardGame.Script.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonefireSparkCounterDisplay : MonoBehaviour
{

    public GameStateManager gameState;
    public TextMesh sparkCountText;

    void Start()
    {
    }

    void Update()
    {
        sparkCountText.text = $"{gameState.bonefireSparks} Sparks";
    }
}
