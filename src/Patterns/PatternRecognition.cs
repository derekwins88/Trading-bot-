using System.Linq;

namespace SymbolicTrading.Patterns
{
    // PatternUtils provides extension methods for PulseRing to detect recent repeating glyph motifs
    // Used for triggering conditional logic based on symbolic pattern recurrence
    public static class PatternUtils
    {
        public static bool ShouldExpectRepeatPattern(this PulseRing pulseRing, params string[] sequence)
        {
            var recent = pulseRing.GetRecentGlyphs(sequence.Length);
            return recent.Select(r => r.Symbol).SequenceEqual(sequence);
        }
    }
}
