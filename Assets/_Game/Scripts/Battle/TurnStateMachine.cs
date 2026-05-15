using System.Collections;
using System.Collections.Generic;
using Capybrawlers.Core;
using UnityEngine;

namespace Capybrawlers.Battle
{
    // Turn loop:  [initial 3-card draw] → Decision(15s) → Action → Draw(1) → Decision → …
    // Drawing Phase: draw 1 card per turn. If hand is full (5), player has 5s to discard first.
    // Turn 10+: penalty damage (10 × (turn−9)) applied to every brawler before Decision.
    // Brawler KO: their cards are permanently removed; last survivor gets cooldowns lifted.
    public class TurnStateMachine
    {
        public TurnPhase CurrentPhase { get; private set; } = TurnPhase.Idle;
        public int        CurrentTurn  { get; private set; } = 1;

        private readonly BattleTeam   _player;
        private readonly BattleTeam   _opponent;
        private readonly IEventBus    _events;
        private readonly MonoBehaviour _runner;

        private bool _queueConfirmed;
        private bool _discardCompleted;

        public TurnStateMachine(BattleTeam player, BattleTeam opponent,
            IEventBus events, MonoBehaviour runner)
        {
            _player   = player;
            _opponent = opponent;
            _events   = events;
            _runner   = runner;
        }

        public void StartBattle() => _runner.StartCoroutine(Co_RunBattle());

        // Called by UI confirm (Lock In / End Turn).
        public void ConfirmQueue() => _queueConfirmed = true;

        // Called by BattleManager after the player discards a card in Drawing Phase.
        public void NotifyDiscardComplete() => _discardCompleted = true;

        // ── Main loop ─────────────────────────────────────────────────────────

        private IEnumerator Co_RunBattle()
        {
            SetPhase(TurnPhase.Setup);
            yield return null;

            // Initial draw: 3 cards each.
            _player.Hand.DrawN(_player.CardPool, 3);
            _opponent.Hand.DrawN(_opponent.CardPool, 3);
            SetPhase(TurnPhase.DrawPhase);
            yield return null;

            while (!_player.IsDefeated && !_opponent.IsDefeated)
            {
                _events.Publish(new TurnChangedEvent(CurrentTurn));

                // Start-of-turn bookkeeping: cooldowns, regen, DoT effects.
                _player.CardPool.TickCooldowns();
                _opponent.CardPool.TickCooldowns();
                _player.StaminaPool.Regen();
                _opponent.StaminaPool.Regen();
                foreach (var b in _player.Brawlers)   b.TickEffects();
                foreach (var b in _opponent.Brawlers) b.TickEffects();
                ProcessNewDeaths();

                // Penalty damage from turn 10 onward; scales +10 each turn.
                if (CurrentTurn >= 10)
                {
                    int dmg = (CurrentTurn - 9) * 10;
                    foreach (var b in _player.Brawlers)   b.TakeDamage(dmg);
                    foreach (var b in _opponent.Brawlers) b.TakeDamage(dmg);
                    _events.Publish(new PenaltyDamageEvent(CurrentTurn, dmg));
                    ProcessNewDeaths();
                }

                if (_player.IsDefeated || _opponent.IsDefeated) break;

                yield return Co_DecisionPhase();
                yield return Co_ActionPhase();

                if (_player.IsDefeated || _opponent.IsDefeated) break;

                foreach (var b in _player.Brawlers)   b.QueuedActions.Clear();
                foreach (var b in _opponent.Brawlers) b.QueuedActions.Clear();

                yield return Co_DrawPhase();

                CurrentTurn++;
            }

            SetPhase(TurnPhase.BattleEnd);
            _events.Publish(new BattleEndEvent(!_player.IsDefeated));
        }

        // ── Phases ────────────────────────────────────────────────────────────

        private IEnumerator Co_DecisionPhase()
        {
            SetPhase(TurnPhase.DecisionPhase);
            _queueConfirmed = false;
            float timer = 15f;

            while (timer > 0f && !_queueConfirmed)
            {
                timer -= Time.deltaTime;
                _events.Publish(new DecisionTimerEvent(Mathf.Max(0f, timer)));
                yield return null;
            }

            if (!_queueConfirmed)
                _events.Publish(new DecisionTimerExpiredEvent());
        }

