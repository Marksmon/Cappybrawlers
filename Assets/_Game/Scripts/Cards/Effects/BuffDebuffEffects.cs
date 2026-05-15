using Capybrawlers.Core;
using UnityEngine;

namespace Capybrawlers.Cards.Effects
{
    // Rage: raise source ATK for N turns.
    [CreateAssetMenu(fileName = "Effect_Rage", menuName = "Capybrawlers/Card Effects/Rage")]
    public class RageEffect : CardEffect
    {
        public int atkBonus;
        public int duration;

        public override void Execute(EffectContext ctx)
        {
            var source = ctx.Source as IBrawlerInstance;
            source?.ApplyStatusEffect(new StatusEffect(MechanicType.Rage, atkBonus, duration));
        }
    }

    // Berserker: source gains ATK proportional to missing HP.
    [CreateAssetMenu(fileName = "Effect_Berserker", menuName = "Capybrawlers/Card Effects/Berserker")]
    public class BerserkerEffect : CardEffect
    {
        public int atkBonusPerMissingHpPercent;

        public override void Execute(EffectContext ctx)
        {
            var source = ctx.Source as IBrawlerInstance;
            if (source == null) return;
            int bonus = Mathf.RoundToInt(source.MissingHpPercent * atkBonusPerMissingHpPercent);
            source.ApplyStatusEffect(new StatusEffect(MechanicType.Berserker, bonus, 1));
        }
    }

    // Haste: raise source SPD for N turns.
    [CreateAssetMenu(fileName = "Effect_Haste", menuName = "Capybrawlers/Card Effects/Haste")]
    public class HasteEffect : CardEffect
    {
        public int spdBonus;
        public int duration;

        public override void Execute(EffectContext ctx)
        {
            var source = ctx.Source as IBrawlerInstance;
            source?.ApplyStatusEffect(new StatusEffect(MechanicType.Haste, spdBonus, duration));
        }
    }

    // Slow: lower target SPD for N turns.
    [CreateAssetMenu(fileName = "Effect_Slow", menuName = "Capybrawlers/Card Effects/Slow")]
    public class SlowEffect : CardEffect
    {
        public int spdReduction;
        public int duration;

        public override void Execute(EffectContext ctx)
        {
            var target = ctx.Target as IBrawlerInstance;
            target?.ApplyStatusEffect(new StatusEffect(MechanicType.Slow, -spdReduction, duration));
        }
    }

    // Shield: grant source a damage-absorbing shield for N turns.
    [CreateAssetMenu(fileName = "Effect_Shield", menuName = "Capybrawlers/Card Effects/Shield")]
    public class ShieldEffect : CardEffect
    {
        public int shieldAmount;
        public int duration;

        public override void Execute(EffectContext ctx)
        {
            var source = ctx.Source as IBrawlerInstance;
            source?.ApplyStatusEffect(new StatusEffect(MechanicType.Shield, shieldAmount, duration));
        }
    }

    // Reflect: source reflects a portion of incoming damage back to attacker for N turns.
    [CreateAssetMenu(fileName = "Effect_Reflect", menuName = "Capybrawlers/Card Effects/Reflect")]
    public class ReflectEffect : CardEffect
    {
        public int reflectPercent;
        public int duration;

        public override void Execute(EffectContext ctx)
        {
            var source = ctx.Source as IBrawlerInstance;
            source?.ApplyStatusEffect(new StatusEffect(MechanicType.Reflect, reflectPercent, duration));
        }
    }
}
