using Capybrawlers.Core;
using Capybrawlers.Creatures;
using UnityEngine;

namespace Capybrawlers.Battle
{
    // Created per-battle by BattleSceneController; destroyed when BattleEnd fires.
    public class BattleManager : MonoBehaviour
    {
        [SerializeField] private NatureLibrary    _natures;
        [SerializeField] private EquipmentLibrary _equipment;

        public BattleTeam        PlayerTeam   { get; private set; }
        public BattleTeam        OpponentTeam { get; private set; }
        public TurnStateMachine  TurnMachine  { get; private set; }

        private IEventBus _events;

        public void StartBattle(ResolvedBuild[] playerBuilds, ResolvedBuild[] opponentBuilds)
        {
            _events      = ServiceLocator.Get<IEventBus>();
            PlayerTeam   = new BattleTeam(playerBuilds,   _events, isPlayerTeam: true);
            OpponentTeam = new BattleTeam(opponentBuilds, _events, isPlayerTeam: false);
            TurnMachine  = new TurnStateMachine(PlayerTeam, OpponentTeam, _events, this);

            _events.Subscribe<TurnPhaseChangedEvent>(OnPhaseChanged);
            _events.Subscribe<BattleEndEvent>(OnBattleEnd);

            TurnMachine.StartBattle();
        }

        public void ConfirmPlayerQueue() => TurnMachine.ConfirmQueue();

        public void ForceDiscardCard(CardPoolEntry entry)
        {
            PlayerTeam.Hand.RemoveCard(entry);
            PlayerTeam.CardPool.StartCooldown(entry);
            TurnMachine.NotifyDiscardComplete();
        }

        private void OnPhaseChanged(TurnPhaseChangedEvent e)
        {
            if (e.Phase == TurnPhase.DecisionPhase)
                SimpleAIController.QueueActions(OpponentTeam, PlayerTeam);
        }

        private void OnBattleEnd(BattleEndEvent e)
        {
            var bp = ServiceLocator.Get<IBPManager>();
            bp.ApplyResult(e.PlayerWon ? BattleResult.Win : BattleResult.Loss);

            _events.Unsubscribe<TurnPhaseChangedEvent>(OnPhaseChanged);
            _events.Unsubscribe<BattleEndEvent>(OnBattleEnd);
        }
    }
}