        private IEnumerator Co_ActionPhase()
        {
            SetPhase(TurnPhase.ActionPhase);
            var ordered = SPDResolver.Resolve(_player, _opponent);

            while (ordered.Count > 0)
            {
                var action = ordered[0];
                ordered.RemoveAt(0);

                if (action.Source.HasEffect(MechanicType.FullStun)) continue;
                if (action.Source.HasEffect(MechanicType.Stun) && IsLowestPriority(action, ordered)) continue;
                if (action.Source.IsKnockedOut && !action.Source.IsInTerminalRally) continue;

                EffectProcessor.Process(action, _player, _opponent, _events);

                TerminalRallyHandler.CheckAndInsertRallyActions(
                    action.Source, ordered, _player, _opponent, _events);

                yield return null; // one frame per action for animation
            }
        }

        private IEnumerator Co_DrawPhase()
        {
            SetPhase(TurnPhase.DrawPhase);

            // AI draws 1 card (auto-discards oldest if hand full).
            FillHandAI(_opponent);

            // Player draws 1 card (may require discard with 5-second timer).
            yield return Co_FillHandPlayer();

            yield return null;
        }

        private IEnumerator Co_FillHandPlayer()
        {
            var eligible = _player.CardPool.GetEligible();
            if (eligible.Count == 0) yield break; // nothing available to draw

            if (_player.Hand.Cards.Count < Hand.MaxSize)
            {
                // Hand has room — draw immediately.
                _player.Hand.DrawOne(_player.CardPool);
                yield break;
            }

            // Hand is full — player must discard one card before drawing.
            _discardCompleted = false;
            _events.Publish(new DiscardRequiredEvent());

            float timer = 5f;
            while (timer > 0f && !_discardCompleted)
            {
                timer -= Time.deltaTime;
                _events.Publish(new DrawPhaseTimerEvent(Mathf.Max(0f, timer)));
                yield return null;
            }

            if (!_discardCompleted)
            {
                // Timer expired — auto-discard the oldest card in hand.
                var oldest = _player.Hand.Cards[0];
                _player.Hand.RemoveCard(oldest);
                _player.CardPool.StartCooldown(oldest);
                _events.Publish(new DrawPhaseTimerExpiredEvent());
            }

            _player.Hand.DrawOne(_player.CardPool);
        }

        // ── AI helpers ────────────────────────────────────────────────────────

        private static void FillHandAI(BattleTeam team)
        {
            var eligible = team.CardPool.GetEligible();
            if (eligible.Count == 0) return;

            if (team.Hand.Cards.Count >= Hand.MaxSize)
            {
                // Auto-discard oldest card to make room.
                var discard = team.Hand.Cards[0];
                team.CardPool.StartCooldown(discard);
                team.Hand.RemoveCard(discard);
            }
            team.Hand.DrawOne(team.CardPool);
        }

        // ── Death processing ──────────────────────────────────────────────────

        // Processes any newly knocked-out brawlers outside the action phase
        // (DoT from TickEffects, penalty damage). Action-phase deaths are handled
        // by TerminalRallyHandler which also calls OnBrawlerKnockedOut.
        private void ProcessNewDeaths()
        {
            foreach (var b in _player.Brawlers)
            {
                if (b.IsKnockedOut && !b.IsInTerminalRally)
                {
                    b.IsInTerminalRally = true;
                    _events.Publish(new BrawlerKnockedOutEvent(b, 0));
                    _player.OnBrawlerKnockedOut(b);
                }
            }
            foreach (var b in _opponent.Brawlers)
            {
                if (b.IsKnockedOut && !b.IsInTerminalRally)
                {
                    b.IsInTerminalRally = true;
                    _events.Publish(new BrawlerKnockedOutEvent(b, 1));
                    _opponent.OnBrawlerKnockedOut(b);
                }
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void SetPhase(TurnPhase phase)
        {
            CurrentPhase = phase;
            _events.Publish(new TurnPhaseChangedEvent(phase));
        }

        private static bool IsLowestPriority(QueuedAction action, List<QueuedAction> remaining)
        {
            foreach (var a in remaining)
                if (a.Source == action.Source) return false;
            return true;
        }
    }
}
