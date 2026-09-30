namespace HandEvaluator.Test
{
    /// <summary>
    /// xUnit trait names/values used to categorize tests so a fast default
    /// CI gate can be selected with <c>--filter "Category=Fast"</c>.
    /// </summary>
    public static class Traits
    {
        public const string Category = "Category";

        /// <summary>Default: correctness tests that run in milliseconds to a few seconds.</summary>
        public const string Fast = "Fast";

        /// <summary>Exhaustive enumerations / long-running checks, opt-in only.</summary>
        public const string Slow = "Slow";

        /// <summary>Independent-reference cross-checks, opt-in only.</summary>
        public const string Oracle = "Oracle";

        /// <summary>Throughput/timing guards, opt-in only.</summary>
        public const string Perf = "Perf";
    }
}
