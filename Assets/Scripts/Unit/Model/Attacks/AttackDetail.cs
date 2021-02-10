using System;

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

        public int Move = 0;
        public bool Push = false;
        public int PushDamage = 0;
        public bool NodeSplash = false;
        public bool TargetAllies = false;
        public bool AffectAllInRangeUnits = false;
        public AttackSide Side;

        public PreferedTarget TargetPreference;

        internal bool InRange(int pathLength)
        {
            if(pathLength < MinimumRange)
            {
                return false;
            }
            if(InfiniteRange)
            {
                return true;
            }
            else if(pathLength > Range)
            {
                return false;
            }
            return true;
        }

        public bool notEnoughStamina(int unitStamina)
        {
            return unitStamina - StaminaCost <= 0;
        }

        public AttackDetail Clone()
        {
            var clone = new AttackDetail()
            {
                Move = this.Move,

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
            };
            return clone;
        }
    }
}
