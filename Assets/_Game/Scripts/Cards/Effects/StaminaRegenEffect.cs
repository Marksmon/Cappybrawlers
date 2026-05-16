using Capybrawlers.Core;
using UnityEngine;

namespace Capybrawlers.Cards.Effects
{
    [CreateAssetMenu(fileName = "Effect_StaminaRegen", menuName = "Capybrawlers/Card Effects/StaminaRegen")]
    public class StaminaRegenEffect : CardEffect
    {
        public int amount;

        public override void Execute(EffectContext ctx)
        {
            var player = ctx.PlayerTeam as ITeamState;
            player?.Stamina.Add(amount);
        }
    }
}
