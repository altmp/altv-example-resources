using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using AltV.Net.Resources.Chat.Api;
using Freeroam_Extended.Factories;

namespace Freeroam_Extended
{
    public class EventHandler : IScript
    {
        [AsyncScriptEvent(ScriptEventType.PlayerConnect)]
        public async Task OnPlayerConnect(IAltPlayer player, string reason)
        {
            // create async context
            await using (var asyncContext = AsyncContext.Create())
            {
                if (!player.TryToAsync(asyncContext, out var asyncPlayer)) return;
                if (Misc.Misc.BannedPlayers.Contains(asyncPlayer.HardwareIdHash + asyncPlayer.HardwareIdExHash)) // Player banned
                {
                    asyncPlayer.Kick("You're banned from this server!");
                    return;
                }
                // select random entry from SpawnPoints
                var randomSpawnPoint = Misc.Misc.SpawnPositions.ElementAt(new Random().Next(0, Misc.Misc.SpawnPositions.Count));
                asyncPlayer.Spawn(randomSpawnPoint, 0);
                asyncPlayer.Model = (uint) PedModel.FreemodeMale01;
            }
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
        public async Task OnPlayerDisconnect(IAltPlayer player, string reason)
        {
            var vehicles = Alt.GetAllVehicles().Cast<IAltVehicle>().Where(x => x.Owner == player);
            await using (var asyncContext = AsyncContext.Create())
            {
                foreach (var veh in vehicles)
                {
                    if (!veh.TryToAsync(asyncContext, out var asyncVeh)) continue;
                    if (veh.Owner.Id != player.Id) continue;
                    veh.Remove();
                }   
            }
        }

        [AsyncScriptEvent(ScriptEventType.PlayerDead)]
        public async Task OnPlayerDead(IAltPlayer player, IEntity killer, uint weapon)
        {
            // create async context
            await using (var asyncContext = AsyncContext.Create())
            {
                if (!player.TryToAsync(asyncContext, out var asyncPlayer)) return;
                // find random spawnpoint
                var randomSpawnPoint = Misc.Misc.SpawnPositions.ElementAt(new Random().Next(0, Misc.Misc.SpawnPositions.Count));
                asyncPlayer.Spawn(randomSpawnPoint);
            }
        }

        [AsyncScriptEvent(ScriptEventType.ConsoleCommand)]
        public async Task OnConsoleCommand(string name, string[] args)
        {
            switch (name)
            {
                case "op":
                    if (args.Length is > 1 or 0) 
                    {
                        Alt.Log("Usage: op <ID>");
                        break;
                    }
                    if (Misc.Misc.Operators.Contains(int.Parse(args[0])))
                    {
                        Alt.Log($"Id {args[1]} already is an operator!");   
                        break;
                    }
                    Misc.Misc.Operators.Add(int.Parse(args[1]));
                    break;
                
                case "deop":
                    if (args.Length is > 1 or 0) 
                    {
                        Alt.Log("Usage: deop <ID>");
                        break;
                    }
                    if (!Misc.Misc.Operators.Contains(int.Parse(args[0])))
                    {
                        Alt.Log($"Id {args[1]} is not an operator!");
                        break;
                    }
                    Misc.Misc.Operators.Remove(int.Parse(args[1]));
                    break;
            }
        }
    }
}