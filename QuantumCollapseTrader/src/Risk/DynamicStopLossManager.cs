// DynamicStopLossManager adjusts stops using ATR and symbolic state compression
using NinjaTrader.NinjaScript;
using NinjaTrader.Cbi;
using System;

namespace SymbolicTrading.Risk
{
    public class DynamicStopLossManager
    {
        private readonly Strategy _strategy;
        private double _compressionFactor = 1.0;
        private double _quantumFactor = 1.0;

        public DynamicStopLossManager(Strategy strategy) => _strategy = strategy;

        public void OnQuantumCollapse() => _quantumFactor = 0.5;
        public void OnCompression() => _compressionFactor = 0.6;

        public void ManageStops()
        {
            if (_strategy.Position.MarketPosition == MarketPosition.Flat) return;
            
            double atr = _strategy.ATR(14)[0];
            double buffer = atr * 1.5 * _compressionFactor * _quantumFactor;
            
            if (_strategy.Position.MarketPosition == MarketPosition.Long)
                _strategy.SetStopLoss(CalculationMode.Price, 
                    _strategy.Position.AveragePrice - buffer);
            else
                _strategy.SetStopLoss(CalculationMode.Price, 
                    _strategy.Position.AveragePrice + buffer);
        }
    }
}
