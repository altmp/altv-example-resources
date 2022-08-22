using System.Numerics;
using System.Text.Json;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using AltV.Net.Resources.Chat.Api;
using Freeroam_Extended.Factories;
using ExplosionType = AltV.Net.Data.ExplosionType;

namespace Freeroam_Extended
{
    public class EventHandler : IScript
    {
        public EventHandler()
        {
            Alt.OnPlayerCustomEvent += (player, name, array) =>
            {
                var altPlayer = (IAltPlayer) player;
                if (name != "chat:message" && !altPlayer.IsAdmin)
                {
#if RELEASE
                    Alt.Log($"{altPlayer.Name} banned for illegal event: {name}");
                    player.Kick("You are not allowed to use this Event.");
                    Misc.BannedPlayers.Add(new Tuple<ulong, ulong>(altPlayer.HardwareIdHash, altPlayer.HardwareIdExHash));
#endif
                }

                altPlayer.EventCount++;
                if (altPlayer.EventCount > 100) altPlayer.Kick("Event count exceeded");
            };
        }

        private readonly Random _random = new();

        [ScriptEvent(ScriptEventType.PlayerConnect)]
        public async Task OnPlayerConnect(IAltPlayer player, string reason)
        {
            if (Misc.BannedPlayers.Contains(new Tuple<ulong, ulong>(player.HardwareIdHash,
                    player.HardwareIdExHash)))
            {
                player.Kick("You're banned from this server!");
                AltAsync.Log(
                    $"HWID: {player.HardwareIdHash}, SC: {player.SocialClubId}. Tried to join the server with a ban.");
                return;
            }

            if (Misc.Operators.Contains(new Tuple<ulong, ulong>(player.HardwareIdHash, player.HardwareIdExHash)))
                player.IsAdmin = true;

            // select random entry from SpawnPoints
            var randomSpawnPoint = Misc.AdminOverridedSpawnPos is not null
                ? Misc.AdminOverridedSpawnPos
                : Misc.SpawnPositions.ElementAt(_random.Next(0, Misc.SpawnPositions.Length));
            player.Spawn((Position) randomSpawnPoint + new Position(_random.Next(0, 10), _random.Next(0, 10), 0));
            player.Model = (uint) PedModel.FreemodeMale01;
            player.SetDateTime(1, 1, 1, Misc.Hour, 1, 1);
            player.SetWeather(Misc.Weather);

            player.Emit("draw_dmzone", Misc.DMPos.X, Misc.DMPos.Y, Misc.DMRadius, 150);

            if (player.IsAdmin)
                player.Emit("set_chat_state", true);


            // create async context
            lock (StatsHandler.StatsData)
            {
                StatsHandler.StatsData.PlayerConnections++;
            }
        }

        [ScriptEvent(ScriptEventType.VehicleDestroy)]
        public void OnVehicleDestroy(IAltVehicle target)
        {
            lock (StatsHandler.StatsData)
            {
                StatsHandler.StatsData.VehiclesDestroyed++;
            }

            target.Owner.SendChatMessage("Your Vehicle got destroyed. We removed it for you!");
            target.Remove();
        }

        [ScriptEvent(ScriptEventType.PlayerDisconnect)]
        public void OnPlayerDisconnect(IAltPlayer player, string reason)
        {
            var vehicles = player.Vehicles;

            foreach (var veh in vehicles)
            {
                veh.Remove();
            }
        }

        [ScriptEvent(ScriptEventType.PlayerDead)]
        public async Task OnPlayerDead(IAltPlayer player, IEntity killer, uint weapon)
        {
            if (killer is not IAltPlayer killerPlayer) return;
            var spawnPointPool = player.DmMode ? Misc.AirportSpawnPositions : Misc.SpawnPositions;

            var randomSpawnPoint = spawnPointPool.ElementAt(_random.Next(0, spawnPointPool.Length));
            player.Spawn(randomSpawnPoint + new Position(_random.Next(0, 10), _random.Next(0, 10), 0));

            lock (StatsHandler.StatsData)
            {
                StatsHandler.StatsData.PlayerDeaths++;
            }

            if (!Misc.BlacklistedWeapons.Contains(weapon)) return;
            Alt.Core.LogColored(
                $"~r~ Banned Player: {killerPlayer.Name} ({killerPlayer.Id}) for using illegal weapon!");
            Misc.BannedPlayers.Add(new Tuple<ulong, ulong>(killerPlayer.HardwareIdHash, killerPlayer.HardwareIdExHash));
            string json = JsonSerializer.Serialize(Misc.BannedPlayers);
            await File.WriteAllTextAsync(@"BannedPlayers.json", json);
            killerPlayer.Kick("You're banned from this server!");
        }

