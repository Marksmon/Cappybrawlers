using System;
using System.Linq;
using Capybrawlers.Core;
using Capybrawlers.Creatures;
using UnityEngine;

namespace Capybrawlers.Collection
{
    public class FormationController : MonoBehaviour
    {
        [SerializeField] private FormationSlotView[] _slots; // length 3, indexed by FormationSlot enum

        private PlayerProfile     _profile;
        private NatureLibrary     _natures;
        private EquipmentLibrary  _equipment;
        private Action<PlayerProfile> _onSave;

        public void Initialize(
            PlayerProfile profile,
            NatureLibrary natures,
            EquipmentLibrary equipment,
            Action<PlayerProfile> onSave)
        {
            _profile   = profile;
            _natures   = natures;
            _equipment = equipment;
            _onSave    = onSave;
            RefreshSlotViews();
        }

        // Called by CollectionController when the player taps a BuildCardView.
        public void TryAssign(string buildId)
        {
            // If already in formation, remove it.
            for (int i = 0; i < _profile.activeTeamBuildIds.Length; i++)
            {
                if (_profile.activeTeamBuildIds[i] == buildId)
                {
                    _profile.activeTeamBuildIds[i] = null;
                    RefreshSlotViews();
                    _onSave(_profile);
                    return;
                }
            }

            // Fill first empty slot.
            for (int i = 0; i < _profile.activeTeamBuildIds.Length; i++)
            {
                if (string.IsNullOrEmpty(_profile.activeTeamBuildIds[i]))
                {
                    _profile.activeTeamBuildIds[i] = buildId;
                    RefreshSlotViews();
                    _onSave(_profile);
                    return;
                }
            }
            // All slots full — no assignment, could surface a UI warning here.
        }

        public bool IsTeamComplete =>
            _profile.activeTeamBuildIds.All(id => !string.IsNullOrEmpty(id));

        private void RefreshSlotViews()
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                var id = _profile.activeTeamBuildIds[i];
                if (string.IsNullOrEmpty(id))
                {
                    _slots[i].ShowEmpty((FormationSlot)i);
                }
                else
                {
                    var record   = _profile.ownedBuildRecords.Find(r => r.buildId == id);
                    var resolved = BuildRegistry.Resolve(record, _natures, _equipment);
                    _slots[i].ShowBrawler((FormationSlot)i, resolved);
                }
            }
        }
    }
}
