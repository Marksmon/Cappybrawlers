using Capybrawlers.Cards;
using Capybrawlers.Core;
using UnityEngine;

namespace Capybrawlers.Creatures
{
    [CreateAssetMenu(fileName = "Equip_", menuName = "Capybrawlers/Equipment Data")]
    public class EquipmentData : ScriptableObject
    {
        public string id;
        public string equipmentName;
        public EquipSlot slot;
        public ElementType element;

        [Header("Stat Modifiers (signed)")]
        public int hpMod;
        public int atkMod;
        public int defMod;
        public int spdMod;

        public CardData grantedCard;
        public Sprite icon;
    }
}
