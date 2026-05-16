using Capybrawlers.Core;
using UnityEngine;

namespace Capybrawlers.Cards.Effects
{
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
}
