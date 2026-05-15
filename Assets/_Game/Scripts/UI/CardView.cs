using System;
using Capybrawlers.Battle;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Capybrawlers.UI
{
    public class CardView : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _costText;
        [SerializeField] private TextMeshProUGUI _descText;
        [SerializeField] private Button          _assignButton;
        [SerializeField] private Image           _assignedIndicator;

        private CardPoolEntry         _entry;
        private Action<CardPoolEntry> _onAssign;

        private bool  _dragMode;
        private bool  _clickMode;
        private bool  _interactable = true;
        private float _totalDragY;

        private const float DragThreshold = 60f;

        public void Bind(CardPoolEntry entry, Action<CardPoolEntry> onAssign,
            string buttonLabel = "Select")
        {
            _entry     = entry;
            _onAssign  = onAssign;
            _dragMode  = false;
            _clickMode = false;
            _totalDragY = 0f;
            _assignButton.gameObject.SetActive(true);

            _nameText.text = entry.Card.cardName;
            _costText.text = entry.Card.staminaCost.ToString();
            _descText.text = entry.Card.effectDescription;

            _assignButton.onClick.RemoveAllListeners();
            _assignButton.onClick.AddListener(() => _onAssign?.Invoke(_entry));

            var labelTmp = _assignButton.GetComponentInChildren<TextMeshProUGUI>();
            if (labelTmp != null) labelTmp.text = buttonLabel;

            SetAssigned(false);
        }

        // Drag upward past threshold to trigger callback (hand cards).
        public void SetDragMode(bool drag)
        {
            _dragMode = drag;
            _assignButton.gameObject.SetActive(!drag);
        }

        // Click anywhere on the card to trigger callback (staged cards).
        public void SetClickMode(bool click)
        {
            _clickMode = click;
            _assignButton.gameObject.SetActive(!click);
        }

        public void SetAssigned(bool assigned)
        {
            if (_assignedIndicator) _assignedIndicator.enabled = assigned;
            _assignButton.interactable = !assigned;
        }

        public void SetInteractable(bool interactable)
        {
            _interactable = interactable;
            _assignButton.interactable = interactable;
        }

        // ── Drag handlers ─────────────────────────────────────────────────────

        public void OnBeginDrag(PointerEventData e)
        {
            if (_dragMode && _interactable) _totalDragY = 0f;
        }

        public void OnDrag(PointerEventData e)
        {
            if (_dragMode && _interactable) _totalDragY += e.delta.y;
        }

        public void OnEndDrag(PointerEventData e)
        {
            if (!_dragMode || !_interactable) return;
            if (_totalDragY > DragThreshold) _onAssign?.Invoke(_entry);
            _totalDragY = 0f;
        }

        // ── Click handler (staged card removal) ───────────────────────────────

        public void OnPointerClick(PointerEventData e)
        {
            if (_clickMode && _interactable) _onAssign?.Invoke(_entry);
        }
    }
}
