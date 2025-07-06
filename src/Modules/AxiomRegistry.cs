using System.Collections.Generic;

namespace SymbolicTrading.Modules
{
    // Stores and retrieves axioms used throughout the engine.
    // Accessed by ThinkingEngine when processing symbolic events and by
    // QuantumCollapseTrader strategy on startup to seed core axioms from
    // Capsules or configuration files.
    public class AxiomRegistry
    {
        private readonly HashSet<string> _axioms = new HashSet<string>();

        public void Register(string axiom)
        {
            _axioms.Add(axiom);
        }

        public bool Contains(string axiom)
        {
            return _axioms.Contains(axiom);
        }
    }
}
