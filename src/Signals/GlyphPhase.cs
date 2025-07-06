using System;

namespace SymbolicTrading.Signals
{
    public class GlyphPhase
    {
        public string Symbol { get; }
        public double Entropy { get; }
        public double Drift { get; }
        public DateTime Timestamp { get; }

        public GlyphPhase(string symbol, double entropy, double drift, DateTime timestamp)
        {
            Symbol = symbol;
            Entropy = entropy;
            Drift = drift;
            Timestamp = timestamp;
        }
    }
}
