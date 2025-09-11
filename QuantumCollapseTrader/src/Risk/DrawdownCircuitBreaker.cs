// DrawdownCircuitBreaker monitors equity and halts positions if losses exceed 20%
using System;
using NinjaTrader.NinjaScript;
using NinjaTrader.Cbi;

namespace SymbolicTrading.Risk
{
    public class DrawdownCircuitBreaker
    {
        private readonly Strategy _strategy;
        private double _equityPeak;
        private const double Threshold = 0.2;

        public DrawdownCircuitBreaker(Strategy strategy)
        {
            _strategy = strategy;
            _equityPeak = strategy.Account.Get(AccountItem.CashValue);
        }

        public void CheckDrawdown()
        {
            double current = _strategy.Account.Get(AccountItem.CashValue);
            _equityPeak = Math.Max(_equityPeak, current);
            
            if ((_equityPeak - current) / _equityPeak >= Threshold)
            {
                _strategy.ExitLong("DDReset");
                _strategy.ExitShort("DDReset");
                _equityPeak = current;
            }
        }
    }
}
