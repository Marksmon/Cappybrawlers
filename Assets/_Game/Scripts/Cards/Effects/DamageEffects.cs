using Capybrawlers.Core;
using UnityEngine;

namespace Capybrawlers.Cards.Effects
{
    // Burn: deal ATK damage to target and apply Burn (damage per turn for N turns).
    [CreateAssetMenu(fileName = "Effect_Burn", menuName = "Capybrawlers/Card Effects/Burn")]
    public class BurnEffect : CardEffect
    {
        public int burnDamagePerTurn = 2;
        public int burnDuration      = 3;

        public override void Execute(EffectContext ctx)
        {
            var source = ctx.Source as IBrawlerInstance;
            var target = ctx.Target as IBrawlerInstance;
            if (source == null || target == null) return;
            target.TakeDamage(source.CurrentATK);
            target.ApplyStatusEffect(new StatusEffect(MechanicType.Burn, burnDamagePerTurn, burnDuration));
        }
    }


}
