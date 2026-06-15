namespace System.Text.RegularExpressions
{
    /// <summary>Represents the method that is called each time a regular expression match is found, returning the replacement string.</summary>
    public delegate string MatchEvaluator(Match match);
}
