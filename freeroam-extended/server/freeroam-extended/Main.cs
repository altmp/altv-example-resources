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
            Alt.CreateColShapeSphere(new Position(-1216.839599609375f, -2832.514404296875f, 13.9296875f), 800);
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