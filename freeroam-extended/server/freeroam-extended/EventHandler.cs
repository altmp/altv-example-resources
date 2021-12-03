using System;
using System.Linq;
using System.Threading.Tasks;
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
                return Task.CompletedTask;
            }
            // select random entry from SpawnPoints
            var randomSpawnPoint = Misc.SpawnPositions.ElementAt(_random.Next(0, Misc.SpawnPositions.Length));
            player.Spawn(randomSpawnPoint + new Position(_random.Next(0, 10), _random.Next(0, 10), 0));
            player.Model = (uint) PedModel.FreemodeMale01;
            
            return Task.CompletedTask;
        }

        [AsyncScriptEvent(ScriptEventType.VehicleDestroy)]
        public async Task OnVehicleDestroy(IAltVehicle target)
        {
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
            
            var randomSpawnPoint = spawnPointPool.ElementAt(_random.Next(0, Misc.SpawnPositions.Length));
            player.Spawn(randomSpawnPoint + new Position(_random.Next(0, 10), _random.Next(0, 10), 0));

            if (!Misc.BlacklistedWeapons.Contains(weapon) || killer is not IAltPlayer killerPlayer) return Task.CompletedTask;
            Alt.Server.LogColored($"~r~ Banned Player: {killerPlayer.Name} ({killerPlayer.Id}) for using illegal weapon!");
            Misc.BannedPlayers.Add(new Tuple<ulong, ulong>(killerPlayer.HardwareIdHash, killerPlayer.HardwareIdExHash));
            killerPlayer.Kick("You're banned from this server!");

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
                    if (playerOp is null)
                    {
                        Alt.Log("Player not online!");
                        return Task.CompletedTask;
                    }
                    
                    if (Misc.Operators.Contains(int.Parse(args[0])))
                    {
                        Alt.Log($"Id {args[0]} already is an operator!");   
                        break;
                    }
                    Misc.Operators.Add(int.Parse(args[0]));
                    break;
                
                case "deop":
                    if (args.Length is > 1 or 0) 
                    {
                        Alt.Log("Usage: deop <ID>");
                        break;
                    } 
                    
                    var playerDeOp = playerPool.FirstOrDefault(x => x.Id == int.Parse(args[0]));
                    if (playerDeOp is null)
                    {
                        Alt.Log("Player not online!");
                        return Task.CompletedTask;
                    }
                    
                    if (!Misc.Operators.Contains(int.Parse(args[0])))
                    {
                        Alt.Log($"Id {args[0]} is not an operator!");
                        break;
                    }
                    Misc.Operators.Remove(int.Parse(args[0]));
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
            Misc.BannedPlayers.Add(new Tuple<ulong, ulong>(damagePlayer.HardwareIdHash, damagePlayer.HardwareIdExHash));
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
        public Task OnFireStart(IAltPlayer player, FireInfo[] fireInfos)
        {
            return Task.FromResult(false);
        }

        [ScriptEvent(ScriptEventType.Explosion)]
        public Task OnExplosion(IAltPlayer player, ExplosionType explosionType, Position position, uint explosionFx,
            IEntity target)
        {
            return Task.FromResult(false);
        }

        [ScriptEvent(ScriptEventType.StartProjectile)]
        public Task OnProjectileStart(IAltPlayer player, Position startPosition, Position direction, uint ammoHash, uint weaponHash)
        {
            return Task.FromResult(false);
        }

        [ClientEvent("chat:message")]
        public Task OnChatMessage(IAltPlayer player, params string[] args)
        {
            var isAdmin = Misc.Operators.Contains(player.Id);
            // check if chat is enabled
            if (!Misc.ChatState && !isAdmin) return Task.CompletedTask;

            foreach (var p in Alt.GetAllPlayers())
            {
                p.SendChatMessage($"{(isAdmin ? "{008736}" : "{FFFFFF}")}  {player.Name}: {string.Join("", args)}");
            }
            return Task.CompletedTask;
        }
    } 
}
