namespace UI.RuleEditor
{
    /// <summary>
    /// Central place for every timing constant used by the Rule Plate.
    /// Values come from the RuleForge paper (Section 5, "Temporal precision", and Section 5.2.3).
    /// </summary>
    public static class RuleTimingConfig
    {
        /// <summary>
        /// Timeout for parallel inputs on a merged cube (paper: 200-300 ms timeout for parallel inputs).
        /// </summary>
        public const float ParallelWindowSeconds = 0.3f;

        /// <summary>
        /// Maximum gap allowed between two consecutive events of a sequence (paper: 500-700 ms
        /// for closely related sequential actions). A value &lt;= 0 disables the limit.
        /// </summary>
        public const float SequenceMaxGapSeconds = 0.7f;

        /// <summary>
        /// Seconds two cubes must stay in contact before they merge into a composite input (paper, Section 5.2.3).
        /// </summary>
        public const float MergeHoldSeconds = 3f;
    }
}
