using System.Collections;
using System.Collections.Generic;
using Capybrawlers.Core;
using UnityEngine;

namespace Capybrawlers.Battle
{
    public class TurnStateMachine
    {
        public TurnPhase CurrentPhase { get; private set; } = TurnPhase.Idle;

        private readonly BattleTeam _player;
        private readonly BattleTeam _opponent;
        private readonly IEventBus  _events;
        private readonly MonoBehaviour _runner; // runs coroutines

        public TurnStateMachine(BattleTeam player, BattleTeam opponent, IEventBus events, MonoBehaviour runner)
        {
            _player   = player;
            _opponent = opponent;
            _events   = events;
            _runner   = runner;
        }

        public void StartBattle() => _runner.StartCoroutine(Co_RunBattle());

        private IEnumerator Co_RunBattle()
        {
            SetPhase(TurnPhase.Setup);
            yield return null;

            while (!_player.IsDefeated && !_opponent.IsDefeated)
            {
                yield return Co_StartOfTurn();
                yield return Co_DrawPhase();
                yield return Co_QueuePhase();
                yield return Co_Resolution();
                yield return Co_Cleanup();

                if (_player.IsDefeated || _opponent.IsDefeated) break;
            }

            SetPhase(TurnPhase.BattleEnd);
            _events.Publish(new BattleEndEvent(!_player.IsDefeated));
        }

        private IEnumerator Co_StartOfTurn()
        {
            SetPhase(TurnPhase.StartOfTurn);
            _player.CardPool.TickCooldowns();
            _opponent.CardPool.TickCooldowns();
            _player.StaminaPool.Regen();
            _opponent.StaminaPool.Regen();
            foreach (var b in _player.Brawlers)   b.TickEffects();
            foreach (var b in _opponent.Brawlers) b.TickEffects();
            yield return null;
        }

        private IEnumerator Co_DrawPhase()
        {
            SetPhase(TurnPhase.DrawPhase);
            _player.Hand.Draw(_player.CardPool);
            _opponent.Hand.Draw(_opponent.CardPool);
            yield return null;
        }

        private IEnumerator Co_QueuePhase()
        {
            SetPhase(TurnPhase.QueuePhase);
            // Player input is driven externally (UI confirms via Advance()).
            // Coroutine waits until _queueConfirmed is set true.
            _queueConfirmed = false;
            while (!_queueConfirmed)
                yield return null;
        }

        private IEnumerator Co_Resolution()
        {
            SetPhase(TurnPhase.Resolution);
            var ordered = SPDResolver.Resolve(_player, _opponent);

            while (ordered.Count > 0)
            {
                var action = ordered[0];
                ordered.RemoveAt(0);

                // Skip stunned brawlers — remove their lowest-priority remaining action.
                if (action.Source.HasEffect(MechanicType.FullStun))
                    continue;
                if (action.Source.HasEffect(MechanicType.Stun) && IsLowestPriority(action, ordered))
                    continue;

                if (action.Source.IsKnockedOut && !action.Source.IsInTerminalRally)
                    continue;

                EffectProcessor.Process(action, _player, _opponent, _events);

                TerminalRallyHandler.CheckAndInsertRallyActions(
                    action.Source, ordered, _player, _opponent, _events);

                yield return null; // one frame per action so UI can animate
            }
        }

        private IEnumerator Co_Cleanup()
        {
            SetPhase(TurnPhase.Cleanup);
            _player.Hand.ReturnUnplayed(_player.CardPool);
            _opponent.Hand.ReturnUnplayed(_opponent.CardPool);
            foreach (var b in _player.Brawlers)   b.QueuedActions.Clear();
            foreach (var b in _opponent.Brawlers) b.QueuedActions.Clear();
            yield return null;
        }

        // Called by the UI confirm button after the player has assigned all cards.
        private bool _queueConfirmed;
        public void ConfirmQueue() => _queueConfirmed = true;

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
