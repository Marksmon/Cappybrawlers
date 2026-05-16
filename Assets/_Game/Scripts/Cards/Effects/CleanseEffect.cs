using Capybrawlers.Core;
using UnityEngine;

namespace Capybrawlers.Cards.Effects
{
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
