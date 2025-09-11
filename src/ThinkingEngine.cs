namespace SymbolicTrading
{
    public class ThinkingEngine
    {
        private readonly Signals.SymbolMapper _mapper = new Signals.SymbolMapper();
        private readonly Signals.MarkovChainPredictor _predictor = new Signals.MarkovChainPredictor();

        public string Process(double entropy, double drift)
        {
            _predictor.Observe(entropy, drift);
            return _mapper.Map(entropy, drift);
        }
    }
}
