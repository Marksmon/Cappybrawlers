namespace Capybrawlers.Core
{
    public readonly struct BPChangedEvent
    {
        public readonly int NewTotal;
        public readonly int Delta;
        public BPChangedEvent(int newTotal, int delta) { NewTotal = newTotal; Delta = delta; }
    }

    public readonly struct RankChangedEvent
    {
        public readonly RankTier OldRank;
        public readonly RankTier NewRank;
        public RankChangedEvent(RankTier oldRank, RankTier newRank) { OldRank = oldRank; NewRank = newRank; }
    }
}
