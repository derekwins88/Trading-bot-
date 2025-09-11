namespace SymbolicTrading.Signals
{
    // MarkovChainPredictor observes market states and builds a transition matrix for prediction
    public class MarkovChainPredictor
    {
        public enum MarketState
        {
            LowVol_UpTrend, LowVol_DownTrend,
            HighVol_UpTrend, HighVol_DownTrend,
            Neutral
        }
        
        private MarketState? _lastState;
        private readonly double[,] _transitionCounts = new double[5,5];

        public void Observe(double entropy, double drift)
        {
            var current = Discretize(entropy, drift);
            if (_lastState.HasValue)
                _transitionCounts[(int)_lastState, (int)current]++;
            _lastState = current;
        }

        private MarketState Discretize(double entropy, double drift)
        {
            bool highVol = entropy > 0.7;
            if (!highVol && drift > 0)
                return MarketState.LowVol_UpTrend;
            if (!highVol && drift <= 0)
                return MarketState.LowVol_DownTrend;
            if (highVol && drift > 0)
                return MarketState.HighVol_UpTrend;
            if (highVol && drift <= 0)
                return MarketState.HighVol_DownTrend;
            return MarketState.Neutral;
        }
    }
}
