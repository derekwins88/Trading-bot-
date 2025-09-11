namespace SymbolicTrading.Axioms
{
    // Drives glyph accumulation and applies registered axioms to incoming phases.
    // Emits axiom events which downstream systems may consume.
    public class ReflexGlyphCollapseEngine
    {
        private readonly Queue<GlyphPhase> _glyphBuffer = new(50);
        private readonly AxiomRegistry _axiomRegistry;
        private readonly Queue<AxiomEvent> _axiomQueue = new();

        public ReflexGlyphCollapseEngine(AxiomRegistry registry) =>
            _axiomRegistry = registry;

        public void ProcessGlyph(GlyphPhase current)
        {
            if (_glyphBuffer.Count >= 50) _glyphBuffer.Dequeue();
            _glyphBuffer.Enqueue(current);
            
            foreach (var axiom in _axiomRegistry.GetAll())
            {
                if (axiom.Apply(_glyphBuffer.ToList(), current, out var evt))
                {
                    _axiomQueue.Enqueue(evt);
                }
            }
        }

        public IEnumerable<AxiomEvent> EmitLatestEvents()
        {
            while (_axiomQueue.Count > 0)
                yield return _axiomQueue.Dequeue();
        }
    }
}
