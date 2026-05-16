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


}
