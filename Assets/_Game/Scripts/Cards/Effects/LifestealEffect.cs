using Capybrawlers.Core;
using UnityEngine;

namespace Capybrawlers.Cards.Effects
{
    [CreateAssetMenu(fileName = "Effect_Lifesteal", menuName = "Capybrawlers/Card Effects/Lifesteal")]
    public class LifestealEffect : CardEffect
    {
        public override void Execute(EffectContext ctx)
        {
            var source = ctx.Source as IBrawlerInstance;
            var target = ctx.Target as IBrawlerInstance;
            if (source == null || target == null) return;
            int dealt = target.TakeDamage(source.CurrentATK);
            source.RestoreHP(dealt / 2);
        }
    }
}
