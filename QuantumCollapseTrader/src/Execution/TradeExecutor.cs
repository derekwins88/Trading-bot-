using NinjaTrader.NinjaScript.Strategies;

namespace SymbolicTrading.Execution
{
    public class TradeExecutor
    {
        private readonly Strategy _strategy;

        public TradeExecutor(Strategy strategy)
        {
            _strategy = strategy;
        }

        public void ExecuteTrade(string symbol, int size)
        {
            // Stub: trade execution logic would go here
        }
    }
}
