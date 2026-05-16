using Capybrawlers.Core;
using UnityEngine;

namespace Capybrawlers.Cards.Effects
{
    [CreateAssetMenu(fileName = "Effect_Execute", menuName = "Capybrawlers/Card Effects/Execute")]
    public class ExecuteEffect : CardEffect
    {
        public override void Execute(EffectContext ctx)
        {
            var source = ctx.Source as IBrawlerInstance;
            var target = ctx.Target as IBrawlerInstance;
            if (source == null || target == null) return;
            int dmg = Mathf.RoundToInt(source.CurrentATK * 1.5f);
            if (target.IsHpBelowPercent(0.3f))
                dmg += Mathf.RoundToInt(source.CurrentATK * 0.5f);
            target.TakeDamage(dmg);
        }
    }
}
