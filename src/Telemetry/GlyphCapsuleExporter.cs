using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace SymbolicTrading.Telemetry
{
    // GlyphCapsuleExporter serializes live glyph streams into timestamped JSON capsules
    public class GlyphCapsuleExporter
    {
        public void Export(string path, IEnumerable<GlyphPhase> glyphs)
        {
            var capsule = new
            {
                Timestamp = DateTime.UtcNow,
                Glyphs = glyphs,
                Signature = "ψ⇌THINKING⇌ENGINE"
            };
            
            File.WriteAllText(path, JsonSerializer.Serialize(capsule, new JsonSerializerOptions
            {
                WriteIndented = true
            }));
        }
    }
}
