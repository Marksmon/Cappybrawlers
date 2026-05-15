using Capybrawlers.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Capybrawlers.UI
{
    public class RankBadgeView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _rankNameText;
        [SerializeField] private Image           _badgeIcon;
        [SerializeField] private Sprite[]        _rankSprites; // index matches RankTier enum order

        public void SetRank(RankTier rank)
        {
            if (_rankNameText != null)
                _rankNameText.text = RankDisplayName(rank);

            var idx = (int)rank;
            if (_badgeIcon != null && _rankSprites != null && idx < _rankSprites.Length)
                _badgeIcon.sprite = _rankSprites[idx];
        }

        public static string RankDisplayName(RankTier rank) => rank switch
        {
            RankTier.Pup       => "Pup",
            RankTier.Sprout    => "Sprout",
            RankTier.Brawler   => "Brawler",
            RankTier.StoneFang => "Stone Fang",
            RankTier.Tempest   => "Tempest",
            RankTier.GrandCapy => "Grand Capy",
            _                  => rank.ToString(),
        };
    }
}
