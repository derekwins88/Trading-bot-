namespace SymbolicTrading
{
    public class QuantumCollapseTrader
    {
        private readonly Execution.PositionSizer _sizer = new Execution.PositionSizer();
        private readonly Execution.TradeExecutor _executor;

        public QuantumCollapseTrader(Strategy strategy)
        {
            _executor = new Execution.TradeExecutor(strategy);
        }

        public void Trade(string symbol, double cashValue, double price)
        {
            int size = _sizer.CalculateSize(cashValue, price);
            _executor.ExecuteTrade(symbol, size);
        }
    }
}
