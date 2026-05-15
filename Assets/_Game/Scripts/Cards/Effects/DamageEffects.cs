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

    // Pierce: deal ATK damage that ignores DEF entirely.
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

    // Execute: deal ATK×1.5 damage; bonus ATK×0.5 if target HP < 30%.
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

    // Poison: apply a damage-over-time debuff.
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
