using System.Numerics;
using System.Text.Json;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Enums;
using AltV.Net.Resources.Chat.Api;
using Freeroam_Extended.Factories;

namespace Freeroam_Extended
{
    public class Commands : IScript
    {
        private readonly Random _random = new();

        [Command("veh")]
        public void SpawnVeh(IAltPlayer player, string vehicleName)
        {
            if (Misc.BlacklistedVehicle.Contains(Alt.Hash(vehicleName)) && !player.IsAdmin)
            {
                player.SendChatMessage("{FF0000} Vehicle is blacklisted.");
                return;
            }

            if (!Enum.IsDefined(typeof(VehicleModel), Alt.Hash(vehicleName)))
            {
                player.SendChatMessage("{FF0000} Invalid vehicle model!");
                return;
            }
            
            if (player.InteriorLocation != 0)
            {
                player.SendChatMessage("{FF0000} You can't spawn vehicles in interiors!");
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

            if (player.IsInVehicle)
            {
                player.SendChatMessage("{FF0000} You are already in a vehicle we replaced it for you!");
                player.Vehicle.Remove();
                return;
            }

            lock (StatsHandler.StatsData)
            {
                StatsHandler.StatsData.VehiclesSpawned++;
            }

            var spawnedVeh = (AltVehicle) Alt.CreateVehicle(Alt.Hash(vehicleName),
                player.Position + new Position(1, 0, 0), new Rotation(0, 0, player.Rotation.Yaw));
            player.SetIntoVehicle(spawnedVeh, 1);
            player.LastVehicleSpawn = DateTime.Now;
            player.Vehicles.Add(spawnedVeh);
            spawnedVeh.Owner = player;
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
            var weapons = Misc.WhitelistedWeapons;
            foreach (var weapon in weapons)
            {
                player.GiveWeapon(weapon, 1000, false);
            }
        }

        // [Command("model")]
        // public void ChangeModel(IAltPlayer player, string modelName)
        // {
        //     player.Model = Alt.Hash(modelName);
        //     
        // }

        [Command("tp")]
        public void Teleport(IAltPlayer player, int id = 0)
        {
            if (id > Misc.SpawnPositions.Length || id <= 0)
            {
                player.SendChatMessage(
                    $"{{FF0000}}Invalid Spawnpoint! (Minimum 1, Maximum: {Misc.SpawnPositions.Length})");
                return;
            }

            var spawnpoint = Misc.SpawnPositions[id - 1];
            player.Position = spawnpoint + new Position(_random.Next(0, 10), _random.Next(0, 10), 0);
        }

        [Command("ban")]
        public void Ban(IAltPlayer player, int id)
        {
            if (!player.IsAdmin)
            {
                player.SendChatMessage("{FF0000} No permission!");
                return;
            }

            if (player.Id == id)
            {
                player.SendChatMessage("{FF0000} You can't ban yourself!");
                return;
            }

            var target = Alt.GetAllPlayers().FirstOrDefault(p => p.Id == id);
            if (target == null)
            {
                player.SendChatMessage($"{{FF0000}}Player with id {id} not found!");
                return;
            }

            target.Kick("You've been banned from this server!");
            Misc.BannedPlayers.Add(new Tuple<ulong, ulong>(target.HardwareIdHash, target.HardwareIdExHash));
            string json = JsonSerializer.Serialize(Misc.BannedPlayers);
            File.WriteAllText(@"BannedPlayers.json", json);

            player.SendChatMessage($"{{00FF00}}Player with id {id} banned!");
        }

        [Command("unban")]
        public void Unban(IAltPlayer player, ulong hwid)
        {
            if (!player.IsAdmin)
            {
                player.SendChatMessage("{FF0000} No permission!");
                return;
            }

            var target = Misc.BannedPlayers.FirstOrDefault(tuple => tuple.Item1 == hwid);
            if (target == null)
            {
                player.SendChatMessage($"{{FF0000}}Player with hwid {hwid} not found!");
                return;
            }

            if (Misc.BannedPlayers.All(tuple => tuple.Item1 != hwid))
            {
                player.SendChatMessage($"{{FF0000}}Player with hwid {hwid} not banned!");
                return;
            }

            // remove banned player from list
            Misc.BannedPlayers.Remove(new Tuple<ulong, ulong>(player.HardwareIdHash, player.HardwareIdExHash));
            player.SendChatMessage($"{{00FF00}}Player with hwid {hwid} unbanned!");
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

        [Command("togglechat")]
        public void ToggleChat(IAltPlayer player, bool state)
        {
            // check if player is operator
            if (!player.IsAdmin && !Misc.ChatState)
            {
                player.SendChatMessage("{FF0000} No permission!");
                return;
            }

            player.SendChatMessage("{00FF00} Chat is now " + (state ? "enabled" : "disabled") + "!");
            Misc.ChatState = state;
            foreach (var p in Alt.GetAllPlayers())
            {
                // check if player is operator
                if (player.IsAdmin) continue;
                p.Emit("set_chat_state", state);
            }
        }

        [Command("dimension")]
        public void Dimension(IAltPlayer player, int dimension = 0)
        {
            if (!player.IsAdmin)
            {
                player.SendChatMessage("{FF0000} No permission!");
                return;
            }

            player.Dimension = dimension;
        }

        [Command("clearvehicles")]
        public void ClearVehicles(IAltPlayer player)
        {
            // get all vehicles owned by player
            foreach (var veh in player.Vehicles)
            {
                veh.Remove();
            }
        }

        [Command("tpallhere")]
        public void TpAllhere(IAltPlayer player)
        {
            if (!player.IsAdmin)
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
        }

        [Command("tphere")]
        public void TpHere(IAltPlayer player, int target)
        {
            if (!player.IsAdmin)
            {
                player.SendChatMessage("{FF0000} No permission!");
                return;
            }

            var targetPlayer = Alt.GetAllPlayers().FirstOrDefault(p => p.Id == target);
            if (targetPlayer == null)
            {
                player.SendChatMessage("{FF0000} Player not found!");
                return;
            }

            targetPlayer.Position = player.Position;
            targetPlayer.SendChatMessage("{00FF00} You were teleported to " + player.Name + "!");
        }

        [Command("tpto")]
        public void TpTo(IAltPlayer player, int target)
        {
            if (!player.IsAdmin)
            {
                player.SendChatMessage("{FF0000} No permission!");
                return;
            }

            var targetPlayer = Alt.GetAllPlayers().FirstOrDefault(p => p.Id == target);
            if (targetPlayer == null)
            {
                player.SendChatMessage("{FF0000} Player not found!");
                return;
            }

            player.Position = targetPlayer.Position;
            player.SendChatMessage("{00FF00} You were teleported to " + targetPlayer.Name + "!");
        }

        [Command("clearallvehicles")]
        public void ClearAllVehicles(IAltPlayer player, int distance = 0)
        {
            if (!player.IsAdmin)
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

            var distSqr = distance * distance;
            foreach (var veh in Alt.GetAllVehicles())
            {
                // compare squared distance between player and vehicle
                if (Vector3.DistanceSquared(veh.Position, player.Position) <= distSqr) veh.Remove();
            }
        }

        [Command("settime")]
        public void SetTime(IAltPlayer player, int hour)
        {
            if (!player.IsAdmin)
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
        }

        [Command("setweather")]
        public void SetWeather(IAltPlayer player, uint weather)
        {
            if (!player.IsAdmin)
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
        }

        [Command("noclip")]
        public void NoClip(IAltPlayer player)
        {
            if (!player.IsAdmin)
            {
                player.SendChatMessage("{FF0000} No permission!");
                return;
            }

            player.NoClip = !player.NoClip;
            player.Streamed = !player.NoClip;
            player.Visible = !player.NoClip;
            player.SendChatMessage($"{{00FF00}}NoClip is now {(player.NoClip ? "enabled" : "disabled")}!");

            player.Emit("noclip", player.NoClip);
        }

        [Command("revive")]
        public void Respawn(IAltPlayer player)
        {
            if (Misc.AdminOverridedSpawnPos is not null) player.Spawn((Position) Misc.AdminOverridedSpawnPos);
            else player.Spawn(player.Position);
            player.ClearBloodDamage();
        }

        [Command("announce")]
        public void Announce(IAltPlayer player, string header, int time, params string[] body)
        {
            if (!player.IsAdmin)
            {
                player.SendChatMessage("{FF0000} No permission!");
                return;
            }

            var message = string.Join(" ", body);
            Alt.EmitAllClients("announce", header, message, time);
        }

        [Command("tpcoords")]
        public void TpCoords(IAltPlayer player, int x, int y, int z)
        {
            if (!player.IsAdmin)
            {
                player.SendChatMessage("{FF0000} No permission!");
                return;
            }

            player.Position = new Vector3(x, y, z);
            player.SendChatMessage($"{{00FF00}} You were teleported to {x}, {y}, {z}!");
        }

        [Command("kick")]
        public void Kick(IAltPlayer player, int id)
        {
            if (!player.IsAdmin)
            {
                player.SendChatMessage("{FF0000} No permission!");
                return;
            }

            if (player.Id == id)
            {
                player.SendChatMessage("{FF0000} You can't kick yourself!");
                return;
            }

            var target = Alt.GetAllPlayers().FirstOrDefault(p => p.Id == id);
            if (target == null)
            {
                player.SendChatMessage($"{{FF0000}}Player with id {id} not found!");
                return;
            }

            target.Kick("You've been kicked from this server!");

            player.SendChatMessage($"{{00FF00}}Player with id {id} kicked!");
        }

        [Command("godmode")]
        public void Godmode(IAltPlayer player, int id)
        {
            if (!player.IsAdmin)
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

            target.Invincible = !target.Invincible;
            target.SendChatMessage(
                $"{(target.Invincible ? "{00FF00}" : "{FF0000}")}Godmode {(target.Invincible ? "on" : "off")}!");

            if (player.Id != target.Id)
                player.SendChatMessage($"{{00FF00}}Godmode {(target.Invincible ? "on" : "off")}!");
        }

        [Command("overridespawnpos")]
        public void OverrideSpawnPos(IAltPlayer player, bool mode)
        {
            if (!player.IsAdmin)
            {
                player.SendChatMessage("{FF0000} No permission!");
                return;
            }

            if (mode)
            {
                var pos = player.Position;
                Misc.AdminOverridedSpawnPos = pos;

                player.SendChatMessage(
                    $"{{00FF00}} You're overrided spawn position for all player on {pos.X}, {pos.Y}, {pos.Z}!");
            }
            else
            {
                Misc.AdminOverridedSpawnPos = null;
                player.SendChatMessage($"{{00FF00}} You're no longer overrided spawn position!");
            }
        }

        [Command("getpos")]
        public void GetPosition(IAltPlayer player)
        {
            var pos = player.Position;
            player.SendChatMessage($"{{00FF00}} Your position is {pos.X}, {pos.Y}, {pos.Z}!");
            player.Emit("get_pos");
        }

        [Command("godmodeall")]
        public void GodmodeAllPlayers(IAltPlayer player, bool mode)
        {
            if (!player.IsAdmin)
            {
                player.SendChatMessage("{FF0000} No permission!");
                return;
            }

            var targets = Alt.GetAllPlayers().ToList();

            foreach (var target in targets)
            {
                target.Invincible = mode;
                target.SendChatMessage($"{{00FF00}}Godmode for all players is {(mode ? "activated" : "deactivated")}!");
            }
        }
        
        [Command("esp")]
        public void Esp(IAltPlayer player, bool mode)
        {
            if (!player.IsAdmin)
            {
                player.SendChatMessage("{FF0000} No permission!");
                return;
            }

            player.Emit("esp", mode);
        }
    }
}