using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace SymbolicTrading.ShardTools
{
    // Integration Note:
    // - ShardManager reads `.shard` files and produces validated JSON summaries for integrity
    //   and metadata export.
    // - Uses a static ShardSummarizer helper (assumed available in the same namespace)
    public static class ShardManager
    {
        public static void ExportShardCollection(string inputDir, string outputPath)
        {
            var shardSummaries = new List<object>();
            foreach (var file in Directory.GetFiles(inputDir, "*.shard"))
            {
                var summary = ShardSummarizer.SummarizeShard(file);
                shardSummaries.Add(new {
                    mnemonic = summary.Mnemonic,
                    thread_id = summary.ThreadID,
                    sha256_extracted = summary.HashExtracted,
                    sha256_computed = summary.HashComputed,
                    checksum_valid = summary.Status == "✅ Valid",
                    timestamp = summary.Timestamp,
                    summary = summary.Summary
                });
            }

            File.WriteAllText(outputPath,
                JsonSerializer.Serialize(shardSummaries, new JsonSerializerOptions { WriteIndented = true }));
        }

        public static void ExportShardToJson(string shardPath, string outputDir)
        {
            var summary = ShardSummarizer.SummarizeShard(shardPath);
            var jsonObject = new {
                mnemonic = summary.Mnemonic,
                thread_id = summary.ThreadID,
                sha256_extracted = summary.HashExtracted,
                sha256_computed = summary.HashComputed,
                checksum_valid = summary.Status == "✅ Valid",
                timestamp = summary.Timestamp,
                summary = summary.Summary
            };

            Directory.CreateDirectory(outputDir);
            File.WriteAllText(
                Path.Combine(outputDir, $"{Path.GetFileNameWithoutExtension(shardPath)}.json"),
                JsonSerializer.Serialize(jsonObject, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
