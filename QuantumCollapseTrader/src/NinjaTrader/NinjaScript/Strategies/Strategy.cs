using System;

namespace NinjaTrader.NinjaScript.Strategies
{
    public enum State { Configure, Active }

    // Simplified stub for Strategy base class used for compilation only.
    public abstract class Strategy
    {
        protected State State { get; set; }
        protected int BarsInProgress { get; set; }
        protected int CurrentBar { get; set; }
        protected double[] Close { get; set; } = new double[2];
        protected DateTime[] Time { get; set; } = new DateTime[1];

        public Account Account { get; } = new Account();

        protected virtual void OnStateChange() { }
        protected virtual void OnBarUpdate() { }

        protected double[] ATR(int period) => new double[] { 1.0 };
        protected double[] SMA(int period) => new double[] { 1.0 };
    }

    public class Account
    {
        public double Get(AccountItem item) => 10000.0;
    }

    public enum AccountItem
    {
        CashValue
    }
}
