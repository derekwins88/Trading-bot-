namespace SymbolicTrading.Axioms
{
    // Placeholder axiom representing sigil descent processing.
    public class AxiomSigilDescent : IAxiom
    {
        public bool Apply(List<GlyphPhase> history, GlyphPhase current, out AxiomEvent evt)
        {
            evt = new AxiomEvent();
            return false;
        }
    }
}
