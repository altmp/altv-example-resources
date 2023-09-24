using System.Numerics;
using System.Text.Json;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.ColoredConsole;
using AltV.Net.Elements.Entities;
using Freeroam_Extended.Factories;
using Timer = System.Timers.Timer;

namespace Freeroam_Extended
{
    public class Main : AsyncResource
    {
        public override void OnStart()
        {
            ChatController.Init();
            PlayerController.Init();
            VehicleController.Init();
            VoiceController.Init();
            StatsController.Init();
            
            // colshape for weapon disabling everywhere but the airport
            Alt.CreateColShapeSphere(Misc.DMPos, Misc.DMRadius);
            
            var fileWriteTimer = new Timer();
            fileWriteTimer.Interval = 60000;
            fileWriteTimer.Enabled = true;
            fileWriteTimer.Elapsed += (sender, args) =>
            {
                StatsController.UpdateStats();
                foreach (var p in Alt.GetAllPlayers())
                {
                    var player = (IAltPlayer)p;
                    player.ResetEventCount();
                }
            };
            Alt.LogColored(new ColoredMessage() + TextColor.Green + "Freeroam-Extended Started!");
        }

        public override void OnStop()
        {
            Alt.LogColored(new ColoredMessage() + TextColor.Green + "Freeroam-Extended Stopped!");
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