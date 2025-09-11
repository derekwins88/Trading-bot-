namespace SymbolicTrading.Axioms
{
    // Maintains a list of axioms used throughout the glyph processing pipeline.
    // Provides registration and default initialization for the engine.
    public class AxiomRegistry
    {
        private readonly List<IAxiom> _axioms = new();

        public void Register(IAxiom axiom) => _axioms.Add(axiom);
        public IReadOnlyList<IAxiom> GetAll() => _axioms.AsReadOnly();
        
        public void InitializeDefaultAxioms()
        {
            Register(new AxiomCollapseThreshold());
            Register(new AxiomXORFusion());
            Register(new AxiomSigilDescent());
            Register(new AxiomQuantumEntanglement());
        }
    }
}
