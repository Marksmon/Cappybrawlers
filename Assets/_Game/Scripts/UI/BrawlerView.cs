using System;
using Capybrawlers.Battle;
using Capybrawlers.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Capybrawlers.UI
{
    public class BrawlerView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Slider          _hpBar;
        [SerializeField] private TextMeshProUGUI _hpText;
        [SerializeField] private TextMeshProUGUI _natureText;
        [SerializeField] private Image           _targetHighlight;
        [SerializeField] private GameObject      _knockedOutOverlay;
        [SerializeField] private Animator        _animator;

        [Header("Effect Badges")]
        [SerializeField] private Image[]           _badgeBackgrounds;
        [SerializeField] private TextMeshProUGUI[] _badgeLabels;

        private static readonly Color BuffColor  = new(0.20f, 0.72f, 0.30f, 0.92f);
        private static readonly Color DebuffColor = new(0.85f, 0.22f, 0.18f, 0.92f);

        private CapybrawlerInstance _brawler;

        public Action<CapybrawlerInstance> OnClicked;

        public void Bind(CapybrawlerInstance brawler)
        {
            _brawler = brawler;
            if (_natureText)
                _natureText.text = brawler.Build.Nature.natureType.ToString();
            Refresh();
        }

        public void Refresh()
        {
            if (_brawler == null) return;

            float ratio = (float)_brawler.CurrentHP / _brawler.MaxHP;
            if (_hpBar)  _hpBar.value = ratio;
            if (_hpText) _hpText.text = $"{_brawler.CurrentHP}/{_brawler.MaxHP}";

            if (_knockedOutOverlay)
                _knockedOutOverlay.SetActive(_brawler.IsKnockedOut);

            RefreshBadges();
        }

        public void SetTargeted(bool targeted)
        {
            if (_targetHighlight)
                _targetHighlight.enabled = targeted;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_brawler != null) OnClicked?.Invoke(_brawler);
        }

        public void PlayAttack() { if (_animator != null) _animator.SetTrigger("Attack"); }
        public void PlayHit()    { if (_animator != null) _animator.SetTrigger("Hit"); }
        public void PlayDeath()  { if (_animator != null) _animator.SetTrigger("Death"); }

        private void RefreshBadges()
        {
            if (_badgeBackgrounds == null || _badgeLabels == null) return;
            var effects = _brawler.ActiveEffects;
            for (int i = 0; i < _badgeBackgrounds.Length; i++)
            {
                bool active = i < effects.Count;
                _badgeBackgrounds[i].enabled = active;
                _badgeLabels[i].enabled      = active;
                if (!active) continue;
                _badgeBackgrounds[i].color = ActiveEffect.IsNegative(effects[i].Type) ? DebuffColor : BuffColor;
                _badgeLabels[i].text       = GetBadgeAbbrev(effects[i].Type);
            }
        }

        private static string GetBadgeAbbrev(MechanicType type) => type switch
        {
            MechanicType.Burn        => "BRN",
            MechanicType.Poison      => "PSN",
            MechanicType.Slow        => "SLW",
            MechanicType.Stun        => "STN",
            MechanicType.FullStun    => "FST",
            MechanicType.Rage        => "RGE",
            MechanicType.Berserker   => "BSK",
            MechanicType.Haste       => "HST",
            MechanicType.Shield      => "SHD",
            MechanicType.Reflect     => "RFL",
            _                        => type.ToString()[..3].ToUpper()
        };
    }
}
