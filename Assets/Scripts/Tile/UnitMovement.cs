using UnityEngine;

namespace Assets.Scripts.Tile
{
    public class UnitMovement
    {
        public PositionBehavior MoveFrom { get; set; }
        public PositionBehavior MoveTo { get; set; }
        public GameObject Unit { get; set; }
    }
}