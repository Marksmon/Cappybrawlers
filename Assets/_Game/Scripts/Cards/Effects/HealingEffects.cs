using Capybrawlers.Core;
using UnityEngine;

namespace Capybrawlers.Cards.Effects
{
    // Heal: restore ATK×0.5 HP to self.
    [CreateAssetMenu(fileName = "Effect_Heal", menuName = "Capybrawlers/Card Effects/Heal")]
    public class HealEffect : CardEffect
    {
        public override void Execute(EffectContext ctx)
        {
            var source = ctx.Source as IBrawlerInstance;
            if (source == null) return;
            source.RestoreHP(Mathf.RoundToInt(source.CurrentATK * 0.5f));
        }
    }



}
