namespace Assets.Scripts.Unit.Model.Attacks
{
    public class Encounter
    {
        public UnitBasicProperties Attacker { get; set; }
        public UnitBasicProperties Defender { get; set; }

        public AttackDetail Attack { get; set; }
        public DefenseDices Defense {get;set;}

        public int DamageRoll { get; set; }
        public int DefenseRoll { get; set; }
        public int DodgeRoll { get; set; }

    }
}
