using System;

namespace NinjaTrader.Cbi
{
    public enum AccountItem { CashValue }
    public enum MarketPosition { Long, Short, Flat }
    public class Account
    {
        public double CashValue { get; set; } = 100000;
        public double Get(AccountItem item) => CashValue;
    }
}

namespace NinjaTrader.NinjaScript
{
    public enum CalculationMode { Price }

    public class Position
    {
        public NinjaTrader.Cbi.MarketPosition MarketPosition { get; set; } = NinjaTrader.Cbi.MarketPosition.Flat;
        public double AveragePrice { get; set; }
    }

    public class Strategy
    {
        public Position Position { get; } = new Position();
        public NinjaTrader.Cbi.Account Account { get; } = new NinjaTrader.Cbi.Account();

        public double[] ATR(int period) => new double[] { 1.0 };

        public void SetStopLoss(CalculationMode mode, double price) { }
        public void ExitLong(string signalName) { }
        public void ExitShort(string signalName) { }
    }

    public enum State { SetDefaults, DataLoaded }

    public abstract class StrategyBase : Strategy
    {
        public State State { get; protected set; }
        public virtual void OnStateChange() { }
        public virtual void OnBarUpdate() { }
        public string Name { get; set; }
    }
}
