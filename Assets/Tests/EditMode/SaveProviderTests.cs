using System.IO;
using Capybrawlers.Core;
using NUnit.Framework;
using UnityEngine;

namespace Capybrawlers.Tests
{
    // These tests write to Application.persistentDataPath and clean up after themselves.
    public class SaveProviderTests
    {
        private string _savedPath;
        private LocalJsonSaveProvider _provider;

        [SetUp]
        public void SetUp()
        {
            _provider  = new LocalJsonSaveProvider();
            _savedPath = Path.Combine(Application.persistentDataPath, "profile.dat");
            if (File.Exists(_savedPath)) File.Delete(_savedPath);
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_savedPath)) File.Delete(_savedPath);
        }

        [Test]
        public void Load_ReturnsNull_WhenNoFileExists()
        {
            Assert.IsNull(_provider.Load());
        }

        [Test]
        public void SaveAndLoad_RoundTrip_PreservesAllFields()
        {
            var original = new PlayerProfile
            {
                playerId     = "test-id-123",
                displayName  = "SwiftCapy456",
                bpPoints     = 1500,
                currentRank  = RankTier.Brawler,
                accountLevel = 3,
            };

            _provider.Save(original);
            var loaded = _provider.Load();

            Assert.IsNotNull(loaded);
            Assert.AreEqual(original.playerId,     loaded.playerId);
            Assert.AreEqual(original.displayName,  loaded.displayName);
            Assert.AreEqual(original.bpPoints,     loaded.bpPoints);
            Assert.AreEqual(original.currentRank,  loaded.currentRank);
            Assert.AreEqual(original.accountLevel, loaded.accountLevel);
        }

        [Test]
        public void Save_WritesEncryptedBytes_NotPlainJson()
        {
            _provider.Save(new PlayerProfile { displayName = "SneakyRock789" });
            var raw = File.ReadAllText(_savedPath);
            StringAssert.DoesNotContain("SneakyRock789", raw);
        }
    }
}
