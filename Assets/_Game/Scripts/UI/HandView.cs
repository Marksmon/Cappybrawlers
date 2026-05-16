using System;
using System.Collections.Generic;
using Capybrawlers.Battle;
using UnityEngine;

namespace Capybrawlers.UI
{
    public class HandView : MonoBehaviour
    {
        [SerializeField] private CardView[]          _cardViews;       // unchosen, bottom row (18 slots)
        [SerializeField] private CardView[]          _stagedCardViews; // chosen, staging strip (10 slots)
        [SerializeField] private TMPro.TextMeshProUGUI _deckCountText;  // deck count label beside hand

        private BattleTeam         _team;
        private BattleUIController _ui;

        // Tracks which hand slot each staged card came from.
        private readonly int[]           _stagedFromSlot = new int[10];
        private readonly CardPoolEntry[] _stagedEntries  = new CardPoolEntry[10];
        private int _stagedCount;

        private bool                  _discardMode;
        private Action<CardPoolEntry> _onDiscardCallback;

        public void Bind(BattleTeam team, BattleUIController ui)
        {
            _team = team;
            _ui   = ui;
            Refresh();
        }

        // Same as Refresh but animates cards that weren't visible before (newly drawn).
        public void RefreshWithAnimation()
        {
            bool[] prev = new bool[_cardViews.Length];
            for (int i = 0; i < _cardViews.Length; i++)
                prev[i] = _cardViews[i].gameObject.activeSelf;

            Refresh();

            int animIndex = 0;
            for (int i = 0; i < _cardViews.Length; i++)
            {
                if (_cardViews[i].gameObject.activeSelf && !prev[i])
                {
                    _cardViews[i].AnimateDealIn(animIndex * 0.18f);
                    animIndex++;
                }
            }
        }

        // Redraws hand without touching staging state.
        // Staged slots remain hidden; only un-staged hand cards are shown.
        public void Refresh()
        {
            var stagedSlots = new HashSet<int>();
            for (int i = 0; i < _stagedCount; i++)
                stagedSlots.Add(_stagedFromSlot[i]);

            var hand = _team.Hand.Cards;
            for (int i = 0; i < _cardViews.Length; i++)
            {
                if (i < hand.Count && !stagedSlots.Contains(i))
                {
                    _cardViews[i].gameObject.SetActive(true);
                    int slot = i;
                    _cardViews[i].Bind(hand[i], _ => StageCard(slot), "Select");
                    _cardViews[i].SetDragMode(true);
                }
                else
                {
                    _cardViews[i].gameObject.SetActive(false);
                }
            }

            if (_deckCountText != null && _team != null)
                _deckCountText.text = $"{_team.CardPool.DeckCount}";
        }

        public void SetInteractable(bool interactable)
        {
            foreach (var cv in _cardViews)       cv.SetInteractable(interactable);
            foreach (var cv in _stagedCardViews) cv.SetInteractable(interactable);
        }

        // Called by Lock In button or timer expiry — queues staged cards then clears staging.
        // Cards are round-robined across alive brawlers (brawler 0, 1, 2, 0, 1, 2, ...).
        // Stamina was already spent when each card was staged — do not spend again.
        public void QueueStagedCards()
        {
            var alive = new System.Collections.Generic.List<CapybrawlerInstance>();
            foreach (var b in _team.Brawlers)
                if (!b.IsKnockedOut) alive.Add(b);

            if (alive.Count == 0)
            {
                for (int i = 0; i < _stagedCount; i++)
                    _team.StaminaPool.Add(_stagedEntries[i].Card.staminaCost);
                ClearStaging();
                return;
            }

            int cycle = 0;
            for (int i = 0; i < _stagedCount; i++)
            {
                var entry = _stagedEntries[i];
                var actor = alive[cycle % alive.Count];
                actor.QueuedActions.Add(new QueuedAction
                {
                    Source = actor,
                    Card   = entry.Card,
                    Target = _ui.CurrentTarget,
                });
                _team.CardPool.StartCooldown(entry);
                _team.Hand.RemoveCard(entry);
                cycle++;
            }
            ClearStaging();
        }

        // Called by End Turn button or after QueueStagedCards — hides staging strip.
        public void ClearStaging()
        {
            for (int i = 0; i < _stagedCardViews.Length; i++)
                _stagedCardViews[i].gameObject.SetActive(false);

            // Restore any hand slots that were hidden because they were staged.
            for (int i = 0; i < _stagedCount; i++)
            {
                int slot = _stagedFromSlot[i];
                if (slot < _team.Hand.Cards.Count)
                    _cardViews[slot].gameObject.SetActive(true);
            }
            _stagedCount = 0;
        }

        // Called when Drawing Phase requires the player to discard one card.
        public void EnterDiscardMode(Action<CardPoolEntry> onDiscard)
        {
            _discardMode       = true;
            _onDiscardCallback = onDiscard;

            var stagedSlots = new HashSet<int>();
            for (int i = 0; i < _stagedCount; i++)
                stagedSlots.Add(_stagedFromSlot[i]);

            var hand = _team.Hand.Cards;
            for (int i = 0; i < _cardViews.Length; i++)
            {
                if (i < hand.Count && !stagedSlots.Contains(i) && _cardViews[i].gameObject.activeSelf)
                {
                    int slot = i;
                    _cardViews[i].Bind(hand[i], _ => OnDiscardSelected(slot), "Discard");
                }
            }
        }

        public void ExitDiscardMode()
        {
            _discardMode       = false;
            _onDiscardCallback = null;
            Refresh();
        }

        // ── Private staging logic ─────────────────────────────────────────────

        private void StageCard(int handSlot)
        {
            if (_discardMode) return;
            if (_stagedCount >= _stagedCardViews.Length) return;

            var entry = _team.Hand.Cards[handSlot];
            if (!_team.StaminaPool.TrySpend(entry.Card.staminaCost)) return;

            int stagedSlot = _stagedCount;
            _stagedEntries[stagedSlot]  = entry;
            _stagedFromSlot[stagedSlot] = handSlot;
            _stagedCount++;

            _cardViews[handSlot].gameObject.SetActive(false);
            RenderStagedViews();
        }

        private void UnstageCard(int stagedSlot)
        {
            _team.StaminaPool.Add(_stagedEntries[stagedSlot].Card.staminaCost);
            _cardViews[_stagedFromSlot[stagedSlot]].gameObject.SetActive(true);

            for (int i = stagedSlot; i < _stagedCount - 1; i++)
            {
                _stagedEntries[i]  = _stagedEntries[i + 1];
                _stagedFromSlot[i] = _stagedFromSlot[i + 1];
            }
            _stagedCount--;
            RenderStagedViews();
        }

        private void RenderStagedViews()
        {
            for (int i = 0; i < _stagedCardViews.Length; i++)
            {
                if (i < _stagedCount)
                {
                    _stagedCardViews[i].gameObject.SetActive(true);
                    int captured = i;
                    _stagedCardViews[i].Bind(_stagedEntries[i], _ => UnstageCard(captured), "");
                    _stagedCardViews[i].SetClickMode(true);
                }
                else
                {
                    _stagedCardViews[i].gameObject.SetActive(false);
                }
            }
        }

        private void OnDiscardSelected(int handSlot)
        {
            var entry = _team.Hand.Cards[handSlot];
            var cb    = _onDiscardCallback;
            ExitDiscardMode();
            cb?.Invoke(entry);
        }
    }
}
