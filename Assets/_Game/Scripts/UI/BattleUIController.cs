using Capybrawlers.Battle;
using Capybrawlers.Core;
using TMPro;
using UnityEngine;

namespace Capybrawlers.UI
{
    public class BattleUIController : MonoBehaviour
    {
        [Header("Team Views")]
        [SerializeField] private BrawlerView[] _playerBrawlerViews;
        [SerializeField] private BrawlerView[] _opponentBrawlerViews;

        [Header("Hand & Stamina")]
        [SerializeField] private HandView    _handView;
        [SerializeField] private StaminaView _staminaView;

        [Header("Controls")]
        [SerializeField] private QueueConfirmButton  _confirmButton;
        [SerializeField] private BattleEndController _endScreen;

        [Header("HUD")]
        [SerializeField] private TextMeshProUGUI _turnText;
        [SerializeField] private TextMeshProUGUI _timerText;

        [Header("Info Panel")]
        [SerializeField] private BrawlerInfoPanel _brawlerInfoPanel;

        private BattleManager _manager;
        private IEventBus     _events;

        public CapybrawlerInstance CurrentTarget { get; private set; }

        public void Initialize(BattleManager manager)
        {
            _manager = manager;
            _events  = ServiceLocator.Get<IEventBus>();

            BindBrawlerViews(_playerBrawlerViews,   manager.PlayerTeam);
            BindBrawlerViews(_opponentBrawlerViews, manager.OpponentTeam);

            foreach (var v in _playerBrawlerViews)
                v.OnClicked += ShowBrawlerInfo;

            foreach (var v in _opponentBrawlerViews)
                v.OnClicked += b => { SelectTarget(b); ShowBrawlerInfo(b); };

            _staminaView.Bind(manager.PlayerTeam.StaminaPool, _events);
            _handView.Bind(manager.PlayerTeam, this);
            _confirmButton.Bind(manager, _handView);
            _endScreen.gameObject.SetActive(false);

            RefreshDefaultTarget();

            _events.Subscribe<TurnPhaseChangedEvent>(OnPhaseChanged);
            _events.Subscribe<BattleEndEvent>(OnBattleEnd);
            _events.Subscribe<ActionResolvedEvent>(OnActionResolved);
            _events.Subscribe<TurnChangedEvent>(OnTurnChanged);
            _events.Subscribe<DecisionTimerEvent>(OnDecisionTimer);
            _events.Subscribe<DecisionTimerExpiredEvent>(OnDecisionTimerExpired);
            _events.Subscribe<DiscardRequiredEvent>(OnDiscardRequired);
            _events.Subscribe<DrawPhaseTimerEvent>(OnDrawPhaseTimer);
            _events.Subscribe<DrawPhaseTimerExpiredEvent>(OnDrawPhaseTimerExpired);
            _events.Subscribe<BrawlerKnockedOutEvent>(OnBrawlerKnockedOut);
        }

        private void OnDestroy()
        {
            if (_events == null) return;
            _events.Unsubscribe<TurnPhaseChangedEvent>(OnPhaseChanged);
            _events.Unsubscribe<BattleEndEvent>(OnBattleEnd);
            _events.Unsubscribe<ActionResolvedEvent>(OnActionResolved);
            _events.Unsubscribe<TurnChangedEvent>(OnTurnChanged);
            _events.Unsubscribe<DecisionTimerEvent>(OnDecisionTimer);
            _events.Unsubscribe<DecisionTimerExpiredEvent>(OnDecisionTimerExpired);
            _events.Unsubscribe<DiscardRequiredEvent>(OnDiscardRequired);
            _events.Unsubscribe<DrawPhaseTimerEvent>(OnDrawPhaseTimer);
            _events.Unsubscribe<DrawPhaseTimerExpiredEvent>(OnDrawPhaseTimerExpired);
            _events.Unsubscribe<BrawlerKnockedOutEvent>(OnBrawlerKnockedOut);
        }

        // Only the first alive opponent (frontline) is a valid target.
        public void SelectTarget(CapybrawlerInstance brawler)
        {
            if (brawler != GetFrontTarget(_manager.OpponentTeam)) return;
            CurrentTarget = brawler;
            RefreshOpponentHighlights();
        }

