using Capybrawlers.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace Capybrawlers.UI
{
    public class QueueConfirmButton : MonoBehaviour
    {
        [SerializeField] private Button _endTurnButton;

        private BattleManager _manager;
        private HandView      _handView;

        public void Bind(BattleManager manager, HandView handView)
        {
            _manager  = manager;
            _handView = handView;
            _endTurnButton.onClick.RemoveAllListeners();
            _endTurnButton.onClick.AddListener(OnEndTurn);
            SetInteractable(false);
        }

        public void SetPhase(TurnPhase phase) =>
            SetInteractable(phase == TurnPhase.DecisionPhase);

        // Queues any staged cards (no-op if none), then advances the turn.
        private void OnEndTurn()
        {
            _handView.QueueStagedCards();
            _manager.ConfirmPlayerQueue();
        }

        private void SetInteractable(bool value) =>
            _endTurnButton.interactable = value;
    }
}
