using System;
using System.Collections.Generic;
using System.Linq;

namespace SymbolicTrading.Signals
{
    // Integration: records market entropy and links readings with price drift.
    // QuantumCollapseTrader.cs queries this profiler alongside PulseRing data to
    // understand emotional market swings.
    public class SymbolEntropyProfiler
    {
        private readonly Dictionary<string, EntropyProfile> _profiles = new();
        private readonly int _profileSize;

        public SymbolEntropyProfiler(int profileSize = 100)
        {
            _profileSize = profileSize;
        }

        public void RecordEntropy(string symbol, double entropy, double priceDrift)
        {
            if (!_profiles.TryGetValue(symbol, out var profile))
            {
                profile = new EntropyProfile(_profileSize);
                _profiles[symbol] = profile;
            }
            profile.AddReading(entropy, priceDrift);
        }

        public EntropyMetrics GetMetrics(string symbol)
        {
            if (_profiles.TryGetValue(symbol, out var profile))
            {
                return profile.CalculateMetrics();
            }
            return new EntropyMetrics();
        }

        public double GetEmotionalDrift(string symbol)
        {
            var metrics = GetMetrics(symbol);
            return metrics.EntropyVolatility * metrics.DriftCorrelation;
        }

        public class EntropyProfile
        {
            private readonly int _capacity;
            private readonly Queue<double> _entropyReadings = new();
            private readonly Queue<double> _driftReadings = new();

            public EntropyProfile(int capacity) => _capacity = capacity;

            public void AddReading(double entropy, double drift)
            {
                _entropyReadings.Enqueue(entropy);
                _driftReadings.Enqueue(drift);
                if (_entropyReadings.Count > _capacity)
                {
                    _entropyReadings.Dequeue();
                    _driftReadings.Dequeue();
                }
            }

            public EntropyMetrics CalculateMetrics()
            {
                if (_entropyReadings.Count < 5) return new EntropyMetrics();

                return new EntropyMetrics
                {
                    AverageEntropy = _entropyReadings.Average(),
                    EntropyVolatility = CalculateVolatility(_entropyReadings),
                    DriftCorrelation = CalculateCorrelation(),
                    DecayRate = CalculateDecayRate()
                };
            }

            private double CalculateVolatility(IEnumerable<double> values)
            {
                var avg = values.Average();
                var sumSquares = values.Sum(v => Math.Pow(v - avg, 2));
                return Math.Sqrt(sumSquares / values.Count());
            }

            private double CalculateCorrelation()
            {
                // Simple correlation between entropy and price drift
                var entropyArray = _entropyReadings.ToArray();
                var driftArray = _driftReadings.ToArray();
                double sumProd = 0;
                for (int i = 0; i < entropyArray.Length; i++)
                {
                    sumProd += entropyArray[i] * driftArray[i];
                }
                return sumProd / (entropyArray.Length * entropyArray.Average() * driftArray.Average());
            }

            private double CalculateDecayRate()
            {
                // Calculate entropy decay rate over last 10% of readings
                int count = _entropyReadings.Count;
                int sampleSize = Math.Max(5, count / 10);
                var recent = _entropyReadings.TakeLast(sampleSize).ToArray();
                return (recent.Last() - recent.First()) / sampleSize;
            }
        }

        public struct EntropyMetrics
        {
            public double AverageEntropy { get; set; }
            public double EntropyVolatility { get; set; }
            public double DriftCorrelation { get; set; }
            public double DecayRate { get; set; }
        }
    }
}
