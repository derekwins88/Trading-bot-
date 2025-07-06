using NinjaTrader.NinjaScript.Strategies;

namespace SymbolicTrading.Risk
{
    public class DrawdownCircuitBreaker
    {
        private readonly Strategy _strategy;

        public DrawdownCircuitBreaker(Strategy strategy)
        {
            _strategy = strategy;
        }

        public void CheckDrawdown()
        {
            // Stub: no drawdown logic
        }
    }
}
