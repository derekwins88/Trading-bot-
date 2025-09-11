using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace SymbolicTrading.ShardTools
{
    /// <summary>
    /// Provides helper methods for operating on a collection of shards. It wraps
    /// <see cref="ShardSummarizer"/> to produce exportable summary files and can
    /// inject annotations directly into shard files when needed.
    /// </summary>
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
                JsonSerializer.Serialize(shardSummaries, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Converters = { new ShardSummaryConverter() }
                }));
        }

        public static void AnnotateShard(string shardPath, string annotation)
        {
            var content = File.ReadAllText(shardPath);
            var newContent = content.Replace("<<SHARD-BEGIN>>", $"// @memo: {annotation}\n<<SHARD-BEGIN>>");
            File.WriteAllText(shardPath, newContent);
        }
    }

    // Custom converter for proper JSON serialization
    public class ShardSummaryConverter : System.Text.Json.Serialization.JsonConverter<object>
    {
        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            if (value is IReadOnlyDictionary<string, object> dict)
            {
                writer.WriteStartObject();
                foreach (var kvp in dict)
                {
                    writer.WritePropertyName(kvp.Key);
                    JsonSerializer.Serialize(writer, kvp.Value, options);
                }
                writer.WriteEndObject();
            }
            else
            {
                JsonSerializer.Serialize(writer, value, value.GetType(), options);
            }
        }
    }
}
