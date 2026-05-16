using Capybrawlers.Core;
using UnityEngine;

namespace Capybrawlers.Cards.Effects
{
    [CreateAssetMenu(fileName = "Effect_Purge", menuName = "Capybrawlers/Card Effects/Purge")]
    public class PurgeEffect : CardEffect
    {
        public override void Execute(EffectContext ctx)
        {
            var source = ctx.Source as IBrawlerInstance;
            source?.RemoveNegativeEffects();
        }
    }
}
