using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
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
                if (player.TryToAsync(asyncContext, out var asyncPlayer)) return;
                
                // select random entry from SpawnPoints
                var randomSpawnPoint = Misc.Misc.SpawnPositions.ElementAt(new Random().Next(0, Misc.Misc.SpawnPositions.Count));
                player.Spawn(randomSpawnPoint, 0);
            }
        }

        [AsyncScriptEvent(ScriptEventType.VehicleDestroy)]
        public async Task OnVehicleDestroy(IAltVehicle target, IEntity attacker, uint bodyHealthDamage, uint additionalBodyHealthDamage, uint engineHealthDamage, uint petrolTankDamage, uint weaponHash)
        {
            await Task.Delay(5000);
            target.Owner.SendChatMessage("{FF0000} Your vehicle got destroyed! We removed it for you.");
            
            // create async context
            await using (var asyncContext = AsyncContext.Create())
            {
                if (target.TryToAsync(asyncContext, out var asyncVehicle)) return;
                asyncVehicle.Remove();
            }
        }
    }
}