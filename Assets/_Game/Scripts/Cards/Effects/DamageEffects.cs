using Capybrawlers.Core;
using UnityEngine;

namespace Capybrawlers.Cards.Effects
{
    // Burn: deal damage to target equal to a flat amount; apply Burn status (damage per turn).
    [CreateAssetMenu(fileName = "Effect_Burn", menuName = "Capybrawlers/Card Effects/Burn")]
    public class BurnEffect : CardEffect
    {
        public int damage;
        public int burnDamagePerTurn;
        public int burnDuration;

        public override void Execute(EffectContext ctx)
        {
            var target = ctx.Target as IBrawlerInstance;
            if (target == null) return;
            target.TakeDamage(damage);
            target.ApplyStatusEffect(new StatusEffect(MechanicType.Burn, burnDamagePerTurn, burnDuration));
        }
    }

    // Pierce: deal damage that ignores DEF entirely.
    [CreateAssetMenu(fileName = "Effect_Pierce", menuName = "Capybrawlers/Card Effects/Pierce")]
    public class PierceEffect : CardEffect
    {
        public int damage;

        public override void Execute(EffectContext ctx)
        {
            var target = ctx.Target as IBrawlerInstance;
            target?.TakeDamageIgnoreArmor(damage);
        }
    }

    // Execute: deal bonus damage if target HP is below threshold %.
    [CreateAssetMenu(fileName = "Effect_Execute", menuName = "Capybrawlers/Card Effects/Execute")]
    public class ExecuteEffect : CardEffect
    {
        public int baseDamage;
        public int bonusDamage;
        [Range(0f, 1f)] public float hpThreshold;

        public override void Execute(EffectContext ctx)
        {
            var target = ctx.Target as IBrawlerInstance;
            if (target == null) return;
            int dmg = target.IsHpBelowPercent(hpThreshold) ? baseDamage + bonusDamage : baseDamage;
            target.TakeDamage(dmg);
        }
    }

    // Poison: apply a stacking damage-over-time debuff.
    [CreateAssetMenu(fileName = "Effect_Poison", menuName = "Capybrawlers/Card Effects/Poison")]
    public class PoisonEffect : CardEffect
    {
        public int poisonDamagePerTurn;
        public int duration;

        public override void Execute(EffectContext ctx)
        {
            var target = ctx.Target as IBrawlerInstance;
            target?.ApplyStatusEffect(new StatusEffect(MechanicType.Poison, poisonDamagePerTurn, duration));
        }
    }
}
