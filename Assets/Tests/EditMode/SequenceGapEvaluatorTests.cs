using NUnit.Framework;
using UI.RuleEditor;

namespace Tests.EditMode
{
    public class SequenceGapEvaluatorTests
    {
        [Test]
        public void NoPriorEvent_NeverExceedsGap()
        {
            Assert.IsFalse(SequenceGapEvaluator.HasExceededGap(null, 100f, 0.7f));
        }

        [Test]
        public void WithinGap_DoesNotExceed()
        {
            Assert.IsFalse(SequenceGapEvaluator.HasExceededGap(10f, 10.5f, 0.7f));
        }

        [Test]
        public void BeyondGap_Exceeds()
        {
            Assert.IsTrue(SequenceGapEvaluator.HasExceededGap(10f, 11f, 0.7f));
        }

        [Test]
        public void LimitDisabled_WhenZeroOrNegative()
        {
            Assert.IsFalse(SequenceGapEvaluator.HasExceededGap(10f, 1000f, 0f));
            Assert.IsFalse(SequenceGapEvaluator.HasExceededGap(10f, 1000f, -1f));
        }
    }
}
