using Capybrawlers.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Capybrawlers.UI
{
    public class BattleEndController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _resultText;
        [SerializeField] private TextMeshProUGUI _bpText;
        [SerializeField] private TextMeshProUGUI _rankText;
        [SerializeField] private Button          _returnButton;

        private void Start()
        {
            _returnButton.onClick.AddListener(ReturnToMenu);
        }

        public void Show(bool playerWon, int newBP, RankTier newRank)
        {
            _resultText.text = playerWon ? "Victory!" : "Defeat";
            _bpText.text     = $"{newBP} BP  ({(playerWon ? "+25" : "-15")})";
            _rankText.text   = RankBadgeView.RankDisplayName(newRank);
        }

        private void ReturnToMenu()
        {
            ServiceLocator.Get<ISceneLoader>().Load(SceneId.MainMenu);
        }
    }
}
