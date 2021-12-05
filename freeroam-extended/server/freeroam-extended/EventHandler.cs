using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using AltV.Net.Resources.Chat.Api;
using Freeroam_Extended.Factories;
using ExplosionType = AltV.Net.Data.ExplosionType;

namespace Freeroam_Extended
{
    public class EventHandler : IScript
    {
        private readonly Random _random = new Random();

        [ScriptEvent(ScriptEventType.PlayerConnect)]
        public Task OnPlayerConnect(IAltPlayer player, string reason)
        {
            // create async context
            if (Misc.BannedPlayers.Any(tuple => tuple.Item1 == player.HardwareIdHash && tuple.Item2 == player.HardwareIdExHash))
            {
                player.Kick("You're banned from this server!");
                AltAsync.Log($"HWID: {player.HardwareIdHash}, SC: {player.SocialClubId}. Tried to join the server with a ban.");
                return Task.CompletedTask;
            }
            
            if (Misc.Operators.Contains(new Tuple<ulong, ulong>(player.HardwareIdHash, player.HardwareIdExHash)))
                player.IsAdmin = true;
            
            // select random entry from SpawnPoints
            var randomSpawnPoint = Misc.SpawnPositions.ElementAt(_random.Next(0, Misc.SpawnPositions.Length));
            player.Spawn(randomSpawnPoint + new Position(_random.Next(0, 10), _random.Next(0, 10), 0));
            player.Model = (uint) PedModel.FreemodeMale01;
            player.SetDateTime(1, 1, 1, Misc.Hour, 1, 1);
            player.SetWeather(Misc.Weather);

            player.Emit("draw_dmzone", Misc.DMPos.X, Misc.DMPos.Y, Misc.DMRadius, 150);

            if(player.IsAdmin)
                player.Emit("set_chat_state", true);

            lock (StatsHandler.StatsData)
            {
                StatsHandler.StatsData.PlayerConnections++;
                StatsHandler.UpdateFile();
            }

            return Task.CompletedTask;
        }

        [AsyncScriptEvent(ScriptEventType.VehicleDestroy)]
        public async Task OnVehicleDestroy(IAltVehicle target)
        {
            lock (StatsHandler.StatsData)
            {
                StatsHandler.StatsData.VehiclesDestroyed++;
#pragma warning disable 4014
                StatsHandler.UpdateFile();
#pragma warning restore 4014
            }
            await Task.Delay(5000);
            
            await using (var asyncContext = AsyncContext.Create())
            {
                if (!target.TryToAsync(asyncContext, out var asyncVehicle)) return;
                if (!target.Owner.TryToAsync(asyncContext, out var asyncOwner)) return;
                asyncOwner.SendChatMessage("Your Vehicle got destroyed. We removed it for you!");
                asyncVehicle.Remove();
            }
        }

        [AsyncScriptEvent(ScriptEventType.PlayerDisconnect)]
        public Task OnPlayerDisconnect(IAltPlayer player, string reason)
        {
            var vehicles = Alt.GetAllVehicles().Cast<IAltVehicle>().Where(x => x.Owner == player);
           
            foreach (var veh in vehicles)
            {
                if (veh.Owner.Id != player.Id) continue;
                veh.Remove();
            }
            
            return Task.CompletedTask;
        }

