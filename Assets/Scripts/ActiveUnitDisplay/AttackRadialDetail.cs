using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.ActiveUnitDisplay
{
    public class AttackRadialDetail
    {
        public int staminaCost = 0;
        public int blackAttackDices = 0;
        public int blueAttackDices = 0;
        public int orangeAttackDices = 0;
        public int flatModifier = 0;

        public bool magicAttack = false;

        public int minimumRange = 0;
        public int range = 0;
        internal bool infiniteRange = false;
        public int dodgeLevel = 0;

        public bool nodeSplash = false;

        public bool notEnoughStamina = false;

        public bool targetAllies = false;
        public bool affectAllInRangeUnits = false;
        public AttackRadialSide side;
    }
}
