using Capybrawlers.Core;
using UnityEngine;

namespace Capybrawlers.Cards.Effects
{
    // Stun: target loses its lowest-priority queued action this turn.
    [CreateAssetMenu(fileName = "Effect_Stun", menuName = "Capybrawlers/Card Effects/Stun")]
    public class StunEffect : CardEffect
    {
        public override void Execute(EffectContext ctx)
        {
            var target = ctx.Target as IBrawlerInstance;
            target?.ApplyStatusEffect(new StatusEffect(MechanicType.Stun, 0, 1));
        }
    }


}
