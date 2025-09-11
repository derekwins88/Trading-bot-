// StrategyAvatar.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using SymbolicTrading.Memory;
using SymbolicTrading.Signals;

namespace SymbolicTrading.Profiles
{
    /// <summary>
    /// Represents adaptive behaviour traits for a trading thread.
    /// The avatar ingests trade results from <see cref="PulseRing"/> and
    /// outcome statistics from <see cref="SymbolicOutcomeMatrix"/>. These
    /// preferences feed back into the strategy by adjusting motif boosts or
    /// informing the <c>PositionSizer</c> before new orders are placed.
    /// </summary>
    /// <remarks>
    /// <para><see cref="UpdateProfile"/> is typically called at the end of a
    /// trading cycle inside the engine core. <see cref="ExportToShard"/> runs
    /// during state checkpointing so the avatar can be reconstructed across
    /// sessions.</para>
    /// </remarks>
    public class StrategyAvatar
    {
        public string ThreadId { get; init; }
        public string Archetype { get; private set; }
        public double Aggressiveness { get; private set; } = 1.0;
        public double RiskTolerance { get; private set; } = 0.5;
        public Dictionary<string, double> MotifPreferences { get; } = new();
        public DateTime LastUpdated { get; private set; }
        
        [JsonIgnore]
        public Action<StrategyAvatar> OnAvatarUpdated;

        public StrategyAvatar(string threadId, string archetype = "QUANTUM_WANDERER")
        {
            ThreadId = threadId;
            Archetype = archetype;
            LastUpdated = DateTime.UtcNow;
            InitializeArchetype(archetype);
        }

        /// <summary>
        /// Refreshes the avatar metrics using trade performance from
        /// <paramref name="pulseRing"/> and motif outcomes from
        /// <paramref name="outcomeMatrix"/>.
        /// </summary>
        public void UpdateProfile(
            SymbolicOutcomeMatrix outcomeMatrix,
            PulseRing pulseRing,
            double performanceThreshold = 0.65)
        {
            // Update based on recent performance
            var recentPerformance = pulseRing.GetRecentGlyphs(50);
            if (recentPerformance.Count > 0)
            {
                double avgPnl = recentPerformance.Average(g => g.Pnl);
                double winRate = (double)recentPerformance.Count(g => g.Pnl > 0) / recentPerformance.Count;
                
                UpdateAggressiveness(winRate, avgPnl);
                UpdateRiskTolerance(winRate);
            }

            // Update motif preferences
            UpdateMotifPreferences(outcomeMatrix, performanceThreshold);
            
            // Evolve archetype if needed
            if (DateTime.UtcNow - LastUpdated > TimeSpan.FromDays(7))
            {
                EvolveArchetype();
            }

            LastUpdated = DateTime.UtcNow;
            OnAvatarUpdated?.Invoke(this);
        }

        public double ApplyMotifPreference(string motif)
        {
            return MotifPreferences.TryGetValue(motif, out double preference) 
                ? preference 
                : 1.0;
        }

        private void UpdateAggressiveness(double winRate, double avgPnl)
        {
            // Scale between 0.5-2.0 based on performance
            double newAggression = 1.0 + (winRate - 0.5) + (avgPnl * 0.1);
            Aggressiveness = Math.Clamp(newAggression, 0.5, 2.0);
        }

        private void UpdateRiskTolerance(double winRate)
        {
            // Scale between 0.1-0.8 based on win rate
            RiskTolerance = winRate switch
            {
                > 0.7 => 0.8 - ((winRate - 0.7) * 0.5),
                < 0.4 => 0.4 + ((0.4 - winRate) * 0.5),
                _ => 0.5
            };
        }

        private void UpdateMotifPreferences(SymbolicOutcomeMatrix matrix, double threshold)
        {
            var matrixData = matrix.ExportMatrix();
            foreach (var entry in matrixData)
            {
                if (entry.successRate >= threshold && entry.count > 5)
                {
                    double preference = 1.0 + (entry.avgPnl * 0.2);
                    MotifPreferences[entry.motif] = Math.Clamp(preference, 0.8, 1.5);
                }
            }
        }

        private void EvolveArchetype()
        {
            Archetype = Aggressiveness switch
            {
                > 1.5 => RiskTolerance > 0.6 ? "QUANTUM_RAIDER" : "SIGIL_ASSAULT",
                < 0.7 => "ECHO_CONSERVER",
                _ => RiskTolerance > 0.6 ? "VOLATILITY_HARMONIZER" : "AXIOM_GUARDIAN"
            };
        }

        private void InitializeArchetype(string archetype)
        {
            switch (archetype)
            {
                case "QUANTUM_RAIDER":
                    Aggressiveness = 1.8;
                    RiskTolerance = 0.7;
                    break;
                case "SIGIL_ASSAULT":
                    Aggressiveness = 1.6;
                    RiskTolerance = 0.4;
                    break;
                case "ECHO_CONSERVER":
                    Aggressiveness = 0.7;
                    RiskTolerance = 0.3;
                    break;
                case "VOLATILITY_HARMONIZER":
                    Aggressiveness = 1.2;
                    RiskTolerance = 0.6;
                    break;
                case "AXIOM_GUARDIAN":
                    Aggressiveness = 1.0;
                    RiskTolerance = 0.4;
                    break;
            }
        }

        /// <summary>
        /// Creates a textual shard snapshot for persistence. Called when the
        /// engine checkpoints profile data for distribution across shards.
        /// </summary>
        public string ExportToShard()
        {
            var shard = new
            {
                Mnemonic = $"Avatar of {Archetype}",
                ThreadId,
                Archetype,
                Timestamp = DateTime.UtcNow,
                Aggressiveness,
                RiskTolerance,
                MotifPreferences,
                HarmonicStatus = $"ΔΦ = {RiskTolerance - 0.2:0.000}"
            };

            return JsonSerializer.Serialize(shard, new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new AvatarShardConverter() }
            });
        }
    }

    public class AvatarShardConverter : JsonConverter<StrategyAvatar>
    {
        public override StrategyAvatar Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Shard deserialization logic
            throw new NotImplementedException("Avatar shard loading not implemented");
        }

        public override void Write(Utf8JsonWriter writer, StrategyAvatar value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("ThreadID", value.ThreadId);
            writer.WriteString("Archetype", value.Archetype);
            writer.WriteNumber("Aggressiveness", value.Aggressiveness);
            writer.WriteNumber("RiskTolerance", value.RiskTolerance);
            writer.WritePropertyName("MotifPreferences");
            JsonSerializer.Serialize(writer, value.MotifPreferences, options);
            writer.WriteEndObject();
        }
    }
}
