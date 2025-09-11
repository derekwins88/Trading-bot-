using System.Collections.Generic;
using System.Linq;

namespace SymbolicTrading.Memory
{
    public class SymbolicOutcomeMatrix
    {
        private readonly int _memory;
        private readonly Dictionary<string, (decimal sum, int count)> _data = new();

        public SymbolicOutcomeMatrix(int memory)
        {
            _memory = memory;
        }

        public void Observe(IReadOnlyList<GlyphRecord> records)
        {
            if (records.Count < _memory)
                return;

            for (int i = 0; i <= records.Count - _memory; i++)
            {
                var slice = records.Skip(i).Take(_memory).ToList();
                var motif = string.Join("\u2192", slice.Select(r => r.Symbol));
                var pnlSum = slice.Sum(r => (decimal)r.Pnl);

                if (_data.TryGetValue(motif, out var entry))
                {
                    _data[motif] = (entry.sum + pnlSum, entry.count + 1);
                }
                else
                {
                    _data[motif] = (pnlSum, 1);
                }
            }
        }

        public List<MatrixEntry> ExportMatrix()
        {
            var result = new List<MatrixEntry>();
            foreach (var kvp in _data)
            {
                var avg = kvp.Value.count > 0 ? kvp.Value.sum / kvp.Value.count : 0m;
                result.Add(new MatrixEntry { motif = kvp.Key, avgPnl = avg });
            }
            return result;
        }
    }
}
