using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public class DiceRollManager
    {
        public static List<int> RollBlackDices(int numberOfDices)
        {
            var result = new List<int>();
            for(int i=0;i<numberOfDices; i++)
            {
                var rand = UnityEngine.Random.Range(0, 5);
                switch (rand)
                {
                    default:
                    case 0:
                    case 1:
                        result.Add(0);
                        break;
                    case 2:
                    case 3:
                    case 4:
                        result.Add(1);
                        break;
                    case 5:
                        result.Add(2);
                        break;
                }
            }
            return result;
        }

        public static List<int> RollBlueDices(int numberOfDices)
        {
            var result = new List<int>();
            for (int i = 0; i < numberOfDices; i++)
            {
                var rand = UnityEngine.Random.Range(0, 5);
                switch (rand)
                {
                    default:
                    case 0:
                    case 1:
                        result.Add(1);
                        break;
                    case 2:
                    case 3:
                    case 4:
                        result.Add(2);
                        break;
                    case 5:
                        result.Add(3);
                        break;
                }
            }
            return result;
        }

        public static List<int> RollOrangeDices(int numberOfDices)
        {
            var result = new List<int>();
            for (int i = 0; i < numberOfDices; i++)
            {
                var rand = UnityEngine.Random.Range(0, 5);
                switch (rand)
                {
                    default:
                    case 0:
                        result.Add(1);
                        break;
                    case 1:
                    case 2:
                        result.Add(2);
                        break;
                    case 3:
                    case 4:
                        result.Add(3);
                        break;
                    case 5:
                        result.Add(4);
                        break;
                }
            }
            return result;
        }

        public static List<int> SimulateRollDodgeDices(int numberOfDices)
        {
            var result = new List<int>();
            for (int i = 0; i < numberOfDices; i++)
            {
                var rand = UnityEngine.Random.Range(0, 1);
                result.Add(rand);
            }
            return result;
        }

        public static int RollDicesAndSum(int blackDices, int blueDices, int orangeDices, int flatModifier)
        {
            var damageReduction = flatModifier;
            damageReduction += DiceRollManager.RollBlackDices(blackDices).Sum();
            damageReduction += DiceRollManager.RollBlueDices(blueDices).Sum();
            damageReduction += DiceRollManager.RollOrangeDices(orangeDices).Sum();

            return damageReduction;
        }
    }
}
