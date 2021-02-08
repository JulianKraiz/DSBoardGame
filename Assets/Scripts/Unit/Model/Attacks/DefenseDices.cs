public class DefenseDices
{
        public int BlackDices { get; set; }
        public int BlueDices { get; set; }
        public int OrangeDices { get; set; }
        public int DodgeDices { get; set; }
        public int FlatReduce { get; set; }

    public DefenseDices Clone()
    {
        return new DefenseDices()
        {
            BlackDices = BlackDices,
            BlueDices = BlueDices,
            OrangeDices = OrangeDices,
            DodgeDices = DodgeDices,
            FlatReduce = FlatReduce,
        };
    }
}