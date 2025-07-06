using System;
using SymbolicTrading.Engine;
using SymbolicTrading.Modules;

namespace SymbolicTrading.Strategies
{
    // Main live strategy processing market data. Coordinates the
    // ThinkingEngine, MarkovPredictor and ConflictMediator each time a new
    // market bar is received.
    public class QuantumCollapseTrader
    {
        private readonly ThinkingEngine _engine = new ThinkingEngine();
        private readonly AxiomRegistry _registry = new AxiomRegistry();
        private readonly ConflictMediator _mediator = new ConflictMediator();
        private readonly MarkovPredictor _predictor = new MarkovPredictor();

        // Called on each trading session start and on every OnBarUpdate to
        // refresh predictions and resolve conflicts before placing orders.
        public void Execute()
        {
            Console.WriteLine("Executing Quantum Collapse Trading Strategy...");
            _predictor.Predict();
            _engine.Process();
            _mediator.Resolve();
        }
    }
}
