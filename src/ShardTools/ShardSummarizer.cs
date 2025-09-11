using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace SymbolicTrading.ShardTools
{
    /// <summary>
    /// Provides utilities to summarize shard files. The summarizer pulls out
    /// mnemonic and thread information, parses the metadata section for
    /// timestamps, and performs SHA256 verification on the shard payload.
    /// </summary>
    public static class ShardSummarizer
    {
        public class ShardSummary
        {
            public string Mnemonic { get; set; }
            public string ThreadID { get; set; }
            public string HashExtracted { get; set; }
            public string HashComputed { get; set; }
            public string Status { get; set; }
            public DateTime Timestamp { get; set; }
            public string Summary { get; set; }
        }

        public static ShardSummary SummarizeShard(string shardPath)
        {
            if (!File.Exists(shardPath))
                throw new FileNotFoundException("Shard file not found", shardPath);

            var content = File.ReadAllText(shardPath);
            return SummarizeShardContent(content);
        }

        public static ShardSummary SummarizeShardContent(string shardContent)
        {
            var summary = new ShardSummary();

            // Extract sections delimited by headings such as [M] or [TID]
            var sections = ExtractSections(shardContent);

            // Mnemonic and thread ID come from [M] and [TID] headers respectively
            summary.Mnemonic = sections.TryGetValue("M", out var m) ? m.FirstOrDefault() : string.Empty;
            summary.ThreadID = sections.TryGetValue("TID", out var tid) ? tid.FirstOrDefault() : string.Empty;

            // Extract and verify the embedded hash under the [HE] section
            var hashSection = sections.ContainsKey("HE") ? sections["HE"] : Array.Empty<string>();
            summary.HashExtracted = hashSection.FirstOrDefault() ?? string.Empty;
            summary.HashComputed = ComputeShardHash(shardContent);
            summary.Status = summary.HashExtracted == summary.HashComputed ? "✅ Valid" : "⚠️ Modified";

            // Timestamp lives inside the [Metadata] section in a line like 'Timestamp: <date>'
            if (sections.TryGetValue("Metadata", out var metadata))
            {
                var timestampLine = metadata.FirstOrDefault(l => l.Contains("Timestamp:"));
                if (timestampLine != null)
                {
                    var timestampStr = timestampLine.Split(':').Last().Trim();
                    if (DateTime.TryParse(timestampStr, out var timestamp))
                    {
                        summary.Timestamp = timestamp;
                    }
                }
            }

            summary.Summary = GenerateSummary(sections);

            return summary;
            }

        private static Dictionary<string, string[]> ExtractSections(string content)
        {
            var sections = new Dictionary<string, string[]>();
            var sectionRegex = new Regex(@"^\[([A-Z]{1,3})\]\s*(.*)$", RegexOptions.Multiline);
            var currentSection = string.Empty;
            var lines = new List<string>();

            foreach (var line in content.Split('\n'))
            {
                var match = sectionRegex.Match(line);
                if (match.Success)
                {
                    if (!string.IsNullOrEmpty(currentSection))
                    {
                        sections[currentSection] = lines.ToArray();
                        lines.Clear();
                    }

                    currentSection = match.Groups[1].Value;
                    var remainder = match.Groups[2].Value.Trim();
                    if (!string.IsNullOrEmpty(remainder))
                    {
                        lines.Add(remainder);
                    }
                }
                else
                {
                    lines.Add(line.Trim());
                }
            }

            if (!string.IsNullOrEmpty(currentSection) && lines.Count > 0)
            {
                sections[currentSection] = lines.ToArray();
            }

            return sections;
        }

        private static string ComputeShardHash(string content)
        {
            var start = content.IndexOf("<<SHARD-BEGIN>>", StringComparison.Ordinal);
            var end = content.IndexOf("<<SHARD-END>>", StringComparison.Ordinal);

            if (start < 0 || end < 0 || end <= start)
                return "INVALID_FORMAT";

            var cleanContent = content.Substring(start + 15, end - start - 15).Trim();

            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(cleanContent));
                return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLowerInvariant();
            }

        }

        private static string GenerateSummary(Dictionary<string, string[]> sections)
        {
            var sb = new StringBuilder();

            if (sections.TryGetValue("M", out var mnemonicLines))
            {
                sb.AppendLine($"🔮 Mnemonic: {mnemonicLines.FirstOrDefault()}");
            }

            if (sections.TryGetValue("TID", out var tidLines))
            {
                sb.AppendLine($"🧵 Thread: {tidLines.FirstOrDefault()}");
            }

            if (sections.TryGetValue("Encrypted Payload", out var payloadLines))
            {
                var payload = string.Join(" ", payloadLines);
                sb.AppendLine($"🔐 Payload: {(payload.Length > 100 ? payload.Substring(0, 100) + "..." : payload)}");
            }

            if (sections.TryGetValue("Metadata", out var metadata))
            {
                var summaryLine = metadata.FirstOrDefault(m => m.StartsWith("Thread Summary:"));
                if (summaryLine != null)
                {
                    sb.AppendLine($"📌 Summary: {summaryLine.Substring(15).Trim()}");
                }

                var harmonicLine = metadata.FirstOrDefault(m => m.StartsWith("Harmonic Status:"));
                if (harmonicLine != null)
                {
                    sb.AppendLine($"🎵 Harmonic State: {harmonicLine.Substring(16).Trim()}");
                }
            }

            if (sections.TryGetValue("TAG", out var tags))
            {
                sb.AppendLine($"🏷️ Tags: {string.Join(", ", tags)}");
            }

            if (sections.TryGetValue("LINK", out var links))
            {
                sb.AppendLine($"🔗 Links: {string.Join(", ", links)}");
            }

            return sb.ToString();
        }
    }
}
