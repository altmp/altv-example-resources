using System;
using System.Linq;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using AltV.Net.Resources.Chat.Api;
using Freeroam_Extended.Factories;

namespace Freeroam_Extended
{
    public class Commands : IScript
    {
        [Command("veh")]
        public void SpawnVeh(IAltPlayer player, string vehicleName)
        {
            if (player.IsInVehicle)
            {
                player.SendChatMessage("{FF0000} You are already in a vehicle!");
                return;
            }
            
            if (Alt.GetAllVehicles().Any(veh => veh.Position.Distance(player.Position) < 3))
            {
                player.SendChatMessage("{FF0000} You are too close to a vehicle!");
                return;
            }
            
            if (player.LastVehicleSpawn.AddSeconds(10) > DateTime.Now)
            {
                player.SendChatMessage("{FF0000} You have to wait 10s before spawning a new vehicle!");
                return;
            }
            
            if (player.Vehicles.Count >= 3)
            {
                var target = player.Vehicles.OrderBy(veh => veh.SpawnTime).First();
                player.Vehicles.Remove(target);
                target.Remove();
                player.SendChatMessage("{FF0000} You can't have more than 3 vehicles. We removed your oldest one!");
            }
            
            var spawnedVeh = (AltVehicle)Alt.CreateVehicle(Alt.Hash(vehicleName), player.Position + new Position(1, 0,0), Rotation.Zero);
            player.SetIntoVehicle(spawnedVeh, 1);
            player.LastVehicleSpawn = DateTime.Now;
            player.Vehicles.Add(spawnedVeh);
            spawnedVeh.Owner = player;
            player.Emit("set_last_command");
        }
        
        // [Command("spectate")]
        // public void Spectate(IAltPlayer player)
        // {
        //     // Disable
        //     if (player.GhostMode)
        //     {
        //         player.SendChatMessage("{00FF00} Spectator Mode disabled! You're no longer invincible.");
        //         player.Emit("ghost_mode", false);
        //         player.GhostMode = false;
        //         player.DeleteStreamSyncedMetaData("spectator");
        //         return;
        //     }
        //     // Enable
        //     player.GhostMode = true;
        //     player.Emit("ghost_mode", true);
        //     player.SendChatMessage("{00FF00} Spectator Mode enabled! You're now invincible.");
        //     player.SetStreamSyncedMetaData("spectator", true);
        // }
        
        [Command("weapons")]
        public void GetWeapons(IAltPlayer player) 
        {
            // give all weapons from WeaponModel Enum to player
            foreach (var weapon in Enum.GetValues(typeof(WeaponModel)).Cast<WeaponModel>().Where(w => !Misc.BlacklistedWeapons.Contains((uint)w)))
            {
                player.GiveWeapon(weapon, 1000, false);
            }
            player.Emit("set_last_command");
        }

        [Command("model")]
        public void ChangeModel(IAltPlayer player, string modelName)
        {
            player.Model = Alt.Hash(modelName);
            player.Emit("set_last_command");
        }

        [Command("tp")]
        public void Teleport(IAltPlayer player, int id)
        {
            if (id > Misc.SpawnPositions.Length || id < 0)
            {
                player.SendChatMessage($"{{FF0000}}Invalid Spawnpoint! (Minimum 1, Maximum: {Misc.SpawnPositions.Length}");
            }
            var spawnpoint = Misc.SpawnPositions.ElementAt(id);
            var random = new Random();
            player.Position = spawnpoint + new Position(random.Next(0, 10), random.Next(0, 10), 0);
            player.Emit("set_last_command");
        }
        
        [Command("pos")]
        public void Position(IAltPlayer player)
        {
            Alt.Log($"new Position({player.Position.X}, {player.Position.Y}, {player.Position.Z}),");
        }
        
        [Command("ban")]
        public void Ban(IAltPlayer player, int id)
        {
            if (!Misc.Operators.Contains(player.Id))
            {
                player.SendChatMessage("{FF0000} No permission!");
                return;   
            }
            
            var target = Alt.GetAllPlayers().FirstOrDefault(p => p.Id == id);
            if (target == null)
            {
                player.SendChatMessage($"{{FF0000}}Player with id {id} not found!");
                return;
            }
            
            // check if player is already banned
            if (Misc.BannedPlayers.Any(tuple => tuple.Item1 == target.HardwareIdHash && tuple.Item2 == target.HardwareIdExHash))
            {
                player.SendChatMessage($"{{FF0000}}Player with {id} is already banned!");
                return;
            }
            
            target.Kick("You've been banned from this server!");
            Misc.BannedPlayers.Add(new Tuple<ulong, ulong>(target.HardwareIdHash, target.HardwareIdExHash));
            player.SendChatMessage($"{{00FF00}}Player with id {id} banned!");
            player.Emit("set_last_command");
        }
        
        [Command("unban")]
        public void Unban(IAltPlayer player, int id)
        {
            if (!Misc.Operators.Contains(player.Id))
            {
                player.SendChatMessage("{FF0000} No permission!");
                return;   
            }

            var target = Alt.GetAllPlayers().FirstOrDefault(p => p.Id == id);
            if (target == null)
            {
                player.SendChatMessage($"{{FF0000}}Player with id {id} not found!");
                return;
            }
            
            // check if player is already banned
            if (!Misc.BannedPlayers.Any(tuple => tuple.Item1 == target.HardwareIdHash && tuple.Item2 == target.HardwareIdExHash))
            {
                player.SendChatMessage($"{{FF0000}}Player with {id} is not banned!");
                return;
            }
            
            // remove banned player from list
            Misc.BannedPlayers.RemoveWhere(p => p.Item1 == target.HardwareIdHash && p.Item2 == target.HardwareIdExHash);
            player.SendChatMessage($"{{00FF00}}Player with id {id} unbanned!");
            player.Emit("set_last_command");
        }

        [Command("addcomponent")]
        public void WeaponComponent(IAltPlayer player, string name)
        {
            player.AddWeaponComponent(player.CurrentWeapon, Alt.Hash(name));
            player.Emit("set_last_command");
        }
        
        [Command("removecomponent")]
        public void RemoveWeaponComponent(IAltPlayer player, string name)
        {
            player.RemoveWeaponComponent(player.CurrentWeapon, Alt.Hash(name));
            player.Emit("set_last_command");
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
            player.Vehicle.SetMod((byte)index, (byte)value);
            player.Emit("set_last_command");
        }
    }
}