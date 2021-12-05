using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using Freeroam_Extended.Factories;

namespace Freeroam_Extended
{
    public class Main : AsyncResource
    {
        public override void OnStart()
        {
            Alt.Server.LogColored("~g~ Freeroam-Extended Started!");
            // colshape for weapon disabling everywhere but the airport
            Alt.CreateColShapeSphere(Misc.DMPos, Misc.DMRadius);

            
            if(!File.Exists(@"BannedPlayers.json"))
            {
                var hashSet = new HashSet<Tuple<ulong,ulong>>();
                var json = JsonSerializer.Serialize(hashSet);
                File.WriteAllText(@"BannedPlayers.json", json);
            }
            else
            {
                string json = File.ReadAllText(@"BannedPlayers.json") ?? "";

                var bannedPlayers = JsonSerializer.Deserialize<HashSet<Tuple<ulong,ulong>>>(json);

                Misc.BannedPlayers = bannedPlayers ?? new HashSet<Tuple<ulong,ulong>>(); 
            }

            if(!File.Exists(@"Operators.json"))
            {
                var hashSet = new HashSet<Tuple<ulong,ulong>>();
                var json = JsonSerializer.Serialize(hashSet);
                File.WriteAllText(@"Operators.json", json);
            }
            else
            {
                string json = File.ReadAllText(@"Operators.json") ?? "";

                var operators = JsonSerializer.Deserialize<HashSet<Tuple<ulong,ulong>>>(json);

                Misc.Operators = operators ?? new HashSet<Tuple<ulong,ulong>>(); 
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
            
            var fileWriteTimer = new System.Timers.Timer();
            fileWriteTimer.Interval = 60000;
            fileWriteTimer.Enabled = true;
            fileWriteTimer.Elapsed += (sender, args) =>
            {
                Console.WriteLine("Writing to file...");
                StatsHandler.UpdateFile();
            };
        }

        public override void OnStop()
        {
            Alt.Server.LogColored("~g~ Freeroam-Extended Stopped!");
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