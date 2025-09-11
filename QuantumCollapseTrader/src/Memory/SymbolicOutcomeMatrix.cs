using System;
using System.Collections.Generic;
using System.Linq;

namespace SymbolicTrading.Memory
{
    // SymbolicOutcomeMatrix analyzes glyph sequences as motifs and evaluates their historical PnL outcomes
    // Supports exporting pattern effectiveness metrics (avg PnL, success rate, frequency)
    public class SymbolicOutcomeMatrix
    {
        private readonly Dictionary<string, MotifStats> _motifs = new Dictionary<string, MotifStats>();
        private readonly int _motifLen;

        public SymbolicOutcomeMatrix(int motifLength = 3) => 
            _motifLen = motifLength;

        public void Observe(IList<GlyphRecord> trail)
        {
            if (trail.Count < _motifLen) return;

            for (int i = 0; i <= trail.Count - _motifLen; i++)
            {
                var seq = trail.Skip(i).Take(_motifLen).ToList();
                string motif = string.Join("\u2192", seq.Select(r => r.Symbol));
                double totalPnl = seq.Sum(r => r.Pnl);

                if (!_motifs.TryGetValue(motif, out var stats))
                {
                    stats = new MotifStats();
                    _motifs[motif] = stats;
                }
                stats.Update(totalPnl);
            }
        }

        public List<object> ExportMatrix() => 
            _motifs.Select(kvp => new {
                motif = kvp.Key,
                avgPnl = kvp.Value.AvgPnl,
                count = kvp.Value.Count,
                successRate = kvp.Value.SuccessRate
            }).ToList<object>();

        private class MotifStats
        {
            private double _sumPnl;
            private int _wins;
            private int _total;

            public double AvgPnl => _total > 0 ? _sumPnl / _total : 0;
            public int Count => _total;
            public double SuccessRate => _total > 0 ? (double)_wins / _total : 0;

            public void Update(double pnl)
            {
                _total++;
                _sumPnl += pnl;
                if (pnl > 0) _wins++;
            }
        }
    }
}
