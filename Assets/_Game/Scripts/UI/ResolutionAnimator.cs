using System;
using System.Collections;
using Capybrawlers.Battle;
using Capybrawlers.Core;
using TMPro;
using UnityEngine;

namespace Capybrawlers.UI
{
    // Listens for ActionResolvedEvent, plays attack/hit animations on BrawlerViews,
    // spawns a floating damage number, then signals TurnStateMachine when done.
    public class ResolutionAnimator : MonoBehaviour
    {
        [SerializeField] private BrawlerView[]  _playerViews;
        [SerializeField] private BrawlerView[]  _opponentViews;
        [SerializeField] private RectTransform  _popupContainer;

        private BattleManager _manager;
        private IEventBus     _events;

        private void Start()
        {
            var mgr = FindAnyObjectByType<BattleManager>();
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

            StartCoroutine(Co_ShowPopupAndNotify(e.DamageDealt, targetView));
        }

        private IEnumerator Co_ShowPopupAndNotify(int damage, BrawlerView targetView)
        {
            if (_popupContainer != null && damage > 0 && targetView != null)
            {
                bool done = false;
                SpawnPopup(damage, targetView, () => done = true);
                while (!done) yield return null;
            }
            else
            {
                // Non-damage action (buff, heal, control) — short pause for readability.
                yield return new WaitForSeconds(0.28f);
            }

            _manager?.TurnMachine?.NotifyActionAnimDone();
        }

        private void SpawnPopup(int damage, BrawlerView targetView, Action onDone)
        {
            var go = new GameObject("DamagePopup");
            go.transform.SetParent(_popupContainer, false);

            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(220f, 80f);

            // Convert target view's screen position to popup container local space.
            Vector3 screenPos = targetView.GetComponent<RectTransform>().position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _popupContainer,
                new Vector2(screenPos.x, screenPos.y),
                null,
                out Vector2 localPos);
            rt.anchoredPosition = localPos + new Vector2(0f, 50f);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.fontSize  = 52f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;

            go.AddComponent<CanvasGroup>();
            go.AddComponent<DamagePopup>().Play(damage, onDone);
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
