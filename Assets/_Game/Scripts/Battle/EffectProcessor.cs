using Capybrawlers.Cards;
using Capybrawlers.Core;

namespace Capybrawlers.Battle
{
    public static class EffectProcessor
    {
        // Executes all effects on a card and returns total damage dealt to the target.
        public static int Process(
            QueuedAction action,
            BattleTeam playerTeam,
            BattleTeam opponentTeam,
            IEventBus events)
        {
            bool sourceIsPlayer = IsOnTeam(action.Source, playerTeam);
            var  ownTeam        = sourceIsPlayer ? playerTeam  : opponentTeam;
            var  enemyTeam      = sourceIsPlayer ? opponentTeam : playerTeam;

            var ctx = new EffectContext(
                source:       action.Source,
                target:       action.Target,
                playerTeam:   ownTeam,
                opponentTeam: enemyTeam);

            if (action.Card == null)
            {
                UnityEngine.Debug.LogWarning("[EffectProcessor] QueuedAction has null Card — skipping. Run Generate SO Assets.");
                events.Publish(new ActionResolvedEvent(action, 0));
                return 0;
            }

            int hpBefore = action.Target?.CurrentHP ?? 0;

            if (action.Card.effects != null)
                foreach (var effect in action.Card.effects)
                {
                    if (effect == null) { UnityEngine.Debug.LogWarning($"[EffectProcessor] Null effect entry on card '{action.Card.id}' — run Generate SO Assets."); continue; }
                    effect.Execute(ctx);
                }

            int hpAfter   = action.Target?.CurrentHP ?? 0;
            int damage    = hpBefore - hpAfter;

            events.Publish(new ActionResolvedEvent(action, damage));
            return damage;
        }

        private static bool IsOnTeam(CapybrawlerInstance brawler, BattleTeam team)
        {
            foreach (var b in team.Brawlers)
                if (b == brawler) return true;
            return false;
        }
    }
}
