using NUnit.Framework;
using BurakOyun.Gameplay;

namespace BurakOyun.Tests
{
    public class WordProgressTests
    {
        [Test]
        public void CorrectLetter_AdvancesIndex()
        {
            var p = new WordProgress("BURAK");
            Assert.AreEqual(WordProgress.SubmitResult.Correct, p.TrySubmit('B'));
            Assert.AreEqual(1, p.CurrentIndex);
            Assert.AreEqual('U', p.TargetLetter);
        }

        [Test]
        public void WrongLetter_DoesNotChangeIndex()
        {
            var p = new WordProgress("BURAK");
            p.TrySubmit('B');
            Assert.AreEqual(WordProgress.SubmitResult.Wrong, p.TrySubmit('X'));
            Assert.AreEqual(1, p.CurrentIndex); // geri sarma yok
        }

        [Test]
        public void FullWord_ReturnsWordComplete()
        {
            var p = new WordProgress("BURAK");
            p.TrySubmit('B'); p.TrySubmit('U'); p.TrySubmit('R'); p.TrySubmit('A');
            Assert.AreEqual(WordProgress.SubmitResult.WordComplete, p.TrySubmit('K'));
            Assert.IsTrue(p.IsComplete);
        }

        [Test]
        public void AfterComplete_SubmitReturnsAlreadyComplete()
        {
            var p = new WordProgress("AB");
            p.TrySubmit('A'); p.TrySubmit('B');
            Assert.AreEqual(WordProgress.SubmitResult.AlreadyComplete, p.TrySubmit('A'));
        }

        [Test]
        public void Reset_StartsOver()
        {
            var p = new WordProgress("AB");
            p.TrySubmit('A');
            p.Reset();
            Assert.AreEqual(0, p.CurrentIndex);
            Assert.AreEqual('A', p.TargetLetter);
        }

        [Test]
        public void CollectedPart_GrowsWithProgress()
        {
            var p = new WordProgress("BURAK");
            Assert.AreEqual("", p.CollectedPart);
            p.TrySubmit('B'); p.TrySubmit('U');
            Assert.AreEqual("BU", p.CollectedPart);
        }
    }
}
