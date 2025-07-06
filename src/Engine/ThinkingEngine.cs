using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using SymbolicTrading.Axioms;
using SymbolicTrading.Signals;

namespace SymbolicTrading.Engine
{
    /// <summary>
    /// Central engine responsible for translating market ticks into glyph phases and resolved axioms.
    /// <para>Integrates with:</para>
    /// <list type="bullet">
    /// <item><description><c>PulseRing</c> invokes <see cref="OnMarketTick"/> on market updates.</description></item>
    /// <item><description><c>Strategy</c> modules observe <see cref="GlyphStream"/> and <see cref="AxiomStream"/>.</description></item>
    /// <item><description><c>Capsules</c> persist events from <see cref="AxiomStream"/>.</description></item>
    /// </list>
    /// </summary>
    public class ThinkingEngine : IDisposable
    {
        // Publishes the generated glyph phases to observers
        private readonly Subject<GlyphPhase> _glyphSubject = new();
        // Publishes resolved axioms after conflict mediation
        private readonly Subject<AxiomEvent> _axiomSubject = new();
        // Handles glyph collapse logic using the registered axioms
        private readonly ReflexGlyphCollapseEngine _collapseEngine;
        // Mediates conflicts among emitted axioms before publishing
        private readonly AxiomConflictMediator _conflictMediator;
        // Translates entropy/drift pairs into symbol identifiers
        private readonly SymbolMapper _symbolMapper = new();

        /// <summary>Observable stream of generated glyph phases.</summary>
        public IObservable<GlyphPhase> GlyphStream => _glyphSubject.AsObservable();
        /// <summary>Observable stream of resolved axiom events.</summary>
        public IObservable<AxiomEvent> AxiomStream => _axiomSubject.AsObservable();

        public ThinkingEngine()
        {
            var axiomRegistry = new AxiomRegistry();
            axiomRegistry.InitializeDefaultAxioms();

            _collapseEngine = new ReflexGlyphCollapseEngine(axiomRegistry);
            _conflictMediator = new AxiomConflictMediator();

            // Forward resolved axioms from mediator to public stream
            _conflictMediator.OnResolved += ax => _axiomSubject.OnNext(ax);
        }

        /// <summary>
        /// Primary entry point for market ticks. Converts the raw values to a
        /// <see cref="GlyphPhase"/> and processes it through the collapse and
        /// mediation pipelines.
        /// </summary>
        public void OnMarketTick(double entropy, double drift, DateTime timestamp)
        {
            string symbol = _symbolMapper.Map(entropy, drift);
            var glyph = new GlyphPhase(symbol, entropy, drift, timestamp);

            _glyphSubject.OnNext(glyph);
            _collapseEngine.ProcessGlyph(glyph);

            foreach (var axiom in _collapseEngine.EmitLatestEvents())
            {
                _conflictMediator.RegisterAxiom(axiom);
            }
            _conflictMediator.ProcessQueue();
        }

        public void Dispose()
        {
            _glyphSubject?.Dispose();
            _axiomSubject?.Dispose();
        }
    }
}
