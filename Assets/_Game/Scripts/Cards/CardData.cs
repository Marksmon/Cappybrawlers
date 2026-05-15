using System.Collections.Generic;
using UnityEngine;

namespace Capybrawlers.Cards
{
    [CreateAssetMenu(fileName = "Card_", menuName = "Capybrawlers/Card Data")]
    public class CardData : ScriptableObject
    {
        public string id;
        public string cardName;
        [Range(0, 2)] public int staminaCost;
        public Sprite artwork;
        [TextArea(2, 5)] public string effectDescription;
        public List<CardEffect> effects;
    }
}
