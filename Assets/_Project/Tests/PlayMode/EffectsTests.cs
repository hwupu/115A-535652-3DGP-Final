using System.Collections.Generic;
using NUnit.Framework;
using Project3DGP.Game.Effects;
using UnityEngine;

namespace Project3DGP.Game.Tests
{
    /// <summary>Particle effects (#39): a trigger effect jumps to the ball and plays; the same instance is reused.</summary>
    public class EffectsTests
    {
        readonly List<GameObject> _objects = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var go in _objects)
                if (go != null) Object.Destroy(go);
            _objects.Clear();
        }

        [Test]
        public void BallTriggerEffect_PlaysAtTheBall_ReusingOneInstance()
        {
            var effectObject = new GameObject("Splash");
            _objects.Add(effectObject);
            var particles = effectObject.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.playOnAwake = false;
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var pond = new GameObject("Pond");
            _objects.Add(pond);
            pond.AddComponent<BoxCollider>().isTrigger = true;
            var trigger = pond.AddComponent<BallTriggerEffect>();
            trigger.SetForTests(particles);

            trigger.Play(new Vector3(2f, 0f, 5f));
            Assert.That(particles.isPlaying, Is.True);
            Assert.That(effectObject.transform.position, Is.EqualTo(new Vector3(2f, 0f, 5f)));

            trigger.Play(new Vector3(-1f, 0f, 0f));
            Assert.That(effectObject.transform.position, Is.EqualTo(new Vector3(-1f, 0f, 0f)), "the same system moved");
        }
    }
}
