// File: src/Audit/CaretakerAuditLog.cs
// Namespace: SymbolicTrading.Audit

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SymbolicTrading.Audit
{
    /// <summary>
    /// Audit module to log key ritual and module-level events with checksum integrity.
    /// Integration Points:
    ///  - Instantiated by QuantumCollapseTrader in OnStateChange (Configure).
    ///  - LogEvent called after Axiom mutations, memory shard exports, risk circuit triggers, module lifecycle transitions.
    ///  - VerifyLogIntegrity should be invoked at session start and/or shutdown to ensure log consistency.
    ///
    ///  Interacts with:
    ///   - PulseRing: captures signals emitted from PulseRing pulses for ritual analysis.
    ///   - Strategy: log strategy-level state transitions and trade ritual outcomes.
    ///   - ThinkingEngine: records planning shifts or reasoning bursts influencing Axiom mutations.
    ///   - Capsules: persists annotations for capsule updates and memory shard movements.
    ///   - Other modules (e.g., VirtualTradeSimulator, DrawdownCircuitBreaker) invoke LogEvent when important events occur.
    /// </summary>
    public enum LogEventType
    {
        RitualEvent,
        ModuleTransition,
        MemoryShardUpdate,
        QuantumCollapse,
        AxiomMutation,
        RiskCircuitTrigger
    }

    public class CaretakerAuditLog
    {
        private readonly string _logFilePath;
        private readonly SHA256 _hasher = SHA256.Create();
        private readonly object _lock = new object();

        /// <summary>
        /// Constructor: ensure log directory exists, define log file by date.
        /// </summary>
        public CaretakerAuditLog(string logDirectory)
        {
            Directory.CreateDirectory(logDirectory);
            _logFilePath = Path.Combine(logDirectory, $"audit_{DateTime.UtcNow:yyyyMMdd}.log");
        }

        /// <summary>
        /// Primary logging API, invoked throughout the strategy’s lifecycle.
        /// Integration usage:
        ///  - AxiomEvolver: on mutation (AxiomMutation)
        ///  - MemoryCrystalCompressor / ShardManager: after memory export (MemoryShardUpdate)
        ///  - DrawdownCircuitBreaker: on risk reset (RiskCircuitTrigger)
        ///  - VirtualTradeSimulator: for ritual simulation events (RitualEvent)
        ///  - StrategyAvatar: on archetype evolution (ModuleTransition)
        ///  - PulseRing: when pulses cause strategy state changes (RitualEvent)
        ///  - ThinkingEngine: after reasoning cycles affecting trade decisions (AxiomMutation)
        ///  - CapsuleManager: on capsule load/unload or state save (MemoryShardUpdate)
        ///  Call LogEvent as soon as these events occur to keep the log consistent.
        /// </summary>
        public void LogEvent(
            LogEventType eventType,
            string description,
            string module = "",
            string referenceId = "",
            Dictionary<string, object> annotations = null)
        {
            var logEntry = new AuditLogEntry
            {
                Timestamp = DateTime.UtcNow,
                EventType = eventType.ToString(),
                Module = module,
                ReferenceId = referenceId,
                Description = description,
                Annotations = annotations ?? new Dictionary<string, object>()
            };

            logEntry.Annotations["AGI_Note"] = GenerateAgiAnnotation(eventType, description);
            logEntry.Checksum = ComputeChecksum(logEntry);

            lock (_lock)
            {
                File.AppendAllText(_logFilePath,
                    JsonSerializer.Serialize(logEntry) + Environment.NewLine);
            }
        }

        /// <summary>
        /// Invoked at strategy session boundaries to verify log integrity.
        /// Call this during startup and prior to shutdown to ensure log consistency.
        /// Returns true if all entries pass checksum validation.
        /// </summary>
        public bool VerifyLogIntegrity()
        {
            var invalidLines = new List<int>();
            int lineNumber = 0;

            lock (_lock)
            {
                if (!File.Exists(_logFilePath))
                    return true;

                foreach (var line in File.ReadLines(_logFilePath))
                {
                    lineNumber++;
                    try
                    {
                        var entry = JsonSerializer.Deserialize<AuditLogEntry>(line);
                        if (ComputeChecksum(entry) != entry.Checksum)
                        {
                            invalidLines.Add(lineNumber);
                        }
                    }
                    catch
                    {
                        invalidLines.Add(lineNumber);
                    }
                }
            }

            // Potential extension: raise warning or throw if invalidLines.Count > 0
            return invalidLines.Count == 0;
        }

        private string ComputeChecksum(AuditLogEntry entry)
        {
            var temp = new AuditLogEntry
            {
                Timestamp = entry.Timestamp,
                EventType = entry.EventType,
                Module = entry.Module,
                ReferenceId = entry.ReferenceId,
                Description = entry.Description,
                Annotations = entry.Annotations
            };
            var json = JsonSerializer.Serialize(temp);
            var hash = _hasher.ComputeHash(Encoding.UTF8.GetBytes(json));
            return BitConverter.ToString(hash).Replace("-", "");
        }

        private string GenerateAgiAnnotation(LogEventType eventType, string description)
        {
            return eventType switch
            {
                LogEventType.RitualEvent =>
                    $"Ritual energy signature detected: {description[..Math.Min(20, description.Length)]}…",
                LogEventType.ModuleTransition =>
                    $"Quantum state transition in module subspace: {description}",
                LogEventType.MemoryShardUpdate =>
                    $"Memory crystal realignment: {description}",
                LogEventType.QuantumCollapse =>
                    $"Waveform collapse event: {description}",
                LogEventType.AxiomMutation =>
                    $"Axiomatic rewrite detected: {description}",
                LogEventType.RiskCircuitTrigger =>
                    $"Risk containment protocol activated: {description}",
                _ =>
                    $"Unclassified symbolic event: {description}"
            };
        }

        public List<AuditLogEntry> SearchLogs(string query, DateTime? start = null, DateTime? end = null)
        {
            var results = new List<AuditLogEntry>();
            if (!File.Exists(_logFilePath)) return results;

            int lineNum = 0;
            lock (_lock)
            {
                foreach (var line in File.ReadLines(_logFilePath))
                {
                    lineNum++;
                    try
                    {
                        var entry = JsonSerializer.Deserialize<AuditLogEntry>(line);
                        bool inRange = (!start.HasValue || entry.Timestamp >= start)
                                       && (!end.HasValue || entry.Timestamp <= end);

                        if (inRange &&
                            (entry.Description.Contains(query, StringComparison.OrdinalIgnoreCase)
                             || entry.Module.Contains(query, StringComparison.OrdinalIgnoreCase)
                             || entry.EventType.Contains(query, StringComparison.OrdinalIgnoreCase)
                             || ContainsAnnotation(entry, query)))
                        {
                            results.Add(entry);
                        }
                    }
                    catch { /* skip corrupted */ }
                }
            }

            return results;
        }

        private bool ContainsAnnotation(AuditLogEntry entry, string query)
        {
            foreach (var kvp in entry.Annotations)
            {
                if (kvp.Value?.ToString()?.Contains(query, StringComparison.OrdinalIgnoreCase) == true)
                    return true;
            }
            return false;
        }

        private class AuditLogEntry
        {
            public DateTime Timestamp { get; set; }
            public string EventType { get; set; }
            public string Module { get; set; }
            public string ReferenceId { get; set; }
            public string Description { get; set; }
            public Dictionary<string, object> Annotations { get; set; }
            public string Checksum { get; set; }
        }
    }
}
