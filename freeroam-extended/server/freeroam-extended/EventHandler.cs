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
            AltAsync.Log("Player Connect!");
            // create async context
            await using (var asyncContext = AsyncContext.Create())
            {
                if (!player.TryToAsync(asyncContext, out var asyncPlayer)) return;
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
    }
}