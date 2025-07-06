namespace SymbolicTrading.Signals
{
    public class SymbolMapper
    {
        public string Map(double entropy, double drift)
        {
            // Simple mapping based on entropy threshold
            return entropy > 0.5 ? "HIGH" : "LOW";
        }
    }
}
