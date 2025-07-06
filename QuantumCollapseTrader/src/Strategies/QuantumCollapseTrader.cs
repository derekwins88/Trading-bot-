using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using NinjaTrader.NinjaScript.Strategies;
using SymbolicTrading.Engine;
using SymbolicTrading.Risk;
using SymbolicTrading.Execution;

namespace SymbolicTrading.Strategies
{
    // Integration notes:
    // - Consumes market metrics from ThinkingEngine via AxiomStream and GlyphStream.
    // - Uses PositionSizer and DrawdownCircuitBreaker for risk management.
    // - Executes orders with TradeExecutor when glyphs are received.
    public class QuantumCollapseTrader : Strategy
    {
        private ThinkingEngine _engine;
        private PositionSizer _positionSizer;
        private TradeExecutor _tradeExecutor;
        private DrawdownCircuitBreaker _ddBreaker;
        private double _lastVolatility;
        
        // Called by the framework when the strategy state changes. During
        // configuration this hooks up dependencies and subscribes to engine events.
        protected override void OnStateChange()
        {
            if (State == State.Configure)
            {
                // Initialize modules
                _engine = new ThinkingEngine();
                _positionSizer = new PositionSizer();
                _tradeExecutor = new TradeExecutor(this);
                _ddBreaker = new DrawdownCircuitBreaker(this);

                // Wire events from the engine to local handlers
                _engine.AxiomStream.Subscribe(HandleAxiom);
                _engine.GlyphStream.Subscribe(HandleGlyph);
            }
        }

        // Invoked on each market bar. Feeds the engine with entropy and drift
        // metrics then runs drawdown checks.
        protected override void OnBarUpdate()
        {
            if (BarsInProgress != 0 || CurrentBar < 50) return;

            // Calculate market metrics
            double entropy = CalculateEntropy();
            double drift = Close[0] - Close[1];
            
            // Feed engine
            _engine.OnMarketTick(entropy, drift, Time[0]);
            
            // Risk management
            _ddBreaker.CheckDrawdown();
        }

        // Triggered when the ThinkingEngine emits a GlyphPhase. This is where
        // trade orders are generated and sent through the TradeExecutor.
        private void HandleGlyph(GlyphPhase glyph)
        {
            int size = _positionSizer.CalculateSize(
                Account.Get(AccountItem.CashValue), 
                Close[0]
            );
            
            _tradeExecutor.ExecuteTrade(glyph.Symbol, size);
        }

        // Fired when an AxiomEvent is produced. Adjusts the position sizing
        // multiplier based on the event's boost factor.
        private void HandleAxiom(AxiomEvent axiom)
        {
            _positionSizer.ApplyBoostMultiplier(axiom.BoostFactor);
        }

        // Utility used during OnBarUpdate to derive current market entropy.
        private double CalculateEntropy() =>
            Math.Max(0.01, ATR(14)[0] / SMA(14)[0]);
    }
}
