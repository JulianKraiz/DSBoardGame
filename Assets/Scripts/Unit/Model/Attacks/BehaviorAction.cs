using Assets.Scripts.Tile;
using System.Collections.Generic;

namespace Assets.Scripts.Unit.Model.Attacks
{
    public abstract class BehaviorAction
    {
        public AttackSide Side;

        public abstract BehaviorAction Clone();
        public abstract List<UnitBasicProperties> FindTargetsInRange(UnitBasicProperties attacker, PositionBehavior attackerPosition, IEnumerable<PositionBehavior> positions);
    }
}