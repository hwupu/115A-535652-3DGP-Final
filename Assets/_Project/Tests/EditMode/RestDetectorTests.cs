using NUnit.Framework;
using Project3DGP.Core.Motion;

namespace Project3DGP.Game.Tests
{
    /// <summary>RestDetector (#14): "still for long enough" and the timeout, without physics.</summary>
    public class RestDetectorTests
    {
        const float Step = 0.02f;   // one physics step (50 Hz)

        static RestDetector Make()
        {
            var detector = new RestDetector(speedThreshold: 0.15f, restSeconds: 0.5f, timeoutSeconds: 20f);
            detector.Begin();
            return detector;
        }

        // Feeds the same speed for some seconds; returns true if the detector said "stopped" at any step.
        static bool Run(RestDetector detector, float speed, float seconds)
        {
            var stopped = false;
            for (var t = 0f; t < seconds - 1e-4f; t += Step)
                stopped |= detector.Tick(speed, Step);
            return stopped;
        }

        [Test]
        public void Fast_NeverStopsBeforeTimeout()
        {
            Assert.That(Run(Make(), 3f, 10f), Is.False);
        }

        [Test]
        public void Slow_StopsAfterRestSeconds()
        {
            var detector = Make();
            Assert.That(Run(detector, 0.05f, 0.4f), Is.False, "not yet: only 0.4 s still");
            Assert.That(Run(detector, 0.05f, 0.2f), Is.True, "0.6 s still");
            Assert.That(detector.TimedOut, Is.False);
        }

        [Test]
        public void ShortSlowMoment_LikeTopOfBounce_DoesNotStop()
        {
            var detector = Make();
            Assert.That(Run(detector, 0.05f, 0.3f), Is.False);   // slow at the top of a bounce...
            Assert.That(Run(detector, 2f, 0.1f), Is.False);      // ...then fast again: the still clock restarts
            Assert.That(Run(detector, 0.05f, 0.3f), Is.False);
        }

        [Test]
        public void StuckRocking_StopsAtTimeout()
        {
            var detector = Make();
            var stopped = false;
            for (var i = 0; i < 1100 && !stopped; i++)   // 22 s of alternating slow/fast
                stopped = detector.Tick(i % 10 == 0 ? 1f : 0.05f, Step);
            Assert.That(stopped, Is.True);
            Assert.That(detector.TimedOut, Is.True);
        }

        [Test]
        public void Begin_RestartsTheClocks()
        {
            var detector = Make();
            Run(detector, 0.05f, 0.4f);
            detector.Begin();
            Assert.That(Run(detector, 0.05f, 0.4f), Is.False);
        }
    }
}
