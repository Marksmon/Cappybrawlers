using System.Collections.Generic;
using UnityEngine;

namespace Capybrawlers.Creatures
{
    [CreateAssetMenu(fileName = "EquipmentLibrary", menuName = "Capybrawlers/Libraries/Equipment Library")]
    public class EquipmentLibrary : ScriptableObject
    {
        [SerializeField] private EquipmentData[] _equipment;

        private Dictionary<string, EquipmentData> _lookup;

        public EquipmentData Get(string id)
        {
            BuildLookup();
            return _lookup.TryGetValue(id, out var data) ? data
                : throw new KeyNotFoundException($"EquipmentData not found for id '{id}'");
        }

        private void BuildLookup()
        {
            if (_lookup != null) return;
            _lookup = new Dictionary<string, EquipmentData>(_equipment.Length);
            foreach (var e in _equipment)
                _lookup[e.id] = e;
        }
    }
}
