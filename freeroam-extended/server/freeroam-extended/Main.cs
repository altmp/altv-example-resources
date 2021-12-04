using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;

using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
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

            JsonSerializerOptions options = new()
            {
                ReferenceHandler = ReferenceHandler.Preserve,
                WriteIndented = true
            };

            if(!File.Exists(@"BannedPlayers.json"))
            {
                var hashSet = new HashSet<Tuple<ulong,ulong>>();
                var json = JsonSerializer.Serialize(hashSet);
                File.WriteAllText(@"BannedPlayers.json", json);
            }
            else
            {
                string json = File.ReadAllText(@"BannedPlayers.json");
                if(json == null)
                    json = "";

                var bannedPlayers = JsonSerializer.Deserialize<HashSet<Tuple<ulong,ulong>>>(json);

                if(bannedPlayers == null)
                    Misc.BannedPlayers = new HashSet<Tuple<ulong,ulong>>();
                else
                    Misc.BannedPlayers = bannedPlayers; 
            }

            if(!File.Exists(@"Operators.json"))
            {
                var hashSet = new HashSet<Tuple<ulong,ulong>>();
                var json = JsonSerializer.Serialize(hashSet);
                File.WriteAllText(@"Operators.json", json);
            }
            else
            {
                string json = File.ReadAllText(@"Operators.json");
                if(json == null)
                    json = "";
                    
                var operators = JsonSerializer.Deserialize<HashSet<Tuple<ulong,ulong>>>(json);

                if(operators == null)
                    Misc.Operators = new HashSet<Tuple<ulong,ulong>>();
                else
                    Misc.Operators = operators; 
            }
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