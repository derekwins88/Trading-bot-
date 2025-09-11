using System;
using System.Collections.Generic;
using System.Linq;
using SymbolicTrading.Memory;

namespace SymbolicTrading.Prediction
{
    // Uses PulseRing and SymbolicOutcomeMatrix to predict next glyph symbol and confidence
    // Combines simple Markov logic with motif-weighted outcomes
    // Intended to feed into thinking engine or trade decision layers
    public class TemporalEchoPredictor
    {
        private readonly PulseRing _pulseRing;
        private readonly SymbolicOutcomeMatrix _outcomeMatrix;
        private readonly int _lookbackDepth;
        private readonly double _motifWeightThreshold;

        public TemporalEchoPredictor(
            PulseRing pulseRing,
            SymbolicOutcomeMatrix outcomeMatrix,
            int lookbackDepth = 5,
            double motifWeightThreshold = 0.7)
        {
            _pulseRing = pulseRing;
            _outcomeMatrix = outcomeMatrix;
            _lookbackDepth = lookbackDepth;
            _motifWeightThreshold = motifWeightThreshold;
        }

        public (string Symbol, double Confidence) PredictNextSymbol()
        {
            var recentGlyphs = _pulseRing.GetRecentGlyphs(_lookbackDepth).Select(g => g.Symbol).ToArray();
            if (recentGlyphs.Length == 0) return ("◇", 0);

            // Get probabilistic prediction
            var markovPrediction = PredictViaMarkovChain(recentGlyphs);

            // Get motif-based prediction
            var motifPrediction = PredictViaMotifPatterns(recentGlyphs);

            // Combine predictions with weighted confidence
            return CombinePredictions(markovPrediction, motifPrediction);
        }

        private (string Symbol, double Confidence) PredictViaMarkovChain(string[] sequence)
        {
            // Simplified Markov implementation (replace with actual Markov model)
            var lastSymbol = sequence.Last();
            var possibleTransitions = new Dictionary<string, int>();

            foreach (var glyph in sequence)
            {
                // Count transitions in historical sequence
                for (int i = 0; i < sequence.Length - 1; i++)
                {
                    if (sequence[i] == lastSymbol)
                    {
                        string next = sequence[i + 1];
                        if (!possibleTransitions.ContainsKey(next))
                            possibleTransitions[next] = 0;
                        possibleTransitions[next]++;
                    }
                }
            }

            if (possibleTransitions.Count == 0) return (lastSymbol, 0.5);

            var mostLikely = possibleTransitions.OrderByDescending(kv => kv.Value).First();
            double confidence = (double)mostLikely.Value / possibleTransitions.Values.Sum();
            return (mostLikely.Key, confidence);
        }

        private (string Symbol, double Confidence) PredictViaMotifPatterns(string[] sequence)
        {
            var matrix = _outcomeMatrix.ExportMatrix();
            var candidateSymbols = new Dictionary<string, double>();
            int sequenceLength = sequence.Length;

            foreach (var motifEntry in matrix)
            {
                string[] parts = motifEntry.motif.Split("→");
                if (parts.Length <= sequenceLength) continue;

                // Check if current sequence matches the beginning of this motif
                bool matches = true;
                for (int i = 0; i < sequenceLength; i++)
                {
                    if (parts[i] != sequence[i])
                    {
                        matches = false;
                        break;
                    }
                }

                if (matches && motifEntry.successRate > _motifWeightThreshold)
                {
                    string nextSymbol = parts[sequenceLength];
                    double weight = motifEntry.successRate * motifEntry.avgPnl;

                    if (!candidateSymbols.ContainsKey(nextSymbol))
                        candidateSymbols[nextSymbol] = 0;

                    candidateSymbols[nextSymbol] += weight;
                }
            }

            if (candidateSymbols.Count == 0) return ("◇", 0);

            var bestCandidate = candidateSymbols.OrderByDescending(kv => kv.Value).First();
            double totalWeight = candidateSymbols.Values.Sum();
            double confidence = bestCandidate.Value / totalWeight;

            return (bestCandidate.Key, confidence);
        }

        private (string Symbol, double Confidence) CombinePredictions(
            (string Symbol, double Confidence) markov,
            (string Symbol, double Confidence) motif)
        {
            if (motif.Confidence > 0.7) return motif;
            if (markov.Confidence > 0.8) return markov;

            // Weighted combination
            double combinedConfidence = (markov.Confidence * 0.4) + (motif.Confidence * 0.6);
            string combinedSymbol = motif.Confidence > markov.Confidence ? motif.Symbol : markov.Symbol;

            return (combinedSymbol, combinedConfidence);
        }
    }
}

