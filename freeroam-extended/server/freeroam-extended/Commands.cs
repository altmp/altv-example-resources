using System;
using System.Collections.Generic;
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
        }
        
        [Command("ghost")]
        public void Ghost(IAltPlayer player)
        {
            // Disable
            if (player.GhostMode)
            {
                player.SendChatMessage("{00FF00} Ghost Mode disabled! You're no longer invincible.");
                player.Invincible = false;
                return;
            }
            // Enable
            player.Invincible = true;
        }
        
        [Command("weapons")]
        public void GetWeapons(IAltPlayer player) 
        {
            // give all weapons from WeaponModel Enum to player
            foreach (var weapon in Enum.GetValues(typeof(WeaponModel)).Cast<WeaponModel>().Where(w => !Misc.Misc.BlacklistedWeapons.Contains((uint)w)))
            {
                player.GiveWeapon(weapon, 1000, false);
            }
        }
        
        [Command("model")]
        public void ChangeModel(IAltPlayer player, string modelName)
        {
            player.Model = Alt.Hash(modelName);
        }

        [Command("tp")]
        public void Teleport(IAltPlayer player, int id)
        {
            if (id > Misc.Misc.SpawnPositions.Count || id < 0)
            {
                player.SendChatMessage($"{{FF0000}}Invalid Spawnpoint! (Minimum 1, Maximum: {Misc.Misc.SpawnPositions.Count}");
            }
            var spawnpoint = Misc.Misc.SpawnPositions.ElementAt(id);
            player.Position = spawnpoint;
        }
        
        [Command("pos")]
        public void Position(IAltPlayer player)
        {
            Alt.Log($"new Position({player.Position.X}, {player.Position.Y}, {player.Position.Z}),");
        }
        
        [Command("ban")]
        public void Ban(IAltPlayer player, int id)
        {
            if (!Misc.Misc.Operators.Contains(player.Id))
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
            if (Misc.Misc.BannedPlayers.Contains(target.Id))
            {
                player.SendChatMessage($"{{FF0000}}Player with {id} is already banned!");
                return;
            }
            
            target.Kick("You've been banned from this server!");
            Misc.Misc.BannedPlayers.Add(target.HardwareIdHash + player.HardwareIdExHash);
            player.SendChatMessage($"{{00FF00}}Player with id {id} banned!");
        }
        
        [Command("unban")]
        public void Unban(IAltPlayer player, int id)
        {
            if (!Misc.Misc.Operators.Contains(player.Id))
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
            if (!Misc.Misc.BannedPlayers.Contains(target.Id))
            {
                player.SendChatMessage($"{{FF0000}}Player with {id} is not banned!");
                return;
            }
            
            Misc.Misc.BannedPlayers.Remove(target.HardwareIdHash + player.HardwareIdExHash);
            player.SendChatMessage($"{{00FF00}}Player with id {id} unbanned!");
        }
        
    }
}