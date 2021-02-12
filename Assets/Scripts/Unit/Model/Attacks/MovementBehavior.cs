using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Unit.Model.Attacks
{
    public class MovementAction : BehaviorAction
    {
        public override BehaviorAction Clone()
        {
            throw new NotImplementedException();
        }

        public override List<UnitBasicProperties> FindTargetsInRange(UnitBasicProperties attacker, PositionBehavior attackerPosition, IEnumerable<PositionBehavior> positions, bool includeShiftBefore = true)
        {
            throw new NotImplementedException();
        }
    }
}
