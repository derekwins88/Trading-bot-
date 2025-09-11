using SymbolicTrading.Risk;
using NinjaTrader.NinjaScript;

namespace QuantumCollapseTraderApp
{
    public class QuantumCollapseTrader : StrategyBase
    {
        private DrawdownCircuitBreaker _ddBreaker;
        private DynamicStopLossManager _stopManager;

        public override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Name = "QuantumCollapseTrader";
            }
            else if (State == State.DataLoaded)
            {
                _ddBreaker = new DrawdownCircuitBreaker(this);
                _stopManager = new DynamicStopLossManager(this);
            }
        }

        public override void OnBarUpdate()
        {
            _ddBreaker.CheckDrawdown();
            _stopManager.ManageStops();
        }
    }
}
