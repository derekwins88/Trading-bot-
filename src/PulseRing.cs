using System.Collections.Generic;
using System.Linq;

namespace SymbolicTrading
{
    public class Glyph
    {
        public string Symbol { get; set; }
    }

    public class PulseRing
    {
        private readonly List<Glyph> glyphs = new List<Glyph>();

        public void AddGlyph(string symbol)
        {
            glyphs.Add(new Glyph { Symbol = symbol });
        }

        public IEnumerable<Glyph> GetRecentGlyphs(int count)
        {
            return glyphs.Skip(glyphs.Count - count < 0 ? 0 : glyphs.Count - count).Take(count);
        }
    }
}
