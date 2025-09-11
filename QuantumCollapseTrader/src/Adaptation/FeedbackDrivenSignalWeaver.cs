using System;
using System.Collections.Generic;
using System.Linq;
using SymbolicTrading.Memory;
using SymbolicTrading.Signals;

namespace SymbolicTrading.Adaptation
{
    /// <summary>
    /// The FeedbackDrivenSignalWeaver dynamically adjusts glyph signal weights
    /// and motif sequence bias based on recorded trading performance.  The
    /// component works alongside PulseRing and SymbolicOutcomeMatrix to refine
    /// mapping results from SymbolMapper.  QuantumCollapseTrader.cs imports this
    /// module to apply motif-reactive logic when executing trades.
    /// </summary>
    public class FeedbackDrivenSignalWeaver
    {
        private readonly PulseRing _pulseRing;
        private readonly SymbolicOutcomeMatrix _outcomeMatrix;
        private readonly SymbolMapper _symbolMapper;
        private readonly Dictionary<string, double> _glyphWeights = new();
        private readonly Dictionary<string, double> _sequenceAdjustments = new();

        private const double MIN_WEIGHT = 0.3;
        private const double MAX_WEIGHT = 2.0;
        private const double DECAY_RATE = 0.95;

        public FeedbackDrivenSignalWeaver(
            PulseRing pulseRing,
            SymbolicOutcomeMatrix outcomeMatrix,
            SymbolMapper symbolMapper)
        {
            _pulseRing = pulseRing;
            _outcomeMatrix = outcomeMatrix;
            _symbolMapper = symbolMapper;
        }

        /// <summary>
        /// Entry point called by QuantumCollapseTrader to reweave glyph signals.
        /// Updates weights and motif adjustments based on PulseRing statistics
        /// and SymbolicOutcomeMatrix sequences.
        /// </summary>
        public void ReweaveSignals()
        {
            // Update individual glyph weights using PulseRing's performance
            // metrics. Glyph weights influence ReweightedMap() probability.
            UpdateGlyphWeights();

            // Sequence adjustments derived from SymbolicOutcomeMatrix adjust
            // motif-driven bias that GetSequenceAdjustment() returns.
            UpdateSequenceAdjustments();

            // Apply decay each cycle to gradually reduce influence of older
            // performance data, preventing stagnation.
            ApplyWeightDecay();
        }

        /// <summary>
        /// Maps entropy and drift through SymbolMapper while considering
        /// feedback weights.  If a glyph's weight is poor the method can map to
        /// an alternative symbol.  QuantumCollapseTrader uses this to alter
        /// glyph output when trades underperform.
        /// </summary>
        public string ReweightedMap(double entropy, double drift)
        {
            string originalSymbol = _symbolMapper.Map(entropy, drift);

            if (_glyphWeights.TryGetValue(originalSymbol, out double weight))
            {
                // Apply probabilistic adjustment using the learned weight.
                if (weight < 0.5 && new Random().NextDouble() > weight)
                {
                    return FindAlternativeSymbol(entropy, drift, originalSymbol);
                }
            }

            return originalSymbol;
        }

        /// <summary>
        /// Returns a multiplier for the provided symbol based on the latest
        /// motif sequence pattern.  QuantumCollapseTrader invokes this when
        /// evaluating entry logic to make motif-reactive decisions.
        /// </summary>
        public double GetSequenceAdjustment(string currentSymbol)
        {
            var recentGlyphs = _pulseRing.GetRecentGlyphs(2).Select(g => g.Symbol).ToList();
            if (recentGlyphs.Count < 2) return 1.0;

            string sequence = $"{recentGlyphs[0]}→{recentGlyphs[1]}→{currentSymbol}";
            return _sequenceAdjustments.TryGetValue(sequence, out var adj) ? adj : 1.0;
        }

        /// <summary>
        /// Calculate new glyph weights from PulseRing trade statistics. Each
        /// glyph's average PnL and trade count become performance scores that
        /// scale weight between MIN_WEIGHT and MAX_WEIGHT.
        /// </summary>
        private void UpdateGlyphWeights()
        {
            var glyphPerformance = _pulseRing.ExportSummary();
            foreach (var glyphStat in glyphPerformance)
            {
                double performanceScore = CalculatePerformanceScore(glyphStat.avgPnl, glyphStat.count);
                double newWeight = Math.Clamp(performanceScore, MIN_WEIGHT, MAX_WEIGHT);

                if (_glyphWeights.ContainsKey(glyphStat.symbol))
                {
                    // Smooth weight updates using a moving average.
                    _glyphWeights[glyphStat.symbol] =
                        (_glyphWeights[glyphStat.symbol] * 0.7) + (newWeight * 0.3);
                }
                else
                {
                    _glyphWeights[glyphStat.symbol] = newWeight;
                }
            }
        }

        /// <summary>
        /// Derive motif sequence adjustments from SymbolicOutcomeMatrix entries.
        /// High success sequences get a boost, while poor outcomes reduce bias.
        /// </summary>
        private void UpdateSequenceAdjustments()
        {
            var matrix = _outcomeMatrix.ExportMatrix();
            foreach (var entry in matrix)
            {
                double adjustment = 1.0;

                if (entry.successRate > 0.65 && entry.count > 3)
                {
                    adjustment = 1.0 + (0.2 * entry.avgPnl);
                }
                else if (entry.successRate < 0.4 && entry.count > 5)
                {
                    adjustment = 0.8 - (0.1 * Math.Abs(entry.avgPnl));
                }

                _sequenceAdjustments[entry.motif] = Math.Clamp(adjustment, 0.5, 1.5);
            }
        }

        /// <summary>
        /// Slowly fades glyph weights over time to ensure old data doesn't
        /// dominate. Compatible with QuantumCollapseTrader's periodic refresh.
        /// </summary>
        private void ApplyWeightDecay()
        {
            var keys = _glyphWeights.Keys.ToList();
            foreach (var key in keys)
            {
                _glyphWeights[key] = Math.Clamp(_glyphWeights[key] * DECAY_RATE, MIN_WEIGHT, MAX_WEIGHT);
            }
        }

        /// <summary>
        /// Chooses an alternative glyph of similar weight when the original
        /// symbol performs poorly relative to entropy and drift context.
        /// </summary>
        private string FindAlternativeSymbol(double entropy, double drift, string original)
        {
            // Find similar performing glyphs.
            var alternatives = _glyphWeights
                .Where(kv => kv.Value > 0.8)
                .OrderByDescending(kv => kv.Value)
                .Select(kv => kv.Key)
                .Take(3)
                .ToList();

            if (alternatives.Count == 0) return original;

            // Context-aware selection.
            if (entropy > 0.7 && drift < 0)
                return alternatives.FirstOrDefault(a => a == "☍" || a == "⚯") ?? original;

            if (entropy < 0.3 && drift > 0)
                return alternatives.FirstOrDefault(a => a == "🝗" || a == "⧖") ?? original;

            return alternatives[0];
        }

        private double CalculatePerformanceScore(double avgPnl, int count)
        {
            double countFactor = Math.Min(1.0, count / 10.0);
            double pnlFactor = 1.0 + (Math.Sign(avgPnl) * Math.Pow(Math.Abs(avgPnl), 0.5));
            return countFactor * pnlFactor;
        }
    }
}
