namespace SymbolicTrading.Axioms
{
    // Central coordinator that sets up the axiom registry and glyph collapse engine.
    public class ThinkingEngine
    {
        private readonly AxiomRegistry _registry;
        private readonly ReflexGlyphCollapseEngine _engine;

        public ThinkingEngine()
        {
            _registry = new AxiomRegistry();
            _registry.InitializeDefaultAxioms();
            _engine = new ReflexGlyphCollapseEngine(_registry);
        }

        public void Process(GlyphPhase phase) => _engine.ProcessGlyph(phase);

        public IEnumerable<AxiomEvent> EmitEvents() => _engine.EmitLatestEvents();
    }
}
