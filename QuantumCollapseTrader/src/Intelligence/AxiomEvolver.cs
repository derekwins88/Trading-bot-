using System;
using System.Collections.Generic;
using System.Linq;
using SymbolicTrading.Axioms;
using SymbolicTrading.Telemetry;

namespace SymbolicTrading.Intelligence
{
    // Integration: coordinates with QuantumCollapseTrader.cs to adapt axioms
    // Based on trade outcomes recorded in OutcomeMatrix and fractal feedback
    // from PulseRing, this module mutates entries in AxiomRegistry.
    public class AxiomEvolver
    {
        private readonly AxiomRegistry _registry;
        private readonly Dictionary<string, AxiomPerformance> _performanceMetrics = new();
        private readonly IEchoFractalTracker _fractalTracker;

        public AxiomEvolver(AxiomRegistry registry, IEchoFractalTracker fractalTracker)
        {
            _registry = registry;
            _fractalTracker = fractalTracker;
        }

        public void RecordOutcome(string axiomId, double pnlImpact, bool wasTriggered)
        {
            if (!_performanceMetrics.ContainsKey(axiomId))
            {
                _performanceMetrics[axiomId] = new AxiomPerformance();
            }

            var metric = _performanceMetrics[axiomId];
            metric.TotalTriggers++;
            metric.CumulativePnl += pnlImpact;
            if (wasTriggered) metric.ActivationCount++;
        }

        public void EvolveLogic(double evolutionThreshold = 0.65)
        {
            foreach (var (axiomId, metric) in _performanceMetrics.ToList())
            {
                double successRate = metric.SuccessRate;
                double activationRate = metric.ActivationRate;
                if (successRate < evolutionThreshold)
                {
                    MutateAxiom(axiomId, successRate, activationRate);
                }
            }
        }

        private void MutateAxiom(string axiomId, double successRate, double activationRate)
        {
            var axiom = _registry.GetAxiom(axiomId);
            if (axiom == null) return;

            // Evolutionary mutation logic updates the axiom configuration
            string mutationType = successRate < 0.4 ? "REPLACE" : "ADJUST";
            var context = _fractalTracker.GetRecentContext();

            axiom.AdjustWeights(
                activationBoost: activationRate < 0.3 ? 1.5 : 0.8,
                strictnessFactor: successRate < 0.5 ? 0.7 : 1.2
            );

            // Mutation events are tracked so QuantumCollapseTrader.cs can
            // correlate fractal state with axiom performance.
            _fractalTracker.RecordMutation(axiomId, mutationType, context);
        }

        private class AxiomPerformance
        {
            public int TotalTriggers { get; set; }
            public int ActivationCount { get; set; }
            public double CumulativePnl { get; set; }
            public double SuccessRate => TotalTriggers > 0 ? CumulativePnl / TotalTriggers : 0;
            public double ActivationRate => TotalTriggers > 0 ? (double)ActivationCount / TotalTriggers : 0;
        }
    }

    // Interface is implemented by any Axiom that can adjust its weights at runtime
    public interface IAxiomAdjustable
    {
        void AdjustWeights(double activationBoost, double strictnessFactor);
    }
}
