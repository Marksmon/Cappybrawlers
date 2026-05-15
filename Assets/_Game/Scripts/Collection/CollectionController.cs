using System.Collections.Generic;
using Capybrawlers.Core;
using Capybrawlers.Creatures;
using UnityEngine;

namespace Capybrawlers.Collection
{
    public class CollectionController : MonoBehaviour
    {
        [SerializeField] private NatureLibrary    _natures;
        [SerializeField] private EquipmentLibrary _equipment;
        [SerializeField] private BuildCardView    _cardViewPrefab;
        [SerializeField] private Transform        _gridRoot;
        [SerializeField] private FormationController _formation;

        private readonly List<BuildCardView> _spawnedCards = new();

        private void Start()
        {
            var profile = ServiceLocator.Get<ISaveProvider>().Load();
            Populate(profile);
            _formation.Initialize(profile, _natures, _equipment, OnFormationSaved);
        }

        private void Populate(PlayerProfile profile)
        {
            foreach (var card in _spawnedCards)
                Destroy(card.gameObject);
            _spawnedCards.Clear();

            foreach (var record in profile.ownedBuildRecords)
            {
                var resolved = BuildRegistry.Resolve(record, _natures, _equipment);
                var view     = Instantiate(_cardViewPrefab, _gridRoot);
                view.Bind(resolved, () => _formation.TryAssign(record.buildId));
                _spawnedCards.Add(view);
            }
        }

        private void OnFormationSaved(PlayerProfile updated)
        {
            ServiceLocator.Get<ISaveProvider>().Save(updated);
        }
    }
}
