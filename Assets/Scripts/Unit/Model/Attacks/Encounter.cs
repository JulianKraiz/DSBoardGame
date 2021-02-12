namespace Assets.Scripts.Unit.Model.Attacks
{
    public class Encounter 
    {
        public Encounter(BehaviorAction action, UnitBasicProperties attacker, UnitBasicProperties defender)
        {
            Action = action;
            Attacker = attacker;
            Defender = defender;
        }

        public UnitBasicProperties Attacker { get; set; }
        public UnitBasicProperties Defender { get; set; }
        public BehaviorAction Action { get; set; }
    }
}
