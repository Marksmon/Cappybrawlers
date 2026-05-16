using System.Collections;
using System.Collections.Generic;
using Capybrawlers.Core;
using UnityEngine;

namespace Capybrawlers.Battle
{
    // Turn loop:  [initial 6-card draw] → Decision(15s) → Action → Draw(3) → Decision → …
    // Drawing Phase: draw 3 cards per turn; auto-discard oldest if hand would exceed max.
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
        private bool _actionAnimDone;

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

        // Called by ResolutionAnimator when the damage popup animation finishes.
        public void NotifyActionAnimDone() => _actionAnimDone = true;

        // ── Main loop ─────────────────────────────────────────────────────────

        private IEnumerator Co_RunBattle()
        {
            SetPhase(TurnPhase.Setup);
            yield return null;

            // Initial draw: 6 cards each.
            _player.Hand.DrawN(_player.CardPool, 6);
            _opponent.Hand.DrawN(_opponent.CardPool, 6);
            SetPhase(TurnPhase.DrawPhase);
            yield return null;

            while (!_player.IsDefeated && !_opponent.IsDefeated)
            {
                _events.Publish(new TurnChangedEvent(CurrentTurn));

                // Start-of-turn bookkeeping: cooldowns, DoT effects.
                _player.CardPool.TickCooldowns();
                _opponent.CardPool.TickCooldowns();
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

                // Regen after actions so turn 1 starts with 3, turn 2 starts with 5.
                _player.StaminaPool.Regen();
                _opponent.StaminaPool.Regen();

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

                // Wait for the damage popup animation (ResolutionAnimator calls NotifyActionAnimDone).
                // 2-second timeout so a missing animator can't soft-lock the battle.
                _actionAnimDone = false;
                float animRemaining = 2f;
                while (!_actionAnimDone && animRemaining > 0f)
                {
                    animRemaining -= Time.deltaTime;
                    yield return null;
                }
            }
        }

        private IEnumerator Co_DrawPhase()
        {
            SetPhase(TurnPhase.DrawPhase);
            FillHandAI(_opponent);
            DrawCardsForPlayer(3);
            yield return null;
        }

        private void DrawCardsForPlayer(int count)
        {
            // Auto-discard oldest cards to make room if needed.
            while (_player.Hand.Cards.Count > Hand.MaxSize - count)
            {
                var oldest = _player.Hand.Cards[0];
                _player.Hand.RemoveCard(oldest);
            }
            _player.Hand.DrawN(_player.CardPool, count);
        }

        // ── AI helpers ────────────────────────────────────────────────────────

        private static void FillHandAI(BattleTeam team)
        {
            const int drawCount = 3;
            while (team.Hand.Cards.Count > Hand.MaxSize - drawCount)
            {
                var discard = team.Hand.Cards[0];
                team.Hand.RemoveCard(discard);
            }
            team.Hand.DrawN(team.CardPool, drawCount);
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
