using Capybrawlers.Battle;
using Capybrawlers.Core;
using TMPro;
using UnityEngine;

namespace Capybrawlers.UI
{
    public class StaminaView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _staminaText;

        private StaminaPool _pool;

        public void Bind(StaminaPool pool, IEventBus events)
        {
            _pool = pool;
            events.Subscribe<StaminaChangedEvent>(_ => Refresh());
            Refresh();
        }

        public void Refresh()
        {
            if (_staminaText && _pool != null)
                _staminaText.text = $"{_pool.Current}";
        }
    }
}
