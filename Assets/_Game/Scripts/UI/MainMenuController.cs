using Capybrawlers.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Capybrawlers.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _displayNameText;
        [SerializeField] private TextMeshProUGUI _bpText;
        [SerializeField] private RankBadgeView   _rankBadge;
        [SerializeField] private Button          _collectionButton;
        [SerializeField] private Button          _battleButton;

        private IEventBus    _events;
        private IBPManager   _bp;
        private ISceneLoader _scenes;

        private void Start()
        {
            _events = ServiceLocator.Get<IEventBus>();
            _bp     = ServiceLocator.Get<IBPManager>();
            _scenes = ServiceLocator.Get<ISceneLoader>();

            _events.Subscribe<BPChangedEvent>(OnBPChanged);
            _events.Subscribe<RankChangedEvent>(OnRankChanged);

            if (_collectionButton != null)
                _collectionButton.onClick.AddListener(() => _scenes.Load(SceneId.Collection));
            if (_battleButton != null)
                _battleButton.onClick.AddListener(() => _scenes.Load(SceneId.Battle));

            var profile = ServiceLocator.Get<ISaveProvider>().Load();
            if (_displayNameText != null)
                _displayNameText.text = profile?.displayName ?? "Overlord";
            RefreshBP();
        }

        private void OnDestroy()
        {
            if (ServiceLocator.TryGet<IEventBus>(out var events))
            {
                events.Unsubscribe<BPChangedEvent>(OnBPChanged);
                events.Unsubscribe<RankChangedEvent>(OnRankChanged);
            }
        }

        private void OnBPChanged(BPChangedEvent e) => RefreshBP();
        private void OnRankChanged(RankChangedEvent e) => _rankBadge.SetRank(e.NewRank);

        private void RefreshBP()
        {
            _bpText.text = $"{_bp.CurrentBP} BP";
            _rankBadge.SetRank(_bp.CurrentRank);
        }
    }
}
