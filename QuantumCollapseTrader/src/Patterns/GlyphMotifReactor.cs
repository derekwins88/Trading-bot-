using System;
using System.Collections.Generic;
using System.Linq;
using SymbolicTrading.Memory;

namespace SymbolicTrading.Patterns
{
    // Integration: leverages OutcomeMatrix statistics and recent PulseRing glyphs
    // to shape trade bias used by QuantumCollapseTrader.cs. Motifs that yield
    // strong performance get stored as boosts while weak motifs signal avoidance.
    public class GlyphMotifReactor
    {
        private readonly SymbolicOutcomeMatrix _outcomeMatrix;
        private readonly PulseRing _pulseRing;
        private readonly Dictionary<string, double> _motifBoosts = new();

        public GlyphMotifReactor(SymbolicOutcomeMatrix outcomeMatrix, PulseRing pulseRing)
        {
            _outcomeMatrix = outcomeMatrix;
            _pulseRing = pulseRing;
        }

        public void RefreshMotifBoosts(double successThreshold = 0.75)
        {
            var matrix = _outcomeMatrix.ExportMatrix();
            foreach (var entry in matrix)
            {
                if (entry.successRate >= successThreshold && entry.count > 5)
                {
                    // Calculate boost based on performance
                    double boost = 1.0 + (0.1 * entry.avgPnl);
                    _motifBoosts[entry.motif] = Math.Min(boost, 2.0);
                }
            }
        }

        public double GetPositionBoost()
        {
            var recentGlyphs = _pulseRing.GetRecentGlyphs(3);
            if (recentGlyphs.Count < 3) return 1.0;

            string motif = string.Join("→", recentGlyphs.Select(g => g.Symbol));
            return _motifBoosts.TryGetValue(motif, out var boost) ? boost : 1.0;
        }

        public bool ShouldAvoidTrade(string currentSymbol)
        {
            var recent = _pulseRing.GetRecentGlyphs(2).Select(g => g.Symbol).ToList();
            if (recent.Count < 2) return false;

            // Avoid repeating losing patterns that are reflected in OutcomeMatrix
            string sequence = $"{recent[0]}→{recent[1]}→{currentSymbol}";
            return _motifBoosts.TryGetValue(sequence, out var boost) && boost < 0.9;
        }
    }
}
