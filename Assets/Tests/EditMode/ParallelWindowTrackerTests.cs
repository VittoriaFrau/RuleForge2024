using NUnit.Framework;
using UI.RuleEditor;
using UnityEngine;

namespace Tests.EditMode
{
    public class ParallelWindowTrackerTests
    {
        private GameObject goA;
        private GameObject goB;
        private ECAEvent eventA;
        private ECAEvent eventB;

        [SetUp]
        public void SetUp()
        {
            goA = new GameObject("A");
            goB = new GameObject("B");
            eventA = new ECAEvent(goA);
            eventB = new ECAEvent(goB);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(goA);
            Object.DestroyImmediate(goB);
        }

        [Test]
        public void EventsInsideWindow_Complete()
        {
            var tracker = new ParallelWindowTracker(new[] { eventA, eventB }, 0.3f);

            Assert.IsFalse(tracker.RegisterTrigger(eventA, 10f));
            Assert.IsTrue(tracker.RegisterTrigger(eventB, 10.2f));
        }

        [Test]
        public void EventsOutsideWindow_DoNotComplete()
        {
            var tracker = new ParallelWindowTracker(new[] { eventA, eventB }, 0.3f);

            Assert.IsFalse(tracker.RegisterTrigger(eventA, 10f));
            // 0.5s later: window expired, discarded and reopened with eventB alone.
            Assert.IsFalse(tracker.RegisterTrigger(eventB, 10.5f));
            Assert.IsFalse(tracker.IsComplete);
        }

        [Test]
        public void DuplicateEvent_DoesNotCountTwice()
        {
            var tracker = new ParallelWindowTracker(new[] { eventA, eventB }, 0.3f);

            Assert.IsFalse(tracker.RegisterTrigger(eventA, 10f));
            Assert.IsFalse(tracker.RegisterTrigger(eventA, 10.1f)); // same event again
            Assert.IsFalse(tracker.IsComplete);

            Assert.IsTrue(tracker.RegisterTrigger(eventB, 10.2f));
        }

        [Test]
        public void CompletedWindow_ResetsAutomatically()
        {
            var tracker = new ParallelWindowTracker(new[] { eventA, eventB }, 0.3f);

            tracker.RegisterTrigger(eventA, 10f);
            tracker.RegisterTrigger(eventB, 10.1f);

            Assert.IsFalse(tracker.IsComplete);
            Assert.IsFalse(tracker.HasTriggered(eventA));
        }
    }
}
