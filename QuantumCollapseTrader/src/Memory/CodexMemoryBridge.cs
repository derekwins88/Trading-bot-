using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using SymbolicTrading.Storage;
using SymbolicTrading.Telemetry;

namespace SymbolicTrading.Memory
{
    /// <summary>
    /// Bridge between memory capsules and higher-level engines.
    ///
    /// <para>This component is typically instantiated during the boot
    /// sequence of <c>ThinkingEngine</c> as it prepares the runtime. It is
    /// later injected into <c>StrategyAvatar</c> instances so that trading
    /// strategies can retrieve and resolve context via <c>RetrieveContext()</c>
    /// or <c>GenerateContinuityPrompt()</c>. <c>PulseRing</c> uses
    /// <c>ResolveContext()</c> to persist new capsules after market cycles, and
    /// <c>ShardManager</c> calls <c>GetDormantLinks()</c> when pruning state.</para>
    ///
    /// <para>The bridge communicates with the rest of the system purely via the
    /// provided interfaces: <see cref="ICapsuleRepository"/> for storage and
    /// <see cref="IEchoFractalTracker"/> for telemetry. Typical lifecycle is:
    /// instantiated once, mutated whenever <c>LinkContext()</c> or
    /// <c>ResolveContext()</c> is invoked, serialized by the hosting engine as
    /// part of its state snapshot, and reset when the engine performs a clean
    /// restart.</para>
    /// </summary>
    public class CodexMemoryBridge
    {
        private readonly ICapsuleRepository _capsuleRepository;
        private readonly IEchoFractalTracker _fractalTracker;
        private readonly Dictionary<string, ContextualLink> _contextMap = new();

        public CodexMemoryBridge(ICapsuleRepository capsuleRepository, IEchoFractalTracker fractalTracker)
        {
            _capsuleRepository = capsuleRepository;
            _fractalTracker = fractalTracker;
        }

        /// <summary>
        /// Associates a symbolic anchor with a context identifier. Used by
        /// StrategyAvatar when creating or advancing positions.
        /// </summary>
        public void LinkContext(string contextId, string capsuleId, string symbolicAnchor)
        {
            _contextMap[contextId] = new ContextualLink
            {
                CapsuleId = capsuleId,
                SymbolicAnchor = symbolicAnchor,
                LastAccessed = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Fetches the raw capsule content. This is typically invoked during
        /// <c>RetrieveContext()</c> calls within ThinkingEngine as it composes
        /// prompts.
        /// </summary>
        public string RetrieveContext(string contextId)
        {
            if (!_contextMap.TryGetValue(contextId, out var link)) 
                return null;

            // Update access tracking
            link.LastAccessed = DateTime.UtcNow;
            _fractalTracker.RecordAccess(contextId, link.CapsuleId);

            // Retrieve capsule content
            return _capsuleRepository.GetCapsuleContent(link.CapsuleId);
        }

        /// <summary>
        /// Generates a continuity prompt for ThinkingEngine so that command
        /// generation maintains symbolic awareness of prior anchors.
        /// </summary>
        public string GenerateContinuityPrompt(string input, string contextId)
        {
            if (!_contextMap.TryGetValue(contextId, out var link)) 
                return input;

            var capsule = _capsuleRepository.GetCapsule(link.CapsuleId);
            var anchorData = capsule?.GetAnchorData(link.SymbolicAnchor);
            
            if (anchorData == null) 
                return input;

            return $"""
                [Continuity Anchor: {link.SymbolicAnchor}]
                {anchorData}
                
                [Current Input]
                {input}
                
                [Instruction]
                Maintain symbolic continuity with the anchor context
                """;
        }

        /// <summary>
        /// Resolves context by either appending to an existing capsule or
        /// creating a new one. <c>PulseRing</c> typically calls this after
        /// <c>AdvanceMarket()</c> cycles to persist AI responses.
        /// </summary>
        public void ResolveContext(string contextId, string response, string capsuleId)
        {
            if (!_contextMap.TryGetValue(contextId, out var link))
                return;

            // Create new capsule if different from original
            if (link.CapsuleId != capsuleId)
            {
                _capsuleRepository.StoreResponseAsCapsule(capsuleId, response, link.SymbolicAnchor);
                _fractalTracker.RecordResolution(contextId, capsuleId);
                
                // Update link to new capsule
                link.CapsuleId = capsuleId;
            }
            else
            {
                // Append to existing capsule
                _capsuleRepository.AppendToCapsule(capsuleId, response, link.SymbolicAnchor);
            }
        }

        /// <summary>
        /// Returns context links that have not been accessed within a given
        /// threshold. <c>ShardManager</c> queries this when deciding which
        /// contexts to archive or remove during house keeping.
        /// </summary>
        public IEnumerable<ContextualLink> GetDormantLinks(TimeSpan threshold)
        {
            var cutoff = DateTime.UtcNow - threshold;
            return _contextMap.Values.Where(l => l.LastAccessed < cutoff);
        }

        private class ContextualLink
        {
            public string CapsuleId { get; set; }
            public string SymbolicAnchor { get; set; }
            public DateTime LastAccessed { get; set; }
        }
    }

    public interface ICapsuleRepository
    {
        string GetCapsuleContent(string id);
        MemoryCapsule GetCapsule(string id);
        void StoreResponseAsCapsule(string id, string response, string anchor);
        void AppendToCapsule(string id, string content, string anchor);
    }

    /// <summary>
    /// Serializable container for anchor data. Typically persisted by
    /// <c>StrategyAvatar</c> and accessed by <c>ThinkingEngine</c> during
    /// <c>InjectChaos()</c> or other heuristic routines.
    /// </summary>
    public class MemoryCapsule
    {
        public string Id { get; set; }
        public Dictionary<string, string> Anchors { get; } = new();
        public DateTime Created { get; } = DateTime.UtcNow;
        
        public string GetAnchorData(string anchor)
        {
            return Anchors.TryGetValue(anchor, out var data) ? data : null;
        }

        public void AddAnchorData(string anchor, string content)
        {
            Anchors[anchor] = content;
        }
    }
}
