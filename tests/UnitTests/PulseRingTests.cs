using Xunit;
using SymbolicTrading.Memory;

namespace SymbolicTrading.Tests
{
    // Integration notes:
    // PulseRingTests validates core PulseRing behavior (recording, aggregation, and buffer limits)
    // Ensures symbolic glyph PnL history behaves as expected under real-time mutation
    public class PulseRingTests
    {
        [Fact]
        public void Record_ShouldTrackPNL_PerSymbol()
        {
            var ring = new PulseRing();
            ring.Record("☍", 5.0);
            ring.Record("☍", -3.0);
            ring.Record("🝗", 2.5);

            var stats = ring.ExportSummary() as List<dynamic>;
            var sunStats = stats.First(s => s.symbol == "☍");
            var moonStats = stats.First(s => s.symbol == "🝗");

            Assert.Equal(2, sunStats.count);
            Assert.Equal(1.0, (double)sunStats.avgPnl, 3);
            Assert.Equal(2.5, (double)moonStats.avgPnl, 3);
        }

        [Fact]
        public void Capacity_ShouldLimitHistory()
        {
            var ring = new PulseRing(2);
            ring.Record("⧖", 1.0);
            ring.Record("⧖", 2.0);
            ring.Record("⧖", 3.0);

            var stats = ring.ExportSummary() as List<dynamic>;
            Assert.Single(stats);
            Assert.Equal(2, stats[0].count);
            Assert.Equal(2.5, (double)stats[0].avgPnl, 3);
        }
    }
}
