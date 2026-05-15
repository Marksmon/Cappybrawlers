using Capybrawlers.Battle;
using Capybrawlers.Core;
using UnityEngine;

namespace Capybrawlers.UI
{
    // Listens for ActionResolvedEvent and triggers attack/hit animations
    // on the appropriate BrawlerViews.
    public class ResolutionAnimator : MonoBehaviour
    {
        [SerializeField] private BrawlerView[] _playerViews;   // length 3, index matches team.Brawlers
        [SerializeField] private BrawlerView[] _opponentViews; // length 3

        private BattleManager _manager;
        private IEventBus     _events;

        private void Start()
        {
            var mgr = FindFirstObjectByType<BattleManager>();
            if (mgr != null) Bind(mgr);
            else Debug.LogWarning("[ResolutionAnimator] BattleManager not found.");
        }

        public void Bind(BattleManager manager)
        {
            _manager = manager;
            _events  = ServiceLocator.Get<IEventBus>();
            _events.Subscribe<ActionResolvedEvent>(OnActionResolved);
        }

        private void OnDestroy()
        {
            _events?.Unsubscribe<ActionResolvedEvent>(OnActionResolved);
        }

        private void OnActionResolved(ActionResolvedEvent e)
        {
            var sourceView = FindView(e.Action.Source);
            var targetView = e.Action.Target != null ? FindView(e.Action.Target) : null;

            sourceView?.PlayAttack();
            if (e.DamageDealt > 0)
            {
                if (e.Action.Target != null && e.Action.Target.IsKnockedOut)
                    targetView?.PlayDeath();
                else
                    targetView?.PlayHit();
            }
        }

        private BrawlerView FindView(CapybrawlerInstance brawler)
        {
            for (int i = 0; i < _manager.PlayerTeam.Brawlers.Length; i++)
                if (_manager.PlayerTeam.Brawlers[i] == brawler && i < _playerViews.Length)
                    return _playerViews[i];

            for (int i = 0; i < _manager.OpponentTeam.Brawlers.Length; i++)
                if (_manager.OpponentTeam.Brawlers[i] == brawler && i < _opponentViews.Length)
                    return _opponentViews[i];

            return null;
        }
    }
}
