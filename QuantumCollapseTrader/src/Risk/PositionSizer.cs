namespace SymbolicTrading.Risk
{
    public class PositionSizer
    {
        private double _multiplier = 1.0;

        public int CalculateSize(double cash, double price) => (int)((cash / price) * _multiplier);

        public void ApplyBoostMultiplier(double factor)
        {
            _multiplier *= factor;
        }
    }
}
