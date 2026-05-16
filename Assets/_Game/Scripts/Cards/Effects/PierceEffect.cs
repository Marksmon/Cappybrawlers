using Capybrawlers.Core;
using UnityEngine;

namespace Capybrawlers.Cards.Effects
{
    [CreateAssetMenu(fileName = "Effect_Pierce", menuName = "Capybrawlers/Card Effects/Pierce")]
    public class PierceEffect : CardEffect
    {
        public override void Execute(EffectContext ctx)
        {
            var source = ctx.Source as IBrawlerInstance;
            var target = ctx.Target as IBrawlerInstance;
            if (source == null || target == null) return;
            target.TakeDamageIgnoreArmor(source.CurrentATK);
        }
    }
}
