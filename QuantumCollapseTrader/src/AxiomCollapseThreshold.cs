namespace SymbolicTrading.Axioms
{
    // Placeholder implementation of collapse threshold axiom.
    public class AxiomCollapseThreshold : IAxiom
    {
        public bool Apply(List<GlyphPhase> history, GlyphPhase current, out AxiomEvent evt)
        {
            evt = new AxiomEvent();
            return false;
        }
    }
}
