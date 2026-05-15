using Capybrawlers.Cards;
using Capybrawlers.Core;

namespace Capybrawlers.Battle
{
    // Mutable runtime tracker for a status effect applied to a brawler.
    // StatusEffect (Cards assembly) is the immutable descriptor used to apply;
    // ActiveEffect is what lives on CapybrawlerInstance during a battle.
    public class ActiveEffect
    {
        public MechanicType Type;
        public int          Value;
        public int          RemainingTurns;

        public ActiveEffect(StatusEffect source)
        {
            Type           = source.Type;
            Value          = source.Value;
            RemainingTurns = source.Duration;
        }

        public static bool IsNegative(MechanicType type) => type switch
        {
            MechanicType.Burn     => true,
            MechanicType.Poison   => true,
            MechanicType.Slow     => true,
            MechanicType.Stun     => true,
            MechanicType.FullStun => true,
            _                     => false,
        };
    }
}
