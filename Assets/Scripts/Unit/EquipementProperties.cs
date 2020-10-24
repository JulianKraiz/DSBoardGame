using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Unit
{
    public class EquipementProperties : MonoBehaviour
    {
        private Material background;
        public string equipementName;

        public int dodgeRollsDices = 0;

        public int blackArmorDices = 0;
        public int blueArmorDices = 0;
        public int orangeArmorDices = 0;
        public int blackMagicArmorDices = 0;
        public int blueMagicArmorDices = 0;
        public int orangeMagicArmorDices = 0;

        public int flatArmorResistence = 0;
        public int flatMagicArmorResistence = 0;

        public int upgradeSlots = 0;
        public bool isOneHanded = true;
        public bool isArmor = false;

        public int strenghRequirement = 0;
        public int dextirityRequirement = 0;
        public int inteligenceRequirement = 0;
        public int faithRequirement = 0;

        public List<AttackDetail> attackList = new List<AttackDetail>();

        public void SetMaterial(Material material)
        {
            background = material;
            var temp = gameObject.GetComponent<MeshRenderer>();
            temp.material = background;
        }

        public void ContributeDiceToDefenseRolls(DefenseDices defense, bool magicAttack)
        {
            if (!magicAttack)
            {
                defense.BlackDices += blackArmorDices;
                defense.BlueDices += blueArmorDices ;
                defense.OrangeDices += orangeArmorDices ;
                defense.FlatReduce += flatArmorResistence;
            }
            if (magicAttack)
            {
                defense.BlackDices += blackMagicArmorDices;
                defense.BlueDices += blueMagicArmorDices;
                defense.OrangeDices += orangeMagicArmorDices;
                defense.FlatReduce += flatMagicArmorResistence;
            }
            
            defense.DodgeDices += dodgeRollsDices;
        }
    }
}
