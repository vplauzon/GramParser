namespace PasApiClient
{
    /// <summary>Named rule match.</summary>
    public class NamedRuleMatchResult
    {
        /// <summary>Name of the match (specified in grammar).</summary>
        public string Name { get; set; }

        /// <summary>Match.</summary>
        public RuleMatchResult Match { get; set; }
    }
}