        // ── Phase & action handlers ───────────────────────────────────────────

        private void OnPhaseChanged(TurnPhaseChangedEvent e)
        {
            bool isDecision = e.Phase == TurnPhase.DecisionPhase;
            _handView.SetInteractable(isDecision);
            _confirmButton.SetPhase(e.Phase);
            if (!isDecision && _timerText) _timerText.text = "";
            RefreshAllViews();
        }

        private void OnActionResolved(ActionResolvedEvent e) => RefreshAllViews();

        private void OnBattleEnd(BattleEndEvent e)
        {
            _endScreen.gameObject.SetActive(true);
            var bp = ServiceLocator.Get<IBPManager>();
            _endScreen.Show(e.PlayerWon, bp.CurrentBP, bp.CurrentRank);
        }

        private void OnBrawlerKnockedOut(BrawlerKnockedOutEvent _)
        {
            var valid = GetFrontTarget(_manager.OpponentTeam);
            if (valid != null) { CurrentTarget = valid; RefreshOpponentHighlights(); }
        }

        // ── HUD handlers ─────────────────────────────────────────────────────

        private void OnTurnChanged(TurnChangedEvent e)
        {
            if (_turnText) _turnText.text = $"Turn {e.Turn}";
        }

        private void OnDecisionTimer(DecisionTimerEvent e)
        {
            if (_timerText) _timerText.text = Mathf.CeilToInt(e.TimeLeft).ToString();
        }

        private void OnDecisionTimerExpired(DecisionTimerExpiredEvent _)
        {
            _handView.QueueStagedCards();
            if (_timerText) _timerText.text = "";
        }

        private void OnDiscardRequired(DiscardRequiredEvent _)
        {
            _handView.EnterDiscardMode(entry => _manager.ForceDiscardCard(entry));
        }

        private void OnDrawPhaseTimer(DrawPhaseTimerEvent e)
        {
            if (_timerText) _timerText.text = Mathf.CeilToInt(e.TimeLeft).ToString();
        }

        private void OnDrawPhaseTimerExpired(DrawPhaseTimerExpiredEvent _)
        {
            _handView.ExitDiscardMode();
            if (_timerText) _timerText.text = "";
        }

        // ── Brawler info panel ────────────────────────────────────────────────

        private void ShowBrawlerInfo(CapybrawlerInstance brawler)
        {
            if (_brawlerInfoPanel) _brawlerInfoPanel.Show(brawler);
        }

        // ── View refresh ──────────────────────────────────────────────────────

        private void RefreshAllViews()
        {
            RefreshBrawlerViews(_playerBrawlerViews,   _manager.PlayerTeam);
            RefreshBrawlerViews(_opponentBrawlerViews, _manager.OpponentTeam);
            _staminaView.Refresh();
            _handView.Refresh();
        }

        private static void BindBrawlerViews(BrawlerView[] views, BattleTeam team)
        {
            for (int i = 0; i < views.Length && i < team.Brawlers.Length; i++)
                views[i].Bind(team.Brawlers[i]);
        }

        private static void RefreshBrawlerViews(BrawlerView[] views, BattleTeam team)
        {
            for (int i = 0; i < views.Length && i < team.Brawlers.Length; i++)
                views[i].Refresh();
        }

        private void RefreshDefaultTarget()
        {
            var valid = GetFrontTarget(_manager.OpponentTeam);
            if (valid != null) { CurrentTarget = valid; RefreshOpponentHighlights(); }
        }

        private void RefreshOpponentHighlights()
        {
            for (int i = 0; i < _opponentBrawlerViews.Length; i++)
                _opponentBrawlerViews[i].SetTargeted(
                    _manager.OpponentTeam.Brawlers[i] == CurrentTarget);
        }

        private static CapybrawlerInstance GetFrontTarget(BattleTeam team)
        {
            foreach (var b in team.Brawlers)
                if (!b.IsKnockedOut) return b;
            return null;
        }
    }
}
