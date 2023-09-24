using System.Numerics;
using System.Text.Json;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Enums;
using AltV.Net.Resources.Chat.Api;
using Freeroam_Extended.Clothes;
using Freeroam_Extended.Factories;

namespace Freeroam_Extended
{
    public class Commands : IScript
    {
        private readonly Random _random = new();
        
        [Command("veh")]
        public void SpawnVeh(IAltPlayer player, params string[] args)
        {
            if (args.Length < 1)
            {
                player.SendChatMessage(ChatConstants.ErrorPrefix + "Usage: /veh [vehicle name]");
                return;
            }

            var vehicleName = args[0];

            if (player.EnableWeaponUsage)
            {
                player.SendChatMessage(ChatConstants.ErrorPrefix + "You cannot spawn vehicles in DM zone!");
                return;
            }

            if (!Enum.IsDefined(typeof(VehicleModel), Alt.Hash(vehicleName)))
            {
                player.SendChatMessage(ChatConstants.ErrorPrefix + "Invalid vehicle model!");
                return;
            }

            if (!VehicleController.CheckVehicle(vehicleName) && !player.IsAdmin)
            {
                player.SendChatMessage(ChatConstants.ErrorPrefix + "Vehicle is not available!");
                if (VehicleController.IsWhitelist) player.SendChatMessage("Available vehicles: " + string.Join(", ", VehicleController.List));
                return;
            }

            if (player.InteriorLocation != 0)
            {
                player.SendChatMessage(ChatConstants.ErrorPrefix + "You can't spawn vehicles in interiors!");
                return;
            }

            if (Alt.GetAllVehicles().Any(veh => veh.Position.Distance(player.Position) < 3))
            {
                player.SendChatMessage(ChatConstants.ErrorPrefix + "You are too close to a vehicle!");
                return;
            }

            if (player.LastVehicleSpawn.AddSeconds(10) > DateTime.Now)
            {
                player.SendChatMessage(ChatConstants.ErrorPrefix + "You have to wait 10s before spawning a new vehicle!");
                return;
            }

            if (player.Vehicles.Count >= 3)
            {
                var target = player.Vehicles.OrderBy(veh => veh.SpawnTime).First();
                player.Vehicles.Remove(target);
                target.Destroy();
                player.SendChatMessage(ChatConstants.ErrorPrefix + "You can't have more than 3 vehicles. We removed your oldest one!");
            }

            if (player.IsInVehicle)
            {
                player.SendChatMessage(ChatConstants.ErrorPrefix + "You are already in a vehicle we replaced it for you!");
                player.Vehicle.Destroy();
                return;
            }

            lock (StatsController.StatsData)
            {
                StatsController.StatsData.VehiclesSpawned++;
            }

            var spawnedVeh = (AltVehicle) Alt.CreateVehicle(Alt.Hash(vehicleName),
                player.Position + new Position(1, 0, 0), new Rotation(0, 0, player.Rotation.Yaw));
            player.SetIntoVehicle(spawnedVeh, 1);
            player.LastVehicleSpawn = DateTime.Now;
            player.Vehicles.Add(spawnedVeh);
            spawnedVeh.Owner = player;
        }

        [Command("weapons")]
        public void GetWeapons(IAltPlayer player)
        {
            // give all weapons from WeaponModel Enum to player
            var weapons = Misc.WhitelistedWeapons;
            foreach (var weapon in weapons)
            {
                player.GiveWeapon(weapon, 1000, false);
            }
        }

        // TODO fix in core
        // [Command("model")]
        // public async Task ChangeModel(IAltPlayer player)
        // {
        //     if (player.Model == Alt.Hash("mp_m_freemode_01"))
        //     {
        //         player.Model = Alt.Hash("mp_f_freemode_01");
        //     }
        //     else
        //     {
        //         player.Model = Alt.Hash("mp_m_freemode_01");
        //     }
        //     player.Spawn(player.Position);
        //
        //     // AppearanceController.RefreshFace(player);
        //     // await AppearanceController.RefreshClothes(player);
        //     
        //     player.SendChatMessage(ChatConstants.SuccessPrefix + $"Your model was changed");
        // }

        [Command("outfit")]
        public async Task Outfit(IAltPlayer player, string outfitUniqueName = "")
        {
            if (string.IsNullOrEmpty(outfitUniqueName))
            {
                await AppearanceController.RefreshClothes(player);
                return;
            }
            AppearanceController.EquipOutfit(player, Alt.Hash(outfitUniqueName));
            player.SendChatMessage(
                    $"{{00FF00}}Your outfit updated");
        }

        [Command("tp")]
        public void Teleport(IAltPlayer player, int id = 0)
        {
            if (id > Misc.SpawnPositions.Length || id <= 0)
            {
                player.SendChatMessage(
                    ChatConstants.ErrorPrefix + $"Invalid Spawnpoint! (Minimum 1, Maximum: {Misc.SpawnPositions.Length})");
                return;
            }

            var spawnpoint = Misc.SpawnPositions[id - 1];
            player.Position = spawnpoint + new Position(_random.Next(0, 10), _random.Next(0, 10), 0);
        }

        [Command("addcomponent")]
        public void WeaponComponent(IAltPlayer player, string name)
        {
            player.AddWeaponComponent(player.CurrentWeapon, Alt.Hash(name));
        }

        [Command("removecomponent")]
        public void RemoveWeaponComponent(IAltPlayer player, string name)
        {
            player.RemoveWeaponComponent(player.CurrentWeapon, Alt.Hash(name));
        }

        [Command("tune")]
        public void Tune(IAltPlayer player, int index, int value)
        {
            if (!player.IsInVehicle)
            {
                player.SendChatMessage("{FF0000} You're not in a vehicle!");
                return;
            }

            player.Vehicle.ModKit = 1;
            player.Vehicle.SetMod((byte) index, (byte) value);
        }

        [Command("dm")]
        public void Dm(IAltPlayer player)
        {
            player.SendChatMessage(player.DmMode
                ? "{00FF00} Respawning in Death Match Zone disabled!"
                : "{00FF00}Respawning in Death Match Zone enabled!");
            player.DmMode = !player.DmMode;

            if (player.DmMode)
            {
                var weapons = Misc.WhitelistedWeapons;
                foreach (var weapon in weapons)
                {
                    player.GiveWeapon(weapon, 1000, false);
                }

                var randomSpawnPoint =
                    Misc.AirportSpawnPositions.ElementAt(_random.Next(0, Misc.AirportSpawnPositions.Length));
                player.Spawn(randomSpawnPoint + new Position(_random.Next(0, 10), _random.Next(0, 10), 0));
            }
        }

        [Command("clearvehicles")]
        public void ClearVehicles(IAltPlayer player)
        {
            foreach (var veh in player.Vehicles)
            {
                veh.Destroy();
            }
            player.SendChatMessage(ChatConstants.SuccessPrefix + "You removed all your vehicles!");
        }

        [Command("revive")]
        public void Respawn(IAltPlayer player)
        {
            if (Misc.AdminOverridedSpawnPos is not null) player.Spawn((Position) Misc.AdminOverridedSpawnPos);
            else player.Spawn(player.Position);
            player.ClearBloodDamage();
            player.Health = 200;
        }
        
        [Command("getpos")]
        public void GetPosition(IAltPlayer player)
        {
            var pos = player.Position;
            player.SendChatMessage(ChatConstants.SuccessPrefix + $"Your position is {pos.X}, {pos.Y}, {pos.Z}!");
            player.Emit("get_pos");
        }
    }
}