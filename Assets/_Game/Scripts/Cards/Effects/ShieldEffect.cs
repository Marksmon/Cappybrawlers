using Capybrawlers.Core;
using UnityEngine;

namespace Capybrawlers.Cards.Effects
{
    [CreateAssetMenu(fileName = "Effect_Shield", menuName = "Capybrawlers/Card Effects/Shield")]
    public class ShieldEffect : CardEffect
    {
        public int shieldAmount;
        public int duration;

        public override void Execute(EffectContext ctx)
        {
            var source = ctx.Source as IBrawlerInstance;
            source?.ApplyStatusEffect(new StatusEffect(MechanicType.Shield, shieldAmount, duration));
        }
    }
}
