using System.Collections.Generic;
using Capybrawlers.Core;

namespace Capybrawlers.Cards
{
    // Minimal view of a brawler needed by CardEffect implementations.
    // Concrete type lives in Battle assembly and implements this interface.
    public interface IBrawlerInstance
    {
        int CurrentHP { get; }
        int MaxHP     { get; }
        float MissingHpPercent { get; }
        bool IsHpBelowPercent(float percent);

        // Returns actual damage dealt after shields/armor.
        int TakeDamage(int amount);
        int TakeDamageIgnoreArmor(int amount);
        void RestoreHP(int amount);

        void ApplyStatusEffect(StatusEffect effect);
        void RemoveNegativeEffects();
    }

    // Minimal view of a team needed by CardEffect implementations.
    public interface ITeamState
    {
        IStaminaPool Stamina { get; }
        IReadOnlyList<IBrawlerInstance> ActiveBrawlers { get; }
    }

    public interface IStaminaPool
    {
        int Current { get; }
        int Max     { get; }
        // Drains up to 'amount' and returns how much was actually taken.
        int Steal(int amount);
        void Add(int amount);
        bool TrySpend(int amount);
    }

    // Value-type descriptor applied to a brawler for N turns.
    public readonly struct StatusEffect
    {
        public readonly MechanicType Type;
        public readonly int          Value;    // meaning varies by type (damage, bonus, %, etc.)
        public readonly int          Duration; // turns remaining

        public StatusEffect(MechanicType type, int value, int duration)
        {
            Type     = type;
            Value    = value;
            Duration = duration;
        }
    }
}
