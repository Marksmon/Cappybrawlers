using System.Collections.Generic;
using Capybrawlers.Core;
using UnityEngine;

namespace Capybrawlers.Creatures
{
    [CreateAssetMenu(fileName = "NatureLibrary", menuName = "Capybrawlers/Libraries/Nature Library")]
    public class NatureLibrary : ScriptableObject
    {
        [SerializeField] private NatureData[] _natures;

        private Dictionary<NatureType, NatureData> _lookup;

        public NatureData Get(NatureType type)
        {
            BuildLookup();
            return _lookup.TryGetValue(type, out var data) ? data
                : throw new KeyNotFoundException($"NatureData not found for {type}");
        }

        private void BuildLookup()
        {
            if (_lookup != null) return;
            _lookup = new Dictionary<NatureType, NatureData>(_natures.Length);
            foreach (var n in _natures)
                _lookup[n.natureType] = n;
        }
    }
}
