namespace SymbolicTrading.Axioms
{
    // Placeholder XOR fusion axiom.
    public class AxiomXORFusion : IAxiom
    {
        public bool Apply(List<GlyphPhase> history, GlyphPhase current, out AxiomEvent evt)
        {
            evt = new AxiomEvent();
            return false;
        }
    }
}
