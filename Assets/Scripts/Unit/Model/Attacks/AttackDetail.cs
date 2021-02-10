using Assets.Scripts.Tile;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Unit.Model.Attacks
{
    public class AttackDetail
    {
        public int StaminaCost = 0;
        public int BlackDices = 0;
        public int BlueDices = 0;
        public int OrangeAttackDices = 0;
        public int FlatModifier = 0;

        public bool MagicAttack = false;

        public int MinimumRange = 0;
        public int Range = 0;
        public bool InfiniteRange = false;
        public int DodgeLevel = 0;

        public int Repeat = 1;
        internal bool Poison = false;
        internal bool Stagger = false;
        internal bool Bleed = false;
        internal bool Frozen = false;

        public int ShiftBefore = 0;
        public int ShiftAfter = 0;
        public bool Push = false;
        public int PushDamage = 0;
        public bool NodeSplash = false;
        public bool TargetAllies = false;
        public bool AffectAllInRangeUnits = false;
        public AttackSide Side;

        public PreferedTarget TargetPreference;

        internal bool InRange(int pathLength, bool withShiftBefore = true)
        {
            if(pathLength < MinimumRange)
            {
                return false;
            }
            if(InfiniteRange)
            {
                return true;
            }
            else if(pathLength > Range + ShiftBefore)
            {
                return false;
            }
            return true;
        }

        public bool NotEnoughStamina(int unitStamina)
        {
            return unitStamina - StaminaCost <= 0;
        }

        public List<UnitBasicProperties> FindTargetsInRange(UnitBasicProperties attacker, PositionBehavior attackerPosition, IEnumerable<PositionBehavior> positions, bool includeShiftBefore = true)
        {
            var currentSide = attacker.side;
            var targetSide = TargetAllies ? currentSide : currentSide == UnitSide.Hollow ? UnitSide.Player : UnitSide.Hollow;

            var targets = new List<UnitBasicProperties>();
            foreach (var position in positions)
            {
                var pathLength = PathFinder.GetPath(attackerPosition, position).Count;
                if (!InRange(pathLength, includeShiftBefore))
                {
                    continue;
                }

                var potential = position.GetUnits(targetSide).ToList();
                targets.AddRange(potential.Select(p => p.GetComponent<UnitBasicProperties>()));
            }
            return targets;
        }

        public List<UnitBasicProperties> FindTargetsOnNode(UnitBasicProperties attacker, PositionBehavior targetPosition, UnitBasicProperties originalTarget)
        {
            var currentSide = attacker.side;
            var targetSide = TargetAllies ? currentSide : currentSide == UnitSide.Hollow ? UnitSide.Player : UnitSide.Hollow;

            var targets = new List<UnitBasicProperties>();
            if (NodeSplash)
            {
                targets = targetPosition.GetUnits(targetSide).Select(u => u.GetComponent<UnitBasicProperties>()).ToList();
            }
            else
            {
                targets.Add(originalTarget);
            }
            return targets;
        }

        public AttackDetail Clone()
        {
            var clone = new AttackDetail()
            {
                StaminaCost = this.StaminaCost,
                BlackDices = this.BlackDices,
                BlueDices = this.BlueDices,
                OrangeAttackDices = this.OrangeAttackDices,
                FlatModifier = this.FlatModifier,
                MagicAttack = this.MagicAttack,
                MinimumRange = this.MinimumRange,
                Range = this.Range,
                InfiniteRange = this.InfiniteRange,
                DodgeLevel = this.DodgeLevel,

                Repeat = this.Repeat,
                Poison = this.Poison,
                Stagger = this.Stagger,
                Bleed = this.Bleed,
                Frozen = this.Frozen,
                NodeSplash = this.NodeSplash,
                TargetAllies = this.TargetAllies,
                AffectAllInRangeUnits = this.AffectAllInRangeUnits,
                Side = this.Side,
                TargetPreference = this.TargetPreference,
                Push = this.Push,
                PushDamage = this.PushDamage,
                ShiftBefore = this.ShiftBefore,
                ShiftAfter = this.ShiftAfter,
            };
            return clone;
        }
    }
}
