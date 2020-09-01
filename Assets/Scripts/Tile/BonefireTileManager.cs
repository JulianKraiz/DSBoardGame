using BoardGame.Script.Events;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BonefireTileManager : TileManager
{
    protected override void StartInternal()
    {
    }

    protected override void UpdateInternal()
    {
    }

    public override void PrepareTileEntered()
    {
        if (isFocused)
        {
            players = GameObject.FindGameObjectsWithTag("Player")
           .Select(p => p.GetComponent<UnitBasicProperties>())
           .ToList();

            foreach (var player in players)
            {
                player.isActive = false;
            }

            foreach (var position in positions)
            {
                position.ResetPosition(false);
            }

            var potentialPositions = positions;
            var bonefire = gameObject.transform.Find("Bonefire");

            var index = 0;
            foreach (var player in players)
            {
                var slot = positions[index];
                player.transform.SetPositionAndRotation(slot.transform.position, Quaternion.identity);
                player.transform.LookAt(bonefire);
                index++;
            }
        }

    }
}
