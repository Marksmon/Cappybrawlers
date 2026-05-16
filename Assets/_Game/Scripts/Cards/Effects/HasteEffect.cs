using Capybrawlers.Core;
using UnityEngine;

namespace Capybrawlers.Cards.Effects
{
    [CreateAssetMenu(fileName = "Effect_Haste", menuName = "Capybrawlers/Card Effects/Haste")]
    public class HasteEffect : CardEffect
    {
        public int spdBonus;
        public int duration;

        public override void Execute(EffectContext ctx)
        {
            var source = ctx.Source as IBrawlerInstance;
            source?.ApplyStatusEffect(new StatusEffect(MechanicType.Haste, spdBonus, duration));
        }
    }
}
