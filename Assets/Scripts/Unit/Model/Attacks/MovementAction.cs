using Assets.Scripts.Tile;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Unit.Model.Attacks
{
    public class MovementAction : BehaviorAction
    {
        public int MoveDistance;
        public MovementDirection Direction;
        public PreferedTarget TargetPreference;
        public int Repeat = 1;
        public bool Push;
        public int PushDamage;
        public bool MagicAttack;


        public override BehaviorAction Clone()
        {
            return new MovementAction()
            {
                MoveDistance = MoveDistance,
                Direction = Direction,
                TargetPreference = TargetPreference,
                Repeat = Repeat,
                Push = Push,
                PushDamage = PushDamage,
                MagicAttack = MagicAttack,
            };
        }

        public override List<UnitBasicProperties> FindTargetsInRange(UnitBasicProperties attacker, PositionBehavior attackerPosition, IEnumerable<PositionBehavior> positions)
        {
            var currentSide = attacker.side;
            var targetSide = currentSide == UnitSide.Hollow ? UnitSide.Player : UnitSide.Hollow;

            var targets = new List<UnitBasicProperties>();
            foreach (var position in positions)
            {
                var potential = position.GetUnits(targetSide).ToList();
                targets.AddRange(potential.Select(p => p.GetComponent<UnitBasicProperties>()));
            }
            return targets;
        }
    }
}
