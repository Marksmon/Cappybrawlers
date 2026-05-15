using System;

namespace Capybrawlers.Core
{
    public interface IBPManager
    {
        int CurrentBP { get; }
        RankTier CurrentRank { get; }
        void ApplyResult(BattleResult result);
    }

    public class BPManager : IBPManager
    {
        private const int WinBP  =  25;
        private const int LossBP = -15;

        private readonly ISaveProvider _save;
        private readonly IEventBus _events;
        private PlayerProfile _profile;

        public int CurrentBP => _profile.bpPoints;
        public RankTier CurrentRank => _profile.currentRank;

        public BPManager(ISaveProvider save, IEventBus events)
        {
            _save    = save;
            _events  = events;
            _profile = save.Load() ?? new PlayerProfile();
        }

        public void ApplyResult(BattleResult result)
        {
            var oldRank = _profile.currentRank;
            var delta   = result == BattleResult.Win ? WinBP : LossBP;

            _profile.bpPoints   = Math.Max(0, _profile.bpPoints + delta);
            _profile.currentRank = ComputeRank(_profile.bpPoints);
            _save.Save(_profile);

            _events.Publish(new BPChangedEvent(_profile.bpPoints, delta));
            if (_profile.currentRank != oldRank)
                _events.Publish(new RankChangedEvent(oldRank, _profile.currentRank));
        }

        public static RankTier ComputeRank(int bp) => bp switch
        {
            >= 4000 => RankTier.GrandCapy,
            >= 3000 => RankTier.Tempest,
            >= 2000 => RankTier.StoneFang,
            >= 1000 => RankTier.Brawler,
            >= 500  => RankTier.Sprout,
            _       => RankTier.Pup,
        };
    }
}
