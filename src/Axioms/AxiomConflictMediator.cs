using System;
using System.Collections.Generic;

namespace SymbolicTrading.Axioms
{
    public class AxiomConflictMediator
    {
        private readonly Queue<AxiomEvent> _queue = new();
        public event Action<AxiomEvent>? OnResolved;

        public void RegisterAxiom(AxiomEvent axiom)
        {
            _queue.Enqueue(axiom);
        }

        public void ProcessQueue()
        {
            while (_queue.Count > 0)
            {
                var ax = _queue.Dequeue();
                OnResolved?.Invoke(ax);
            }
        }
    }
}
