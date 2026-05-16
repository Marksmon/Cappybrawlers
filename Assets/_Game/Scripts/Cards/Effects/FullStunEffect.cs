using Capybrawlers.Core;
using UnityEngine;

namespace Capybrawlers.Cards.Effects
{
    [CreateAssetMenu(fileName = "Effect_FullStun", menuName = "Capybrawlers/Card Effects/FullStun")]
    public class FullStunEffect : CardEffect
    {
        public override void Execute(EffectContext ctx)
        {
            var target = ctx.Target as IBrawlerInstance;
            target?.ApplyStatusEffect(new StatusEffect(MechanicType.FullStun, 0, 1));
        }
    }
}
