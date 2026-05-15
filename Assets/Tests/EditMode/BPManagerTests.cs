using Capybrawlers.Core;
using NUnit.Framework;

namespace Capybrawlers.Tests
{
    public class BPManagerTests
    {
        // ── ComputeRank ──────────────────────────────────────────────────────────

        [TestCase(0,    RankTier.Pup)]
        [TestCase(499,  RankTier.Pup)]
        [TestCase(500,  RankTier.Sprout)]
        [TestCase(999,  RankTier.Sprout)]
        [TestCase(1000, RankTier.Brawler)]
        [TestCase(1999, RankTier.Brawler)]
        [TestCase(2000, RankTier.StoneFang)]
        [TestCase(2999, RankTier.StoneFang)]
        [TestCase(3000, RankTier.Tempest)]
        [TestCase(3999, RankTier.Tempest)]
        [TestCase(4000, RankTier.GrandCapy)]
        [TestCase(9999, RankTier.GrandCapy)]
        public void ComputeRank_ReturnsCorrectTier(int bp, RankTier expected)
        {
            Assert.AreEqual(expected, BPManager.ComputeRank(bp));
        }

        // ── ApplyResult ──────────────────────────────────────────────────────────

        [Test]
        public void Win_AddsTwentyFiveBP()
        {
            var (manager, _) = MakeManager(100);
            manager.ApplyResult(BattleResult.Win);
            Assert.AreEqual(125, manager.CurrentBP);
        }

        [Test]
        public void Loss_SubtractsFifteenBP()
        {
            var (manager, _) = MakeManager(100);
            manager.ApplyResult(BattleResult.Loss);
            Assert.AreEqual(85, manager.CurrentBP);
        }

        [Test]
        public void Loss_DoesNotGoBelowZero()
        {
            var (manager, _) = MakeManager(10);
            manager.ApplyResult(BattleResult.Loss);
            Assert.AreEqual(0, manager.CurrentBP);
        }

        [Test]
        public void ApplyResult_PublishesBPChangedEvent()
        {
            var (manager, events) = MakeManager(0);
            BPChangedEvent? received = null;
            events.Subscribe<BPChangedEvent>(e => received = e);

            manager.ApplyResult(BattleResult.Win);

            Assert.IsNotNull(received);
            Assert.AreEqual(25, received!.Value.NewTotal);
            Assert.AreEqual(25, received!.Value.Delta);
        }

        [Test]
        public void ApplyResult_PublishesRankChangedEvent_OnPromotion()
        {
            var (manager, events) = MakeManager(499);
            RankChangedEvent? received = null;
            events.Subscribe<RankChangedEvent>(e => received = e);

            manager.ApplyResult(BattleResult.Win); // 499 + 25 = 524 → Sprout

            Assert.IsNotNull(received);
            Assert.AreEqual(RankTier.Pup,    received!.Value.OldRank);
            Assert.AreEqual(RankTier.Sprout, received!.Value.NewRank);
        }

        [Test]
        public void ApplyResult_DoesNotPublishRankChangedEvent_WhenRankUnchanged()
        {
            var (manager, events) = MakeManager(100);
            var fired = false;
            events.Subscribe<RankChangedEvent>(_ => fired = true);

            manager.ApplyResult(BattleResult.Win); // stays Pup

            Assert.IsFalse(fired);
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private static (BPManager manager, IEventBus events) MakeManager(int startingBP)
        {
            var events  = new EventBus();
            var profile = new PlayerProfile { bpPoints = startingBP, currentRank = BPManager.ComputeRank(startingBP) };
            var save    = new StubSaveProvider(profile);
            return (new BPManager(save, events), events);
        }

        private class StubSaveProvider : ISaveProvider
        {
            private PlayerProfile _profile;
            public StubSaveProvider(PlayerProfile p) => _profile = p;
            public PlayerProfile Load() => _profile;
            public void Save(PlayerProfile p) => _profile = p;
        }
    }
}
