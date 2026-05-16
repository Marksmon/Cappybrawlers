using Capybrawlers.Core;
using UnityEngine;

namespace Capybrawlers.Cards.Effects
{
    [CreateAssetMenu(fileName = "Effect_Slow", menuName = "Capybrawlers/Card Effects/Slow")]
    public class SlowEffect : CardEffect
    {
        public int spdReduction;
        public int duration;

        public override void Execute(EffectContext ctx)
        {
            var target = ctx.Target as IBrawlerInstance;
            target?.ApplyStatusEffect(new StatusEffect(MechanicType.Slow, -spdReduction, duration));
        }
    }
}
