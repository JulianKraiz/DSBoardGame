﻿namespace Assets.Scripts.ActiveUnitDisplay
{
    public class AttackDetail
    {
        public int staminaCost = 0;
        public int blackAttackDices = 0;
        public int blueAttackDices = 0;
        public int orangeAttackDices = 0;
        public int flatModifier = 0;

        public bool magicAttack = false;

        public int minimumRange = 0;
        public int range = 0;
        public bool infiniteRange = false;
        public int dodgeLevel = 0;
        
        public bool nodeSplash = false;
        public bool targetAllies = false;
        public bool affectAllInRangeUnits = false;
        public AttackSide side;

        public bool notEnoughStamina(int unitStamina) {
            return unitStamina - staminaCost <= 0;
        }
    }
}
