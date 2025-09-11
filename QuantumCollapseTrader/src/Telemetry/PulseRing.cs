using System.Collections.Generic;
using System.Linq;

namespace SymbolicTrading.Telemetry
{
    public class PulseRing
    {
        private readonly List<Glyph> _glyphs = new();

        public void AddGlyph(string symbol)
        {
            _glyphs.Add(new Glyph { Symbol = symbol });
        }

        public IEnumerable<Glyph> GetRecentGlyphs()
        {
            return _glyphs;
        }
    }

    public class Glyph
    {
        public string Symbol { get; set; } = string.Empty;
    }
}
