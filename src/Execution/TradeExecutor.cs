namespace SymbolicTrading.Execution
{
    // TradeExecutor routes glyph-based decisions to NinjaTrader entry methods
    public class TradeExecutor
    {
        private readonly Strategy _strategy;

        public TradeExecutor(Strategy strategy) => _strategy = strategy;

        public void ExecuteTrade(string symbol, int size)
        {
            switch (symbol)
            {
                case "☍":
                case "⚯":
                    _strategy.EnterLong(size, $"Long_{symbol}");
                    break;
                case "🝗":
                case "⧖":
                    _strategy.EnterShort(size, $"Short_{symbol}");
                    break;
            }
        }
    }
}
