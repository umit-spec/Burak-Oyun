using NUnit.Framework;
using UnityEngine;
using BurakOyun.Core;

namespace BurakOyun.Tests
{
    /// <summary>
    /// SaveManager — lokal PlayerPrefs yıldız kaydı testleri.
    /// Her test sonunda ilgili key temizlenir.
    /// </summary>
    public class SaveManagerTests
    {
        const string Key = "BestStars";

        [TearDown]
        public void Cleanup() => PlayerPrefs.DeleteKey(Key);

        [Test]
        public void BestStars_DefaultsToZero_WhenNoSaveExists()
        {
            PlayerPrefs.DeleteKey(Key);
            Assert.AreEqual(0, SaveManager.BestStars);
        }

        [Test]
        public void ReportStars_SavesScore_WhenNoPreviousSave()
        {
            SaveManager.ReportStars(3);
            Assert.AreEqual(3, SaveManager.BestStars);
        }

        [Test]
        public void ReportStars_Updates_WhenNewScoreIsHigher()
        {
            SaveManager.ReportStars(3);
            SaveManager.ReportStars(7);
            Assert.AreEqual(7, SaveManager.BestStars);
        }

        [Test]
        public void ReportStars_DoesNotOverwrite_WhenNewScoreIsLower()
        {
            SaveManager.ReportStars(5);
            SaveManager.ReportStars(2);
            Assert.AreEqual(5, SaveManager.BestStars);
        }

        [Test]
        public void ReportStars_DoesNotOverwrite_WhenNewScoreIsEqual()
        {
            SaveManager.ReportStars(4);
            SaveManager.ReportStars(4);
            Assert.AreEqual(4, SaveManager.BestStars);
        }
    }
}