        [AsyncScriptEvent(ScriptEventType.PlayerDead)]
        public Task OnPlayerDead(IAltPlayer player, IEntity killer, uint weapon)
        {
            var spawnPointPool = player.DmMode ? Misc.AirportSpawnPositions : Misc.SpawnPositions;
            
            var randomSpawnPoint = spawnPointPool.ElementAt(_random.Next(0, spawnPointPool.Length));
            player.Spawn(randomSpawnPoint + new Position(_random.Next(0, 10), _random.Next(0, 10), 0));

            lock (StatsHandler.StatsData)
            {
                StatsHandler.StatsData.PlayerDeaths++;
                StatsHandler.UpdateFile();
            }

            if (killer is not IAltPlayer killerPlayer)
                return Task.CompletedTask;

            
            if (Misc.BlacklistedWeapons.Contains(weapon))
            {
                Alt.Server.LogColored($"~r~ Banned Player: {killerPlayer.Name} ({killerPlayer.Id}) for using illegal weapon!");
                Misc.BannedPlayers.Add(new Tuple<ulong,ulong>(killerPlayer.HardwareIdHash, killerPlayer.HardwareIdExHash));
                string json = JsonSerializer.Serialize(Misc.BannedPlayers);
                File.WriteAllText(@"BannedPlayers.json", json);
                killerPlayer.Kick("You're banned from this server!");

                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }

        [ScriptEvent(ScriptEventType.ConsoleCommand)]
        public Task OnConsoleCommand(string name, string[] args)
        {
            var playerPool = Alt.GetAllPlayers();
            switch (name)
            {
                case "op":
                    if (args.Length is > 1 or 0) 
                    {
                        Alt.Log("Usage: op <ID>");
                        break;
                    }

                    var playerOp = playerPool.FirstOrDefault(x => x.Id == int.Parse(args[0]));
                    if (playerOp is not IAltPlayer playerOpAlt) return Task.CompletedTask;
                    
                    if (Misc.Operators.Any(tuple => tuple.Item1 == playerOpAlt.HardwareIdHash && tuple.Item2 == playerOpAlt.HardwareIdExHash))
                    {
                        Alt.Log($"Id {args[0]} already is an operator!");   
                        break;
                    }
                    Misc.Operators.Add(new Tuple<ulong,ulong>(playerOpAlt.HardwareIdHash, playerOpAlt.HardwareIdExHash));
                    string json = JsonSerializer.Serialize(Misc.Operators);
                    File.WriteAllText(@"Operators.json", json);

                    playerOpAlt.Emit("set_chat_state", true);
                    playerOpAlt.IsAdmin = true;
                    break;
                
                case "deop":
                    if (args.Length is > 1 or 0) 
                    {
                        Alt.Log("Usage: deop <ID>");
                        break;
                    }
                    var playerDeOp = playerPool.FirstOrDefault(x => x.Id == int.Parse(args[0]));
                    if (playerDeOp is not IAltPlayer playerDeOpAlt) return Task.CompletedTask;
                    
                    if (!Misc.Operators.Any(tuple => tuple.Item1 == playerDeOpAlt.HardwareIdHash && tuple.Item2 == playerDeOpAlt.HardwareIdExHash))
                    {
                        Alt.Log($"Id {args[0]} is not an operator!");
                        break;
                    }
                    Misc.Operators.Remove(new Tuple<ulong,ulong>(playerDeOpAlt.HardwareIdHash, playerDeOpAlt.HardwareIdExHash));
                    playerDeOpAlt.Emit("set_chat_state", Misc.ChatState);
                    playerDeOpAlt.IsAdmin = false;
                    break;
            }
            return Task.CompletedTask;
        }

        [AsyncScriptEvent(ScriptEventType.WeaponDamage)]
        public Task OnWeaponDamage(IAltPlayer player, IEntity target, uint weapon, ushort damage,
            Position shotOffset, BodyPart bodyPart)
        {
            if (!Misc.BlacklistedWeapons.Contains(weapon) || player is not { } damagePlayer) return Task.CompletedTask;
            
            Alt.Server.LogColored($"~r~ Banned Player: {damagePlayer.Name} ({damagePlayer.Id}) for using illegal weapon!");
            //Misc.BannedPlayers.Add(<ulong, ulong>(damagePlayer.HardwareIdHash, damagePlayer.HardwareIdExHash));
            Misc.BannedPlayers.Add(new Tuple<ulong,ulong>(damagePlayer.HardwareIdHash, damagePlayer.HardwareIdExHash));
            string json = JsonSerializer.Serialize(Misc.BannedPlayers);
            File.WriteAllText(@"BannedPlayers.json", json);

            damagePlayer.Kick("You're banned from this server!");

            return Task.CompletedTask;
        }

        [AsyncScriptEvent(ScriptEventType.ColShape)]
        public Task OnColshapeEnter(IColShape colshape, IEntity target, bool state)
        {
            if (target is not IAltPlayer targetPlayer) return Task.CompletedTask;

            // entity to async
            targetPlayer.EnableWeaponUsage = state;
            targetPlayer.Emit("airport_state", state);
            
            return Task.CompletedTask;
        }

        [ScriptEvent(ScriptEventType.Fire)]
        public bool OnFireStart(IAltPlayer player, FireInfo[] fireInfos)
        {
            return false;
        }

        [ScriptEvent(ScriptEventType.Explosion)]
        public bool OnExplosion(IAltPlayer player, ExplosionType explosionType, Position position, uint explosionFx,
            IEntity target)
        {
            return false;
        }

        [ScriptEvent(ScriptEventType.StartProjectile)]
        public bool OnProjectileStart(IAltPlayer player, Position startPosition, Position direction, uint ammoHash, uint weaponHash)
        {
            return false;
        }

        [ClientEvent("chat:message")]
        public Task OnChatMessage(IAltPlayer player, params string[] args)
        {
            var message = string.Join("", args);
            if (args.Length == 0 || message.Length == 0) return Task.CompletedTask;
            
            if (args[0].StartsWith("/")) return Task.CompletedTask;
            if (!Misc.ChatState && !player.IsAdmin)
            {
                player.SendChatMessage("{FF0000}Chat is disabled!");
                return Task.CompletedTask;
            }

            foreach (var p in Alt.GetAllPlayers())
            {
                p.SendChatMessage($"{(player.IsAdmin ? "{008736}" : "{FFFFFF}")} <b>{player.Name}({player.Id})</b>: {{FFFFFF}}{message}");
            }
            return Task.CompletedTask;
        }
    } 
}
