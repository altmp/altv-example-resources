using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Freeroam_Extended
{
    public static class StatsHandler
    {
        public static Stats StatsData = new ();
        
        public static Task UpdateFile()
        {
            File.WriteAllText("Stats.json", JsonSerializer.Serialize(StatsData));
            return Task.CompletedTask;
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