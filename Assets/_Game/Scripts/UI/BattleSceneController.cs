using Capybrawlers.Battle;
using Capybrawlers.Core;
using Capybrawlers.Creatures;
using UnityEngine;

namespace Capybrawlers.UI
{
    public class BattleSceneController : MonoBehaviour
    {
        [SerializeField] private NatureLibrary       _natures;
        [SerializeField] private EquipmentLibrary    _equipment;
        [SerializeField] private BattleManager       _battleManager;
        [SerializeField] private BattleUIController  _ui;
        [SerializeField] private BattleIntroController _introController;

        private void Start()
        {
            var profile = ServiceLocator.Get<ISaveProvider>().Load();
            var playerBuilds = ResolvePlayerTeam(profile);
            if (playerBuilds == null)
            {
                Debug.LogError("No active team set — cannot start battle.");
                return;
            }

            var opponentBuilds = BattleArenaTeamGenerator.Generate(_natures, _equipment);

            if (_introController != null)
            {
                _introController.Show(playerBuilds, opponentBuilds,
                    profile?.displayName ?? "You",
                    () =>
                    {
                        _battleManager.StartBattle(playerBuilds, opponentBuilds);
                        _ui.Initialize(_battleManager);
                    });
            }
            else
            {
                _battleManager.StartBattle(playerBuilds, opponentBuilds);
                _ui.Initialize(_battleManager);
            }
        }

        private ResolvedBuild[] ResolvePlayerTeam(PlayerProfile profile)
        {
            if (profile?.activeTeamBuildIds == null) return null;
            var builds = new ResolvedBuild[3];
            for (int i = 0; i < 3; i++)
            {
                var id = profile.activeTeamBuildIds[i];
                if (string.IsNullOrEmpty(id)) return null;
                var record = profile.ownedBuildRecords.Find(r => r.buildId == id);
                if (record == null) return null;
                builds[i] = BuildRegistry.Resolve(record, _natures, _equipment);
            }
            return builds;
        }
    }
}