        [ScriptEvent(ScriptEventType.ConsoleCommand)]
        public async Task OnConsoleCommand(string name, string[] args)
        {
            var playerPool = Alt.GetAllPlayers();
            switch (name)
            {
                case "op":
                    if (args.Length is > 1 or 0)
                    {
                        Alt.Log("Usage: op <ID>");
                        break;
                    }

                    var playerOp = playerPool.FirstOrDefault(x => x.Id == int.Parse(args[0]));
                    if (playerOp is not IAltPlayer playerOpAlt) return;


                    if (Misc.Operators.Any(tuple =>
                            tuple.Item1 == playerOpAlt.HardwareIdHash &&
                            tuple.Item2 == playerOpAlt.HardwareIdExHash))
                    {
                        Alt.Log($"Id {args[0]} already is an operator!");
                        break;
                    }

                    Misc.Operators.Add(new Tuple<ulong, ulong>(playerOpAlt.HardwareIdHash,
                        playerOpAlt.HardwareIdExHash));
                    string json = JsonSerializer.Serialize(Misc.Operators);
                    await File.WriteAllTextAsync(@"Operators.json", json);

                    await playerOpAlt.EmitAsync("set_chat_state", true);
                    playerOpAlt.IsAdmin = true;
                    break;


                case "deop":
                    if (args.Length is > 1 or 0)
                    {
                        Alt.Log("Usage: deop <ID>");
                        break;
                    }

                    var playerDeOp = playerPool.FirstOrDefault(x => x.Id == int.Parse(args[0]));
                    if (playerDeOp is not IAltPlayer playerDeOpAlt) return;

                    if (!Misc.Operators.Any(tuple =>
                            tuple.Item1 == playerDeOpAlt.HardwareIdHash &&
                            tuple.Item2 == playerDeOpAlt.HardwareIdExHash))
                    {
                        AltAsync.Log($"Id {args[0]} is not an operator!");
                        break;
                    }

                    Misc.Operators.Remove(new Tuple<ulong, ulong>(playerDeOpAlt.HardwareIdHash,
                        playerDeOpAlt.HardwareIdExHash));
                    await playerDeOpAlt.EmitAsync("set_chat_state", Misc.ChatState);
                    playerDeOpAlt.IsAdmin = false;
                    break;
            }
        }

        [ScriptEvent(ScriptEventType.WeaponDamage)]
        public async Task OnWeaponDamage(IAltPlayer player, IEntity target, uint weapon, ushort damage,
            Position shotOffset, BodyPart bodyPart)
        {
            if (!Misc.BlacklistedWeapons.Contains(weapon) || player is not { } damagePlayer) return;

            Alt.Core.LogColored(
                $"~r~ Banned Player: {damagePlayer.Name} ({damagePlayer.Id}) for using illegal weapon!");
            //Misc.BannedPlayers.Add(<ulong, ulong>(damagePlayer.HardwareIdHash, damagePlayer.HardwareIdExHash));
            Misc.BannedPlayers.Add(new Tuple<ulong, ulong>(damagePlayer.HardwareIdHash,
                damagePlayer.HardwareIdExHash));
            string json = JsonSerializer.Serialize(Misc.BannedPlayers);
            await File.WriteAllTextAsync(@"BannedPlayers.json", json);

            player.Kick("You're banned from this server!");
        }

        [ScriptEvent(ScriptEventType.ColShape)]
        public void OnColshapeEnter(IColShape colshape, IEntity target, bool state)
        {
            if (target is not IAltPlayer targetPlayer) return;

            targetPlayer.EnableWeaponUsage = state;
            targetPlayer.Emit("airport_state", state);
        }

        [ScriptEvent(ScriptEventType.Fire)]
        public bool OnFireStart(IAltPlayer player, FireInfo[] fireInfos)
        {
            return false;
        }

        [ScriptEvent(ScriptEventType.Explosion)]
        public bool OnExplosion(IAltPlayer player, ExplosionType explosionType, Position position, uint explosionFx,
            IEntity target)
        {
            return false;
        }

        [ScriptEvent(ScriptEventType.StartProjectile)]
        public bool OnProjectileStart(IAltPlayer player, Position startPosition, Position direction, uint ammoHash,
            uint weaponHash)
        {
            return false;
        }

        [ClientEvent("chat:message")]
        public void OnChatMessage(IAltPlayer player, params string[] args)
        {
            var message = string.Join("", args);
            if (args.Length == 0 || message.Length == 0) return;

            if (args[0].StartsWith("/")) return;
            if (!Misc.ChatState && !player.IsAdmin)
            {
                player.SendChatMessage("{FF0000}Chat is disabled!");
                return;
            }

            foreach (var p in Alt.GetAllPlayers())
            {
                p.SendChatMessage(
                    $"{(player.IsAdmin ? "{008736}" : "{FFFFFF}")} <b>{player.Name}({player.Id})</b>: {{FFFFFF}}{message}");
            }
        }

        [ClientEvent("tp_to_waypoint")]
        public void TeleportWaypoint(IAltPlayer player, int x, int y, int z)
        {
            if (!player.IsAdmin)
            {
                player.SendChatMessage("{FF0000} No permission!");
                return;
            }

            if (player.IsInVehicle) player.Vehicle.Position = new Vector3(x, y, z);
            else player.Position = new Vector3(x, y, z);

            player.SendChatMessage($"{{00FF00}} You were teleported to waypoint on {x}, {y}, {z}!");
        }
    }
}