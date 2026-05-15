using System;
using Capybrawlers.Creatures;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Capybrawlers.Collection
{
    public class BuildCardView : MonoBehaviour
    {
        [SerializeField] private Image    _natureIcon;
        [SerializeField] private TextMeshProUGUI _statsText;
        [SerializeField] private Image[]  _equipmentIcons; // length 3: Helm, Armor, Weapon
        [SerializeField] private Button   _selectButton;

        public void Bind(ResolvedBuild build, Action onSelect)
        {
            _statsText.text = $"HP {build.FinalHP}  ATK {build.FinalATK}  DEF {build.FinalDEF}  SPD {build.FinalSPD}";

            if (build.Nature.idleSprite != null)
                _natureIcon.sprite = build.Nature.idleSprite;

            var equipment = new[] { build.Helm, build.Armor, build.Weapon };
            for (int i = 0; i < _equipmentIcons.Length && i < equipment.Length; i++)
            {
                if (equipment[i]?.icon != null)
                    _equipmentIcons[i].sprite = equipment[i].icon;
            }

            _selectButton.onClick.RemoveAllListeners();
            _selectButton.onClick.AddListener(() => onSelect());
        }
    }
}
