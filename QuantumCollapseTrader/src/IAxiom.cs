namespace SymbolicTrading.Axioms
{
    // Defines an axiom that can be applied to glyph phases.
    public interface IAxiom
    {
        bool Apply(List<GlyphPhase> history, GlyphPhase current, out AxiomEvent evt);
    }
}
