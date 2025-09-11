using System;
using System.Collections.Generic;
using System.Linq;
using SymbolicTrading.Signals;

namespace SymbolicTrading.ChaosEngineering
{
    /// <summary>
    /// Injects controlled chaos into trading glyphs and axioms.
    /// 
    /// Communications:
    /// - <b>PulseRing</b>: receives glyph phases from the real-time market feed. Chaos-modified glyphs
    ///   are passed back into PulseRing for downstream processing.
    /// - <b>StrategyAvatar</b>: strategies may toggle modes via <see cref="Configure"/> during
    ///   scenario exploration or stress testing.
    /// - <b>ShardManager</b>: chaos events may be serialized into shards when the engine state
    ///   persists; deserialization resets <see cref="CurrentMode"/>.
    /// - <b>ThinkingEngine</b>: executed rules can query <see cref="CurrentMode"/> to reason about
    ///   the level of uncertainty in the current simulation step.
    /// 
    /// Typical usage:
    /// <code>
    /// var injector = new AxiomChaosInjector();
    /// injector.Configure(0.1, ChaosMode.TimeAnomalies);
    /// var phase = injector.InjectChaos(originalPhase);
    /// var axiomFlood = injector.InjectAxiomChaos();
    /// </code>
    /// 
    /// Lifecycle:
    /// - Instantiated at the beginning of a simulation run by the simulation harness.
    /// - <see cref="Configure"/> may mutate the injection probability or mode between simulation
    ///   ticks when StrategyAvatar adjusts parameters.
    /// - Chaos events generated during <see cref="InjectChaos"/> or <see cref="InjectAxiomChaos"/>
    ///   can be serialized by ShardManager if the engine state is persisted.
    /// - Reset by creating a new instance or reloading from persisted state.
    /// </summary>
    public class AxiomChaosInjector
    {
        private readonly Random _random = new();
        private double _injectionProbability = 0.05;
        
        public enum ChaosMode
        {
            EdgeCases,
            MalformedGlyphs,
            AxiomFlood,
            TimeAnomalies,
            QuantumDecoherence
        }

        public ChaosMode CurrentMode { get; private set; } = ChaosMode.EdgeCases;

        /// <summary>
        /// Adjusts the probability and mode of chaos injection.
        /// </summary>
        public void Configure(double probability, ChaosMode mode)
        {
            _injectionProbability = Math.Clamp(probability, 0.01, 0.5);
            CurrentMode = mode;
        }

        /// <summary>
        /// Potentially mutates a <see cref="GlyphPhase"/> according to the current mode.
        /// </summary>
        public GlyphPhase InjectChaos(GlyphPhase original)
        {
            if (_random.NextDouble() > _injectionProbability)
                return original;

            return CurrentMode switch
            {
                ChaosMode.EdgeCases => GenerateEdgeCaseGlyph(),
                ChaosMode.MalformedGlyphs => GenerateMalformedGlyph(),
                ChaosMode.AxiomFlood => original, // Handled separately
                ChaosMode.TimeAnomalies => GenerateTimeAnomalyGlyph(original),
                ChaosMode.QuantumDecoherence => GenerateDecoherenceGlyph(original),
                _ => original
            };
        }

        /// <summary>
        /// Generates a burst of axiom events when in <see cref="ChaosMode.AxiomFlood"/>.
        /// </summary>
        public List<AxiomEvent> InjectAxiomChaos()
        {
            if (CurrentMode != ChaosMode.AxiomFlood || 
                _random.NextDouble() > _injectionProbability)
            {
                return new List<AxiomEvent>();
            }

            int floodCount = _random.Next(5, 20);
            var flood = new List<AxiomEvent>();
            
            for (int i = 0; i < floodCount; i++)
            {
                flood.Add(new AxiomEvent(
                    axiomId: $"CHAOS⇌{_random.Next(1000, 9999)}",
                    description: "Axiom flood injection"
                ));
            }
            
            return flood;
        }

        private GlyphPhase GenerateEdgeCaseGlyph()
        {
            var edgeCases = new[]
            {
                new GlyphPhase("", double.NaN, double.NaN, DateTime.MinValue),
                new GlyphPhase("∞", double.PositiveInfinity, double.NegativeInfinity, DateTime.MaxValue),
                new GlyphPhase("∅", 0, 0, DateTime.UtcNow),
                new GlyphPhase("�", -1, -1, DateTime.UtcNow)
            };
            
            return edgeCases[_random.Next(edgeCases.Length)];
        }

        private GlyphPhase GenerateMalformedGlyph()
        {
            string malformedSymbol = _random.Next(0, 4) switch
            {
                0 => new string('☍', 1000), // Symbol flood
                1 => "INVALID_SYMBOL_123",
                2 => "🚫🤯💥❌", // Multiple emojis
                _ => null // Null symbol
            };
            
            return new GlyphPhase(
                malformedSymbol,
                _random.NextDouble() * 100,
                _random.NextDouble() - 0.5,
                DateTime.UtcNow
            );
        }

        private GlyphPhase GenerateTimeAnomalyGlyph(GlyphPhase original)
        {
            DateTime anomalousTime = _random.Next(0, 3) switch
            {
                0 => DateTime.MinValue,
                1 => DateTime.MaxValue,
                2 => DateTime.UtcNow.AddYears(-100),
                _ => DateTime.UtcNow.AddYears(100)
            };
            
            return new GlyphPhase(
                original.Symbol,
                original.Entropy,
                original.Drift,
                anomalousTime
            );
        }

        private GlyphPhase GenerateDecoherenceGlyph(GlyphPhase original)
        {
            return new GlyphPhase(
                symbol: original.Symbol + "~" + GetRandomSymbol(),
                entropy: original.Entropy * (_random.NextDouble() * 10),
                drift: original.Drift * (_random.NextDouble() - 0.5) * 100,
                timestamp: original.Timestamp
            );
        }

        private string GetRandomSymbol()
        {
            string[] symbols = { "☍", "⚯", "🝗", "⧖", "◯", "↻", "⥂", "◇" };
            return symbols[_random.Next(symbols.Length)];
        }
    }
}
