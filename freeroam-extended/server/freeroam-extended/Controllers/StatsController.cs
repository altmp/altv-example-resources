using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AltV.Net;

namespace Freeroam_Extended
{
    public static class StatsController
    {
        private static readonly string UniquePlayersFile = "UniquePlayers.json";
        private static readonly string StatsFile = "Stats.json";
        
        public static HashSet<string> UniquePlayers = new();
        public static Stats StatsData = new ();

        public static void Init()
        {
            if (File.Exists(UniquePlayersFile))
            {
                var json = File.ReadAllText(UniquePlayersFile);
                try
                {
                    UniquePlayers = JsonSerializer.Deserialize<HashSet<string>>(json) ?? new();
                }
                catch (Exception e)
                {
                    Alt.LogError("Failed to parse unique players file!");
                    Alt.LogError(e.ToString());
                }
            }
            
            if (File.Exists(StatsFile))
            {
                var json = File.ReadAllText(StatsFile);
                try
                {
                    StatsData = JsonSerializer.Deserialize<Stats>(json) ?? new();
                }
                catch (Exception e)
                {
                    Alt.LogError("Failed to parse stats file!");
                    Alt.LogError(e.ToString());
                }
            }
        }
        
        public static void UpdateStats()
        {
            File.WriteAllText(StatsFile, JsonSerializer.Serialize(StatsData));
        }
        
        public static void UpdateUniquePlayers()
        {
            File.WriteAllText(UniquePlayersFile, JsonSerializer.Serialize(UniquePlayers));
        }
        
        public static void AddUniquePlayer(string cloudId)
        {
            if (!UniquePlayers.Contains(cloudId))
            {
                UniquePlayers.Add(cloudId);
                lock (StatsData)
                {
                    StatsData.UniquePlayers++;
                }
                UpdateUniquePlayers();
            }
        }
    }
    
    public class Stats
    {
        public int VehiclesSpawned { get; set; }
        public int VehiclesDestroyed { get; set; }
        public int PlayerDeaths { get; set; }
        public int PlayerConnections { get; set; }
        public int UniquePlayers { get; set; }
    }
}