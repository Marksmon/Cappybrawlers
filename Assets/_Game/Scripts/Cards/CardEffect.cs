using Capybrawlers.Core;
using UnityEngine;

namespace Capybrawlers.Cards
{
    public abstract class CardEffect : ScriptableObject
    {
        public MechanicType mechanic;

        // Called during Resolution phase by EffectProcessor.
        // context provides source, target, and both teams' state.
        public abstract void Execute(EffectContext context);
    }

    // Passed to CardEffect.Execute — carries everything needed to resolve an effect.
    public class EffectContext
    {
        public object Source { get; }   // CapybrawlerInstance (typed in Battle assembly)
        public object Target { get; }   // CapybrawlerInstance or null
        public object PlayerTeam { get; }
        public object OpponentTeam { get; }

        public EffectContext(object source, object target, object playerTeam, object opponentTeam)
        {
            Source       = source;
            Target       = target;
            PlayerTeam   = playerTeam;
            OpponentTeam = opponentTeam;
        }
    }
}
