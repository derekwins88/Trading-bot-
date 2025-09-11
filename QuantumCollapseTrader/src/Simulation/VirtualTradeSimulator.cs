using System;
using System.Collections.Generic;
using System.Linq;
using SymbolicTrading.Signals;
using SymbolicTrading.Axioms;

namespace SymbolicTrading.Simulation
{
    // Integration Notes:
    // - Communicates with PulseRing to inject glyphs and market pulses, typically via InjectChaos().
    // - StrategyAvatar queries this simulator to AdvanceMarket() and generate GlyphPhase sequences.
    // - ShardManager may RetrieveContext() from GlyphHistory and AxiomHistory for shard synchronization.
    // - ThinkingEngine uses AdvanceMarket() to progress scenarios and analyze resulting signals.
    //
    // Lifecycle:
    // - Instantiated at runtime by the simulation host when a new scenario is loaded.
    // - Mutated each time AdvanceMarket() or InjectGlyph() is called, updating market state.
    // - Serialized by ShardManager when snapshots of GlyphHistory are required.
    // - Reset() clears internal state, typically between strategy runs or when scenarios change.
    public class VirtualTradeSimulator
    {
        private readonly SymbolMapper _symbolMapper = new();
        private readonly Random _random = new();
        private MarketScenario _currentScenario;
        
        public double CurrentPrice { get; private set; }
        public double CurrentEntropy { get; private set; }
        public double CurrentDrift { get; private set; }
        public List<GlyphPhase> GlyphHistory { get; } = new();
        public List<AxiomEvent> AxiomHistory { get; } = new();

        public void InitializeScenario(MarketScenario scenario, double startingPrice)
        {
            _currentScenario = scenario;
            CurrentPrice = startingPrice;
            CurrentEntropy = scenario.BaseEntropy;
            CurrentDrift = 0;
            GlyphHistory.Clear();
            AxiomHistory.Clear();
        }

        // AdvanceMarket is typically called by StrategyAvatar or ThinkingEngine to progress the
        // simulated timeline. Each step updates the market state, generates a new glyph, and
        // potentially triggers axioms.
        public void AdvanceMarket(int steps = 1)
        {
            for (int i = 0; i < steps; i++)
            {
                // Update market state based on scenario
                UpdateMarketState();
                
                // Generate glyph
                string symbol = _symbolMapper.Map(CurrentEntropy, CurrentDrift);
                var glyph = new GlyphPhase(symbol, CurrentEntropy, CurrentDrift, DateTime.UtcNow);
                GlyphHistory.Add(glyph);
                
                // Simulate axiom processing
                SimulateAxioms(glyph);
            }
        }

        private void UpdateMarketState()
        {
            // Apply scenario-specific market dynamics
            switch (_currentScenario)
            {
                case MarketScenario.TrendingBull:
                    CurrentDrift = _random.NextDouble() * 0.2;
                    CurrentEntropy = 0.4 + (_random.NextDouble() * 0.3);
                    break;
                    
                case MarketScenario.TrendingBear:
                    CurrentDrift = -(_random.NextDouble() * 0.2);
                    CurrentEntropy = 0.5 + (_random.NextDouble() * 0.3);
                    break;
                    
                case MarketScenario.VolatileBreakout:
                    CurrentDrift = (_random.NextDouble() - 0.5) * 0.5;
                    CurrentEntropy = 0.7 + (_random.NextDouble() * 0.25);
                    break;
                    
                case MarketScenario.LowVolatility:
                    CurrentDrift = (_random.NextDouble() - 0.5) * 0.05;
                    CurrentEntropy = 0.2 + (_random.NextDouble() * 0.2);
                    break;
                    
                case MarketScenario.CrashScenario:
                    CurrentDrift = -(_random.NextDouble() * 0.8);
                    CurrentEntropy = 0.9 + (_random.NextDouble() * 0.1);
                    break;
            }
            
            // Update price
            CurrentPrice *= (1 + CurrentDrift);
        }

        private void SimulateAxioms(GlyphPhase glyph)
        {
            // Simulate axiom triggers based on glyph patterns
            if (GlyphHistory.Count > 3)
            {
                string recentPattern = string.Join("", GlyphHistory.TakeLast(3).Select(g => g.Symbol));
                
                // Simulate axiom triggers based on pattern
                if (recentPattern.Contains("☍☍☍") && _random.NextDouble() > 0.7)
                {
                    AxiomHistory.Add(new AxiomEvent("AXIOM⇌025", "Quantum Collapse (Simulated)"));
                }
                
                if (recentPattern.Contains("⚯") && glyph.Entropy > 0.8 && _random.NextDouble() > 0.6)
                {
                    AxiomHistory.Add(new AxiomEvent("AXIOM⇌029", "XOR Fusion (Simulated)"));
                }
            }
        }

        // InjectGlyph can be triggered by PulseRing or external modules to introduce specific
        // glyphs into the simulation without altering the scenario dynamics.
        public void InjectGlyph(string symbol, double entropy, double drift)
        {
            var customGlyph = new GlyphPhase(symbol, entropy, drift, DateTime.UtcNow);
            GlyphHistory.Add(customGlyph);
            SimulateAxioms(customGlyph);
        }

        // Reset is used when the ThinkingEngine or ShardManager signals a restart of the market
        // simulation. It clears history and reverts to default values.
        public void Reset()
        {
            GlyphHistory.Clear();
            AxiomHistory.Clear();
            CurrentPrice = 100; // Default starting price
            CurrentEntropy = 0.5;
            CurrentDrift = 0;
        }
    }

    public enum MarketScenario
    {
        TrendingBull,
        TrendingBear,
        VolatileBreakout,
        LowVolatility,
        CrashScenario,
        Custom
    }

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

    public class AxiomEvent
    {
        public string AxiomId { get; }
        public string Description { get; }

        public AxiomEvent(string axiomId, string description)
        {
            AxiomId = axiomId;
            Description = description;
        }
    }
}
