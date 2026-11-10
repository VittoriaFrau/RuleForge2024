namespace UI.RuleEditor
{
    /// <summary>
    /// Decides whether too much time has passed between two consecutive events of a sequence
    /// (paper: 500-700 ms for closely related sequential actions). Plain C# logic, no Unity
    /// dependency: every timestamp is passed in by the caller.
    /// </summary>
    public static class SequenceGapEvaluator
    {
        /// <summary>
        /// Returns true when the gap between <paramref name="lastEventTime"/> and
        /// <paramref name="now"/> exceeds <paramref name="maxGapSeconds"/>.
        /// A <paramref name="maxGapSeconds"/> &lt;= 0 disables the limit (always returns false).
        /// No prior event (<paramref name="lastEventTime"/> is null) never counts as a gap.
        /// </summary>
        public static bool HasExceededGap(float? lastEventTime, float now, float maxGapSeconds)
        {
            if (maxGapSeconds <= 0f) return false;
            if (!lastEventTime.HasValue) return false;
            return now - lastEventTime.Value > maxGapSeconds;
        }
    }
}
