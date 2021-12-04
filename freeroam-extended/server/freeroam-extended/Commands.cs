using System;
using System.Linq;
using System.Numerics;
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

            var spawnedVeh = (AltVehicle)Alt.CreateVehicle(Alt.Hash(vehicleName),
                player.Position + new Position(1, 0, 0), Rotation.Zero);
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
            var weapons = Enum.GetValues(typeof(WeaponModel)).Cast<WeaponModel>().ToList()
                .Where(w => !Misc.BlacklistedWeapons.Contains((uint) w));
            foreach (var weapon in weapons)
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
        public void Teleport(IAltPlayer player, int id = 0)
        {
            if (id + 1 > Misc.SpawnPositions.Length || id <= 0)
            {
                player.SendChatMessage(
                    $"{{FF0000}}Invalid Spawnpoint! (Minimum 1, Maximum: {Misc.SpawnPositions.Length + 1}");
            }

            var spawnpoint = Misc.SpawnPositions.ElementAt(id + 1);
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
            
            target.Kick("You've been banned from this server!");
            Misc.BannedPlayers.Add((target.HardwareIdHash, target.HardwareIdExHash));
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
            if (!Misc.BannedPlayers.Any(tuple =>
                tuple.Item1 == target.HardwareIdHash && tuple.Item2 == target.HardwareIdExHash))
            {
                player.SendChatMessage($"{{FF0000}}Player with {id} is not banned!");
                return;
            }

            // remove banned player from list
            Misc.BannedPlayers.Remove((player.HardwareIdHash, player.HardwareIdExHash));
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

        [Command("dm")]
        public void Dm(IAltPlayer player)
        {
            player.SendChatMessage(player.DmMode ? "{00FF00} DM mode disabled!" : "{00FF00}DM mode enabled!");
            player.DmMode = !player.DmMode;
            player.Emit("set_last_command");
        }

        [Command("togglechat")]
        public void ToggleChat(IAltPlayer player, bool state)
        {
            // check if player is operator
            if (!Misc.Operators.Contains(player.Id))
            {
                player.SendChatMessage("{FF0000} No permission!");
                return;
            }

            player.SendChatMessage("{00FF00} Chat is now " + (state ? "enabled" : "disabled") + "!");
            Misc.ChatState = state;
            foreach (var p in Alt.GetAllPlayers())
            {
                // check if player is operator
                if (Misc.Operators.Contains(p.Id)) continue;
                p.Emit("set_chat_state", state);
            }
        }

        [Command("dimension")]
        public void Dimension(IAltPlayer player, int dimension = 0)
        {
            if (!Misc.Operators.Contains(player.Id))
            {
                player.SendChatMessage("{FF0000} No permission!");
                return;
            }

            player.Dimension = dimension;
            player.Emit("set_last_command");
        }

        [Command("clearvehicles")]
        public void ClearVehicles(IAltPlayer player)
        {
            // get all vehicles owned by player
            foreach (var veh in Alt.GetAllVehicles())
            {
                if (veh is not IAltVehicle vehicle) continue;
                if (vehicle.Owner.Id != player.Id) continue;
                veh.Remove();
            }
            player.Emit("set_last_command");
        }

        [Command("tpallhere")]
        public void TpAllhere(IAltPlayer player)
        {
            if (!Misc.Operators.Contains(player.Id))
            {
                player.SendChatMessage("{FF0000} No permission!");
                return;
            }

            foreach (var p in Alt.GetAllPlayers())
            {
                if (p is not { } target) continue;
                target.Position = player.Position;
                target.SendChatMessage("{00FF00} You were teleported to " + player.Name + "!");
            }
            player.Emit("set_last_command");
        }

        [Command("tphere")]
        public void TpHere(IAltPlayer player, int target)
        {
            if (!Misc.Operators.Contains(player.Id))
            {
                player.SendChatMessage("{FF0000} No permission!");
                return;
            }

            // check if target is online
            if (Alt.GetAllPlayers().All(p => p.Id != target))
            {
                player.SendChatMessage("{FF0000} Player not found!");
                return;
            }
            var targetPlayer = Alt.GetAllPlayers().First(p => p.Id == target);
            targetPlayer.Position = player.Position;
            targetPlayer.SendChatMessage("{00FF00} You were teleported to " + player.Name + "!");
            player.Emit("set_last_command");
        }

        [Command("tpto")]
        public void TpTo(IAltPlayer player, int target)
        {
            if (!Misc.Operators.Contains(player.Id))
            {
                player.SendChatMessage("{FF0000} No permission!");
                return;
            }

            // check if target is online
            if (Alt.GetAllPlayers().All(p => p.Id != target))
            {
                player.SendChatMessage("{FF0000} Player not found!");
                return;
            }
            var targetPlayer = Alt.GetAllPlayers().First(p => p.Id == target);
            player.Position = targetPlayer.Position;
            player.SendChatMessage("{00FF00} You were teleported to " + player.Name + "!");
            player.Emit("set_last_command");
        }

        [Command("clearallvehicles")]
        public void ClearAllVehicles(IAltPlayer player, int distance = 0)
        {
            if (!Misc.Operators.Contains(player.Id))
            {
                player.SendChatMessage("{FF0000} No permission!");
                return;
            }

            if (distance == 0)
            {
                foreach (var veh in Alt.GetAllVehicles())
                {
                    veh.Remove();
                }
                return;
            }
            foreach (var veh in Alt.GetAllVehicles())
            {
                // compare squared distance between player and vehicle
                if (Vector3.DistanceSquared(veh.Position, player.Position) <= distance * distance) veh.Remove();
            }
            player.Emit("set_last_command");
        }

        [Command("settime")]
        public void SetTime(IAltPlayer player, int hour)
        {
            if (!Misc.Operators.Contains(player.Id))
            {
                player.SendChatMessage("{FF0000} No permission!");
                return;
            }
            if (hour > 23 || hour < 0)
            {
                player.SendChatMessage("{FF0000} Invalid hour!");
                return;
            }

            foreach (var p in Alt.GetAllPlayers())
            {
                p.SetDateTime(0, 0, 0, hour, 0, 0);
            }
            player.Emit("set_last_command");
        }

        [Command("setweather")]
        public void SetWeather(IAltPlayer player, uint weather)
        {
            if (!Misc.Operators.Contains(player.Id))
            {
                player.SendChatMessage("{FF0000} No permission!");
                return;
            }
            if (weather > 14)
            {
                player.SendChatMessage("{FF0000} Invalid weather!");
                return;
            }
            foreach (var p in Alt.GetAllPlayers())
            {
                p.SetWeather(weather);
            }
            Misc.Weather = weather;
            player.Emit("set_last_command");
        }
    }
}