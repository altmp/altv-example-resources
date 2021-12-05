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
        public async Task OnPlayerConnect(IAltPlayer player, string reason)
        {
            await using( var asyncContext = AsyncContext.Create() )
            { 
                if (!player.TryToAsync(asyncContext, out var asyncPlayer)) return;
                
                if (Misc.BannedPlayers.Contains(new Tuple<ulong, ulong>(asyncPlayer.HardwareIdHash, asyncPlayer.HardwareIdExHash)))
                {
                    asyncPlayer.Kick("You're banned from this server!");
                    AltAsync.Log($"HWID: {asyncPlayer.HardwareIdHash}, SC: {asyncPlayer.SocialClubId}. Tried to join the server with a ban.");
                    return;
                }
            
                if (Misc.Operators.Contains(new Tuple<ulong, ulong>(asyncPlayer.HardwareIdHash, asyncPlayer.HardwareIdExHash)))
                    asyncPlayer.IsAdmin = true;
            
                // select random entry from SpawnPoints
                var randomSpawnPoint = Misc.SpawnPositions.ElementAt(_random.Next(0, Misc.SpawnPositions.Length));
                asyncPlayer.Spawn(randomSpawnPoint + new Position(_random.Next(0, 10), _random.Next(0, 10), 0));
                asyncPlayer.Model = (uint)PedModel.FreemodeMale01;
                asyncPlayer.SetDateTime(1, 1, 1, Misc.Hour, 1, 1);
                asyncPlayer.SetWeather(Misc.Weather);

                asyncPlayer.Emit("draw_dmzone", Misc.DMPos.X, Misc.DMPos.Y, Misc.DMRadius, 150);

                if(asyncPlayer.IsAdmin)
                    asyncPlayer.Emit("set_chat_state", true);

            }

            // create async context
            lock (StatsHandler.StatsData)
            {
                StatsHandler.StatsData.PlayerConnections++;
            }

            return;
        }

        [ScriptEvent(ScriptEventType.VehicleDestroy)]
        public void OnVehicleDestroy(IAltVehicle target)
        {
            lock (StatsHandler.StatsData)
            {
                StatsHandler.StatsData.VehiclesDestroyed++;
            }
            target.Owner.SendChatMessage("Your Vehicle got destroyed. We removed it for you!");
            target.Remove();
        }

        [ScriptEvent(ScriptEventType.PlayerDisconnect)]
        public void OnPlayerDisconnect(IAltPlayer player, string reason)
        {
            var vehicles = player.Vehicles;
           
            foreach (var veh in vehicles)
            {
                veh.Remove();
            }
        }

        [ScriptEvent(ScriptEventType.PlayerDead)]
        public async Task OnPlayerDead(IAltPlayer player, IEntity killer, uint weapon)
        {
            var spawnPointPool = player.DmMode ? Misc.AirportSpawnPositions : Misc.SpawnPositions;
            
            var randomSpawnPoint = spawnPointPool.ElementAt(_random.Next(0, spawnPointPool.Length));
            await player.SpawnAsync(randomSpawnPoint + new Position(_random.Next(0, 10), _random.Next(0, 10), 0));

            lock (StatsHandler.StatsData)
            {
                StatsHandler.StatsData.PlayerDeaths++;
            }

            if (killer is not IAltPlayer killerPlayer)
                return;


            if (!Misc.BlacklistedWeapons.Contains(weapon)) return;
            Alt.Server.LogColored($"~r~ Banned Player: {killerPlayer.Name} ({killerPlayer.Id}) for using illegal weapon!");
            Misc.BannedPlayers.Add(new Tuple<ulong,ulong>(killerPlayer.HardwareIdHash, killerPlayer.HardwareIdExHash));
            string json = JsonSerializer.Serialize(Misc.BannedPlayers);
            await File.WriteAllTextAsync(@"BannedPlayers.json", json);
            await killerPlayer.KickAsync("You're banned from this server!");
        }

        [ScriptEvent(ScriptEventType.ConsoleCommand)]
        public async Task OnConsoleCommand(string name, string[] args)
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
                    if (playerOp is not IAltPlayer playerOpAlt) return;

                    await using (var asyncContext = AsyncContext.Create())
                    {
                        if (!playerOpAlt.TryToAsync(asyncContext, out var asyncPlayer)) return;
                        if (Misc.Operators.Any(tuple => tuple.Item1 == asyncPlayer.HardwareIdHash && tuple.Item2 == asyncPlayer.HardwareIdExHash))
                        {
                            Alt.Log($"Id {args[0]} already is an operator!");   
                            break;
                        }
                        Misc.Operators.Add(new Tuple<ulong,ulong>(asyncPlayer.HardwareIdHash, asyncPlayer.HardwareIdExHash));
                        string json = JsonSerializer.Serialize(Misc.Operators);
                        await File.WriteAllTextAsync(@"Operators.json", json);
                    
                        await asyncPlayer.EmitAsync("set_chat_state", true);
                        asyncPlayer.IsAdmin = true;
                        break;
                    }

                case "deop":
                    if (args.Length is > 1 or 0) 
                    {
                        Alt.Log("Usage: deop <ID>");
                        break;
                    }
                    var playerDeOp = playerPool.FirstOrDefault(x => x.Id == int.Parse(args[0]));
                    if (playerDeOp is not IAltPlayer playerDeOpAlt) return;
                    await using (var asyncContext = AsyncContext.Create())
                    {
                        if (!playerDeOpAlt.TryToAsync(asyncContext, out var asyncPlayer)) return;
                        
                        if (!Misc.Operators.Any(tuple => tuple.Item1 == asyncPlayer.HardwareIdHash && tuple.Item2 == asyncPlayer.HardwareIdExHash))
                        {
                            AltAsync.Log($"Id {args[0]} is not an operator!");
                            break;
                        }
                        Misc.Operators.Remove(new Tuple<ulong,ulong>(asyncPlayer.HardwareIdHash, asyncPlayer.HardwareIdExHash));
                        await asyncPlayer.EmitAsync("set_chat_state", Misc.ChatState);
                        asyncPlayer.IsAdmin = false;
                        break;
                    }
            }
            return;
        }

        [ScriptEvent(ScriptEventType.WeaponDamage)]
        public async Task OnWeaponDamage(IAltPlayer player, IEntity target, uint weapon, ushort damage,
            Position shotOffset, BodyPart bodyPart)
        {
            await using (var asyncContext = AsyncContext.Create())
            {
                if (!player.TryToAsync(asyncContext, out var asyncPlayer)) return;
                if (!Misc.BlacklistedWeapons.Contains(weapon) || player is not { } damagePlayer) return;
                if (!damagePlayer.TryToAsync(asyncContext, out var asyncDamagePlayer)) return;
                
         
            
                Alt.Server.LogColored($"~r~ Banned Player: {asyncDamagePlayer.Name} ({asyncDamagePlayer.Id}) for using illegal weapon!");
                //Misc.BannedPlayers.Add(<ulong, ulong>(damagePlayer.HardwareIdHash, damagePlayer.HardwareIdExHash));
                Misc.BannedPlayers.Add(new Tuple<ulong,ulong>(asyncDamagePlayer.HardwareIdHash, asyncDamagePlayer.HardwareIdExHash));
                string json = JsonSerializer.Serialize(Misc.BannedPlayers);
                File.WriteAllText(@"BannedPlayers.json", json);

                asyncPlayer.Kick("You're banned from this server!");

                return;
            }
        }

        [ScriptEvent(ScriptEventType.ColShape)]
        public void OnColshapeEnter(IColShape colshape, IEntity target, bool state)
        {
            if (target is not IAltPlayer targetPlayer) return;

            // entity to async
            targetPlayer.EnableWeaponUsage = state;
            targetPlayer.Emit("airport_state", state);
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
        public void OnChatMessage(IAltPlayer player, params string[] args)
        {
            var message = string.Join("", args);
            if (args.Length == 0 || message.Length == 0) return;
            
            if (args[0].StartsWith("/")) return;
            if (!Misc.ChatState && !player.IsAdmin)
            {
                player.SendChatMessage("{FF0000}Chat is disabled!");
                return;
            }

            foreach (var p in Alt.GetAllPlayers())
            {
                p.SendChatMessage($"{(player.IsAdmin ? "{008736}" : "{FFFFFF}")} <b>{player.Name}({player.Id})</b>: {{FFFFFF}}{message}");
            }
        }
    } 
}
