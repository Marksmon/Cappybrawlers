using System;
using System.Collections;
using Capybrawlers.Creatures;
using TMPro;
using UnityEngine;

namespace Capybrawlers.UI
{
    public class BattleIntroController : MonoBehaviour
    {
        [SerializeField] private BrawlerIntroSlice[] _playerSlices;     // 3
        [SerializeField] private BrawlerIntroSlice[] _opponentSlices;   // 3
        [SerializeField] private TextMeshProUGUI     _playerNameText;
        [SerializeField] private TextMeshProUGUI     _opponentNameText;
        [SerializeField] private TextMeshProUGUI     _countdownText;
        [SerializeField] private float               _displaySecs = 4f;

        private Action _onComplete;

        public void Show(ResolvedBuild[] player, ResolvedBuild[] opponent,
                         string playerName, Action onComplete)
        {
            _onComplete = onComplete;
            gameObject.SetActive(true);

            for (int i = 0; i < 3; i++)
            {
                if (i < player.Length)   _playerSlices[i].Bind(player[i]);
                if (i < opponent.Length) _opponentSlices[i].Bind(opponent[i]);
            }

            _playerNameText.text   = playerName;
            _opponentNameText.text = "Opponent";
            StartCoroutine(Co_Display());
        }

        private IEnumerator Co_Display()
        {
            float elapsed = 0f;
            while (elapsed < _displaySecs)
            {
                elapsed += Time.deltaTime;
                if (_countdownText)
                    _countdownText.text = Mathf.CeilToInt(_displaySecs - elapsed).ToString();
                yield return null;
            }
            gameObject.SetActive(false);
            _onComplete?.Invoke();
        }
    }
}
