using Xunit;
using SymbolicTrading.Memory;
using System.Collections.Generic;

namespace SymbolicTrading.Tests
{
    /// <summary>
    /// SymbolicOutcomeMatrixTests ensures motif detection and aggregation behaves correctly
    /// Validates motif string formatting, PnL summation, and statistical memory output
    /// </summary>
    public class SymbolicOutcomeMatrixTests
    {
        [Fact]
        public void Observe_ShouldBuildMotifMatrix()
        {
            var matrix = new SymbolicOutcomeMatrix(3);
            var records = new List<GlyphRecord> {
                new() { Symbol = "☍", Pnl = 1.0 },
                new() { Symbol = "🝗", Pnl = 2.0 },
                new() { Symbol = "⚯", Pnl = 3.0 }
            };

            matrix.Observe(records);
            var grid = matrix.ExportMatrix();

            Assert.Single(grid);
            Assert.Equal("☍→🝗→⚯", grid[0].motif);
            Assert.Equal(6.0, (double)grid[0].avgPnl, 3);
        }

        [Fact]
        public void MultipleObservations_ShouldAggregate()
        {
            var matrix = new SymbolicOutcomeMatrix(2);
            var records = new List<GlyphRecord> {
                new() { Symbol = "☍", Pnl = 1.0 },
                new() { Symbol = "🝗", Pnl = 2.0 },
                new() { Symbol = "☍", Pnl = -1.0 },
                new() { Symbol = "🝗", Pnl = -2.0 }
            };

            matrix.Observe(records);
            var grid = matrix.ExportMatrix();

            Assert.Equal(2, grid.Count);
            Assert.Equal("☍→🝗", grid[0].motif);
            Assert.Equal(0.0, (double)grid[0].avgPnl, 3);
        }
    }
}
