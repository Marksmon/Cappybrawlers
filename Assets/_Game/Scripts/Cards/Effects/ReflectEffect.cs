using Capybrawlers.Core;
using UnityEngine;

namespace Capybrawlers.Cards.Effects
{
    [CreateAssetMenu(fileName = "Effect_Reflect", menuName = "Capybrawlers/Card Effects/Reflect")]
    public class ReflectEffect : CardEffect
    {
        public int reflectPercent;
        public int duration;

        public override void Execute(EffectContext ctx)
        {
            var source = ctx.Source as IBrawlerInstance;
            source?.ApplyStatusEffect(new StatusEffect(MechanicType.Reflect, reflectPercent, duration));
        }
    }
}
