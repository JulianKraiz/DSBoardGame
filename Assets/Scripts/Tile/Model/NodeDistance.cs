using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Tile.Model
{
    public class NodeDistance
    {
        public PositionBehavior Node { get; set; }
        public PositionBehavior DistanceTo { get; set; }
        
        public int PathLength { get; set; }
        public float Lengh { get; set; }

    }
}
