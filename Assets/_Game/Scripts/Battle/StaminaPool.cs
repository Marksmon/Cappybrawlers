using System;
using Capybrawlers.Cards;
using Capybrawlers.Core;

namespace Capybrawlers.Battle
{
    public class StaminaPool : IStaminaPool
    {
        public int Current      { get; private set; }
        public int Max          { get; }
        public int RegenPerTurn { get; }

        private readonly IEventBus _events;
        private readonly bool      _publishEvents; // only player team publishes (hidden from opponent)

        public StaminaPool(int max, int regenPerTurn, int startingAmount, IEventBus events, bool publishEvents)
        {
            Max            = max;
            RegenPerTurn   = regenPerTurn;
            Current        = Math.Clamp(startingAmount, 0, max);
            _events        = events;
            _publishEvents = publishEvents;
        }

        public bool TrySpend(int amount)
        {
            if (Current < amount) return false;
            Current -= amount;
            Publish(-amount);
            return true;
        }

        public void Add(int amount)
        {
            int before = Current;
            Current = Math.Clamp(Current + amount, 0, Max);
            Publish(Current - before);
        }

        public int Steal(int amount)
        {
            int stolen = Math.Min(Current, amount);
            Current -= stolen;
            if (stolen > 0) Publish(-stolen);
            return stolen;
        }

        public void Regen()
        {
            Add(RegenPerTurn);
        }

        private void Publish(int delta)
        {
            if (_publishEvents)
                _events?.Publish(new StaminaChangedEvent(Current, delta));
        }
    }
}
