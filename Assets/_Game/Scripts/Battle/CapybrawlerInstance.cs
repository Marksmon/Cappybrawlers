using System;
using System.Collections.Generic;
using Capybrawlers.Cards;
using Capybrawlers.Core;
using Capybrawlers.Creatures;

namespace Capybrawlers.Battle
{
    public class CapybrawlerInstance : IBrawlerInstance
    {
        public ResolvedBuild Build { get; }

        public int  CurrentHP      { get; private set; }
        public int  MaxHP          => Build.FinalHP;
        public bool IsKnockedOut   => CurrentHP <= 0;
        public bool IsInTerminalRally { get; set; }

        public float MissingHpPercent      => 1f - (float)CurrentHP / MaxHP;
        public bool  IsHpBelowPercent(float p) => (float)CurrentHP / MaxHP < p;

        // Computed stats: base + active buff/debuff totals.
        public int CurrentATK => Build.FinalATK + SumValues(MechanicType.Rage) + SumValues(MechanicType.Berserker);
        public int CurrentDEF => Build.FinalDEF;
        public int CurrentSPD => Build.FinalSPD + SumValues(MechanicType.Haste) + SumValues(MechanicType.Slow);

        public IReadOnlyList<ActiveEffect> ActiveEffects => _effects;
        public List<QueuedAction>          QueuedActions  { get; } = new();

        private readonly List<ActiveEffect> _effects = new();
        private int _shieldHP;

        public CapybrawlerInstance(ResolvedBuild build)
        {
            Build     = build;
            CurrentHP = build.FinalHP;
        }

        public int TakeDamage(int amount)
        {
            int effective = Math.Max(1, amount - CurrentDEF);
            return AbsorbAndApply(effective);
        }

        public int TakeDamageIgnoreArmor(int amount) => AbsorbAndApply(Math.Max(0, amount));

        public void RestoreHP(int amount) =>
            CurrentHP = Math.Min(MaxHP, CurrentHP + amount);

        public void ApplyStatusEffect(StatusEffect effect)
        {
            if (effect.Type == MechanicType.Shield)
            {
                _shieldHP += effect.Value;
                return;
            }
            _effects.Add(new ActiveEffect(effect));
        }

        public void RemoveNegativeEffects() =>
            _effects.RemoveAll(e => ActiveEffect.IsNegative(e.Type));

        // Called at StartOfTurn: apply DoT then decrement and remove expired effects.
        public void TickEffects()
        {
            foreach (var e in _effects)
                if (e.Type == MechanicType.Burn || e.Type == MechanicType.Poison)
                    AbsorbAndApply(e.Value);

            for (int i = _effects.Count - 1; i >= 0; i--)
            {
                _effects[i].RemainingTurns--;
                if (_effects[i].RemainingTurns <= 0)
                    _effects.RemoveAt(i);
            }
        }

        public bool HasEffect(MechanicType type)
        {
            foreach (var e in _effects)
                if (e.Type == type) return true;
            return false;
        }

        private int AbsorbAndApply(int amount)
        {
            if (_shieldHP > 0)
            {
                int absorbed = Math.Min(_shieldHP, amount);
                _shieldHP -= absorbed;
                amount    -= absorbed;
            }
            int actual = Math.Min(amount, CurrentHP);
            CurrentHP -= actual;
            return actual;
        }

        private int SumValues(MechanicType type)
        {
            int sum = 0;
            foreach (var e in _effects)
                if (e.Type == type) sum += e.Value;
            return sum;
        }
    }
}
