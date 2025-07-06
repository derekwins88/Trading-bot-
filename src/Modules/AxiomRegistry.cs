using System.Collections.Generic;

namespace QuantumCollapseTrader.Modules
{
    // Stores and retrieves axioms used throughout the engine
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
