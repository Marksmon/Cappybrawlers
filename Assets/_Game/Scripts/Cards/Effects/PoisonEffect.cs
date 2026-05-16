using Capybrawlers.Core;
using UnityEngine;

namespace Capybrawlers.Cards.Effects
{
    [CreateAssetMenu(fileName = "Effect_Poison", menuName = "Capybrawlers/Card Effects/Poison")]
    public class PoisonEffect : CardEffect
    {
        public int poisonDamagePerTurn = 4;
        public int duration            = 4;

        public override void Execute(EffectContext ctx)
        {
            var target = ctx.Target as IBrawlerInstance;
            target?.ApplyStatusEffect(new StatusEffect(MechanicType.Poison, poisonDamagePerTurn, duration));
        }
    }
}
