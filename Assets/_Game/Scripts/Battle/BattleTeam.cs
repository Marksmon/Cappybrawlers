using System.Collections.Generic;
using Capybrawlers.Cards;
using Capybrawlers.Core;
using Capybrawlers.Creatures;

namespace Capybrawlers.Battle
{
    public class BattleTeam : ITeamState
    {
        public CapybrawlerInstance[] Brawlers { get; }
        public StaminaPool           StaminaPool { get; }
        public CardPool              CardPool { get; }
        public Hand                  Hand { get; }

        // ITeamState
        IStaminaPool                        ITeamState.Stamina        => StaminaPool;
        IReadOnlyList<IBrawlerInstance>     ITeamState.ActiveBrawlers => ActiveBrawlers;

        public IReadOnlyList<CapybrawlerInstance> ActiveBrawlers
        {
            get
            {
                var alive = new List<CapybrawlerInstance>(3);
                foreach (var b in Brawlers)
                    if (!b.IsKnockedOut) alive.Add(b);
                return alive;
            }
        }

        public bool IsDefeated
        {
            get
            {
                foreach (var b in Brawlers)
                    if (!b.IsKnockedOut) return false;
                return true;
            }
        }

        public BattleTeam(ResolvedBuild[] builds, IEventBus events, bool isPlayerTeam)
        {
            Brawlers = new CapybrawlerInstance[builds.Length];
            for (int i = 0; i < builds.Length; i++)
                Brawlers[i] = new CapybrawlerInstance(builds[i]);

            StaminaPool = new StaminaPool(max: 6, regenPerTurn: 2, startingAmount: 3, events, isPlayerTeam);
            CardPool    = new CardPool(Brawlers);
            Hand        = new Hand();
        }
    }
}
