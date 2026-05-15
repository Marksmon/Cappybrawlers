using Capybrawlers.Core;
using UnityEngine;

namespace Capybrawlers.Cards.Effects
{
    // Heal: restore HP to source.
    [CreateAssetMenu(fileName = "Effect_Heal", menuName = "Capybrawlers/Card Effects/Heal")]
    public class HealEffect : CardEffect
    {
        public int healAmount;

        public override void Execute(EffectContext ctx)
        {
            var source = ctx.Source as IBrawlerInstance;
            source?.RestoreHP(healAmount);
        }
    }

    // Lifesteal: deal damage to target; restore same amount to source.
    [CreateAssetMenu(fileName = "Effect_Lifesteal", menuName = "Capybrawlers/Card Effects/Lifesteal")]
    public class LifestealEffect : CardEffect
    {
        public int damage;

        public override void Execute(EffectContext ctx)
        {
            var source = ctx.Source as IBrawlerInstance;
            var target = ctx.Target as IBrawlerInstance;
            if (source == null || target == null) return;
            int dealt = target.TakeDamage(damage);
            source.RestoreHP(dealt);
        }
    }

    // Purge: remove all negative status effects from source.
    [CreateAssetMenu(fileName = "Effect_Purge", menuName = "Capybrawlers/Card Effects/Purge")]
    public class PurgeEffect : CardEffect
    {
        public override void Execute(EffectContext ctx)
        {
            var source = ctx.Source as IBrawlerInstance;
            source?.RemoveNegativeEffects();
        }
    }

    // Cleanse: remove all negative status effects from all friendly brawlers.
    [CreateAssetMenu(fileName = "Effect_Cleanse", menuName = "Capybrawlers/Card Effects/Cleanse")]
    public class CleanseEffect : CardEffect
    {
        public override void Execute(EffectContext ctx)
        {
            var team = ctx.PlayerTeam as ITeamState;
            if (team == null) return;
            foreach (var brawler in team.ActiveBrawlers)
                brawler.RemoveNegativeEffects();
        }
    }
}
