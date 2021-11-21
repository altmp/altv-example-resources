using System.Threading;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;

namespace Freeroam_Extended
{
    public class EventHandler : IScript
    {
        [AsyncScriptEvent(ScriptEventType.VehicleDestroy)]
        public async Task OnVehicleDestroy(IVehicle target, IEntity attacker, uint bodyHealthDamage, uint additionalBodyHealthDamage, uint engineHealthDamage, uint petrolTankDamage, uint weaponHash)
        {
            // TODO: find owner of vehicle
            
            await Task.Delay(5000);
            
            // create async context
            await using (var asyncContext = AsyncContext.Create())
            {
                if (target.TryToAsync(asyncContext, out var asyncVehicle)) return;
                
                //TODO: send message to owner of vehicle
                
                asyncVehicle.Remove();
            }
        }
    }
}