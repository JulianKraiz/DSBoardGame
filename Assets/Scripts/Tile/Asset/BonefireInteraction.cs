using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Menu.Model;
using BoardGame.Script.Events;
using UnityEngine;

public class BonefireInteraction : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnMouseUp()
    {
        EventManager.RaiseEvent(ObjectEventType.GetActionConfirmation,new ConfirmationMenuLoad()
        {
            Description = "Do you want to rest at the bonefire?" + Environment.NewLine + "All characters will be healed" + Environment.NewLine + "and enemies will respawn.",
            Action = () => { EventManager.RaiseEvent(GameObjectEventType.RestPartyAtBonefire); } 
        });
    }


}
