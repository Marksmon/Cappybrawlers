using Capybrawlers.Core;
using UnityEngine;

namespace Capybrawlers.Creatures
{
    [CreateAssetMenu(fileName = "Nature_", menuName = "Capybrawlers/Nature Data")]
    public class NatureData : ScriptableObject
    {
        public NatureType natureType;
        public int baseHP;
        public int baseATK;
        public int baseDEF;
        public int baseSPD;
        [TextArea(2, 4)] public string roleDescription;
        public Sprite idleSprite;
        public RuntimeAnimatorController animatorController;
    }
}
