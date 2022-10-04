using System.Text.Json;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using Freeroam_Extended.Factories;
using Timer = System.Timers.Timer;

namespace Freeroam_Extended
{
    public class Main : AsyncResource
    {
        public Main() : base(true)
        {
        }

        public override void OnStart()
        {
            Alt.Core.LogColored("~g~ Freeroam-Extended Started!");
            // colshape for weapon disabling everywhere but the airport
            Alt.CreateColShapeSphere(Misc.DMPos, Misc.DMRadius);


            if (!File.Exists(@"BannedPlayers.json"))
            {
                var hashSet = new HashSet<Tuple<ulong, ulong>>();
                var json = JsonSerializer.Serialize(hashSet);
                File.WriteAllText(@"BannedPlayers.json", json);
            }
            else
            {
                string json = File.ReadAllText(@"BannedPlayers.json") ?? "";

                var bannedPlayers = JsonSerializer.Deserialize<HashSet<Tuple<ulong, ulong>>>(json);

                Misc.BannedPlayers = bannedPlayers ?? new HashSet<Tuple<ulong, ulong>>();
            }

            if (!File.Exists(@"Operators.json"))
            {
                var hashSet = new HashSet<Tuple<ulong, ulong>>();
                var json = JsonSerializer.Serialize(hashSet);
                File.WriteAllText(@"Operators.json", json);
            }
            else
            {
                string json = File.ReadAllText(@"Operators.json") ?? "";

                var operators = JsonSerializer.Deserialize<HashSet<Tuple<ulong, ulong>>>(json);

                Misc.Operators = operators ?? new HashSet<Tuple<ulong, ulong>>();
            }

            if (!File.Exists("Stats.json"))
            {
                var json = JsonSerializer.Serialize(StatsHandler.StatsData);
                File.WriteAllText("Stats.json", json);
            }
            else
            {
                var stats = JsonSerializer.Deserialize<Stats>(File.ReadAllText("Stats.json"));
                if (stats != null) StatsHandler.StatsData = stats;
            }
            
            if (!File.Exists("UniquePlayers.json")) 
                File.WriteAllText("UniquePlayers.json", JsonSerializer.Serialize(Misc.UniquePlayers));
            else
            {
                var uniquePlayers = JsonSerializer.Deserialize<HashSet<Tuple<ulong, ulong>>>(File.ReadAllText("UniquePlayers.json"));
                if (uniquePlayers != null) Misc.UniquePlayers = uniquePlayers;
            }

            var fileWriteTimer = new Timer();
            fileWriteTimer.Interval = 60000;
            fileWriteTimer.Enabled = true;
            fileWriteTimer.Elapsed += (sender, args) =>
            {
                StatsHandler.UpdateFile();
                foreach (var p in Alt.GetAllPlayers())
                {
                    var player = (IAltPlayer)p;
                    player.EventCount = 0;
                }
            };
        }

        public override void OnStop()
        {
            Alt.Core.LogColored("~g~ Freeroam-Extended Stopped!");
        }

        public override IEntityFactory<IPlayer> GetPlayerFactory()
        {
            return new AltPlayerFactory();
        }

        public override IEntityFactory<IVehicle> GetVehicleFactory()
        {
            return new AltVehicleFactory();
        }
    }
}