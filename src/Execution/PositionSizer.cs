using System;

namespace SymbolicTrading.Execution
{
    // PositionSizer handles position scaling logic based on account value and axiom signals
    public class PositionSizer
    {
        private double _positionBoost = 1.0;

        public void ApplyBoostMultiplier(double multiplier)
        {
            double boosted = _positionBoost * multiplier;
            if (boosted < 1.0) boosted = 1.0;
            if (boosted > 10.0) boosted = 10.0;
            _positionBoost = boosted;
        }

        public int CalculateSize(double cashValue, double price)
        {
            return (int)Math.Max(1, Math.Round(cashValue * 0.01 * _positionBoost / price));
        }
    }
}
