namespace SymbolicTrading.Signals
{
    // SymbolMapper converts entropy and drift into symbolic glyphs (☍, ⚯, 🝗, etc.)
    public class SymbolMapper
    {
        public string Map(double entropy, double drift)
        {
            if (entropy > 0.8 && drift < -0.2) return "☍";
            if (entropy > 0.8 && drift > 0.2) return "⚯";
            if (entropy < 0.3 && drift > 0.3) return "🝗";
            if (entropy > 0.9 && drift > 0.05) return "⧖";
            return "◇";
        }
    }
}
