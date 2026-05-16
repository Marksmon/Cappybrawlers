using Capybrawlers.Core;
using UnityEngine;

namespace Capybrawlers.Cards.Effects
{
    // StaminaSteal: drain Stamina from opponent team and add it to own team.
    [CreateAssetMenu(fileName = "Effect_StaminaSteal", menuName = "Capybrawlers/Card Effects/StaminaSteal")]
    public class StaminaStealEffect : CardEffect
    {
        public int amount;

        public override void Execute(EffectContext ctx)
        {
            var opponent = ctx.OpponentTeam as ITeamState;
            var player   = ctx.PlayerTeam  as ITeamState;
            if (opponent == null || player == null) return;
            int stolen = opponent.Stamina.Steal(amount);
            player.Stamina.Add(stolen);
        }
    }


}
