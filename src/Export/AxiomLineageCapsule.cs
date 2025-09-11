using System;
using System.Collections.Generic;

namespace SymbolicTrading.Export
{
    // AxiomLineageCapsule defines structured axiom metadata for archival, versioning, and reference
    public class AxiomLineageCapsule
    {
        public string CapsuleID { get; set; } = "AXIOM⇌STABLE⇌025–085";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public List<AxiomEntry> Axioms { get; set; } = new()
        {
            new("AXIOM⇌025", "Quantum Collapse", "Failsafe trade closure"),
            new("AXIOM⇌029", "XOR Fusion Boost", "Amplifies signal strength"),
            new("AXIOM⇌053", "Sigil Descent", "Reduces trade size"),
            new("AXIOM⇌061", "Entanglement", "Position sizing boost")
        };
    }

    public class AxiomEntry
    {
        public string AxiomId { get; }
        public string Label { get; }
        public string Description { get; }

        public AxiomEntry(string id, string label, string desc) =>
            (AxiomId, Label, Description) = (id, label, desc);
    }
}
