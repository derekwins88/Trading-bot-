using System;
using System.Collections.Generic;
using System.Linq;

namespace SymbolicTrading.Memory
{
    /// <summary>
    /// PulseRing logs recent glyph performance over a sliding window.
    /// Supports PnL tracking, frequency analysis, and exportable summaries
    /// for symbolic learning loops.
    /// </summary>
    public class PulseRing
    {
        private readonly int _windowSize;
        private readonly LinkedList<GlyphRecord> _records;

        public PulseRing(int windowSize = 100)
        {
            _windowSize = windowSize;
            _records = new LinkedList<GlyphRecord>();
        }

        public void Record(string symbol, double pnl)
        {
            var rec = new GlyphRecord { Symbol = symbol, Pnl = pnl, Time = DateTime.UtcNow };
            _records.AddLast(rec);
            if (_records.Count > _windowSize)
                _records.RemoveFirst();
        }

        public double? AveragePnlFor(string symbol)
        {
            var matches = _records.Where(r => r.Symbol == symbol).ToList();
            return matches.Any() ? matches.Average(r => r.Pnl) : (double?)null;
        }

        public int CountFor(string symbol) => 
            _records.Count(r => r.Symbol == symbol);

        public List<GlyphRecord> GetRecentGlyphs(int count = 5) => 
            _records.TakeLast(count).ToList();

        public object ExportSummary() => 
            _records
                .GroupBy(r => r.Symbol)
                .Select(g => new {
                    symbol = g.Key,
                    avgPnl = g.Average(r => r.Pnl),
                    count = g.Count()
                }).ToList();
    }

    public class GlyphRecord
    {
        public string Symbol { get; set; }
        public double Pnl { get; set; }
        public DateTime Time { get; set; }
    }
}
