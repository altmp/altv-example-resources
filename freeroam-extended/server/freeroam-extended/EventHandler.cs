using System.Numerics;
using System.Text.Json;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using AltV.Net.Resources.Chat.Api;
using Freeroam_Extended.Clothes;
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
                var altPlayer = (IAltPlayer)player;
                if (name != "chat:message" && !altPlayer.IsAdmin)
                {
                    Alt.Log($"{altPlayer.Name} banned for illegal event: {name}");
                    PlayerController.Ban(altPlayer);
                }

                if (altPlayer.EventCount > 100)
                {
                    altPlayer.Kick("Event count exceeded");
                    return;
                }
                
                altPlayer.IncrementEventCount();
            };
        }

        private readonly Random _random = new();

        [AsyncScriptEvent(ScriptEventType.PlayerConnect)]
        public async Task OnPlayerConnect(IAltPlayer player, string reason)
        {
            string cloudId = "";

            try
            {
                cloudId = await player.RequestCloudId();
            }
            catch (Exception e)
            {
                player.Kick("Authorization error");
                AltAsync.Log(
                    $"HWID: {player.HardwareIdHash}. Tried to join the server with invalid RS ID: {e}");
            }
            player.CloudID = cloudId;
            
            if (PlayerController.IsBanned(player.CloudID))
            {
                player.Kick("You're banned from this server!");
                AltAsync.Log(
                    $"RS ID: {player.CloudID}. Tried to join the server with a ban.");
                return;
            }

            if (PlayerController.PlayerData.TryGetValue(player.CloudID, out var data))
            {
                player.Data = data;
            }

            // select random entry from SpawnPoints
            var randomSpawnPoint = Misc.AdminOverridedSpawnPos is not null
                ? Misc.AdminOverridedSpawnPos
                : Misc.SpawnPositions.ElementAt(_random.Next(0, Misc.SpawnPositions.Length));
            
            player.Spawn((Position)randomSpawnPoint + new Position(_random.Next(0, 10), _random.Next(0, 10), 0));
            player.Model = (uint)PedModel.FreemodeMale01;
            player.SetDateTime(DateTime.UtcNow);
            player.SetWeather(Misc.Weather);

            player.Emit("draw_dmzone", Misc.DMPos.X, Misc.DMPos.Y, Misc.DMRadius);
            
            lock (StatsController.StatsData)
            {
                StatsController.StatsData.PlayerConnections++;
            }

            StatsController.AddUniquePlayer(player.CloudID);
            
            VoiceController.AddPlayer(player);
            if (player.Data.Muted) VoiceController.MutePlayer(player);

            if (Misc.IsResourceLoaded("c_clothesfit"))
            {
                await ClothesFitService.InitPlayer(player);
            }

            AppearanceController.RefreshFace(player);
            await AppearanceController.RefreshClothes(player);
        }

        [ScriptEvent(ScriptEventType.VehicleDestroy)]
        public void OnVehicleDestroy(IAltVehicle target)
        {
            lock (StatsController.StatsData)
            {
                StatsController.StatsData.VehiclesDestroyed++;
            }

            target.Owner.SendChatMessage("Your Vehicle got destroyed. We removed it for you!");
            target.Destroy();
        }

        [ScriptEvent(ScriptEventType.PlayerDisconnect)]
        public void OnPlayerDisconnect(IAltPlayer player, string reason)
        {
            VoiceController.RemovePlayer(player);

            var vehicles = player.Vehicles;

            foreach (var veh in vehicles)
            {
                veh.Destroy();
            }

            if (Misc.IsResourceLoaded("c_clothesfit"))
            {
                ClothesFitService.DestroyPlayer(player);
            }
        }

        [AsyncScriptEvent(ScriptEventType.PlayerDead)]
        public async Task OnPlayerDead(IAltPlayer player, IEntity killer, uint weapon)
        {
            var spawnPointPool = player.DmMode ? Misc.AirportSpawnPositions : Misc.SpawnPositions;

            var randomSpawnPoint = Misc.AdminOverridedSpawnPos ?? spawnPointPool.ElementAt(_random.Next(0, spawnPointPool.Length));
            player.Spawn(randomSpawnPoint + new Position(_random.Next(0, 10), _random.Next(0, 10), 0));

            lock (StatsController.StatsData)
            {
                StatsController.StatsData.PlayerDeaths++;
            }

            if (killer is not IAltPlayer killerPlayer) return;
            if (!Misc.BlacklistedWeapons.Contains(weapon)) return;

            var name = killerPlayer.Serialize();
            PlayerController.Ban(killerPlayer);
            ChatController.BroadcastAdmins($"Banned player {name} for using illegal weapon!");
        }

        [ScriptEvent(ScriptEventType.ConsoleCommand)]
        public void OnConsoleCommand(string name, string[] args)
        {
            switch (name)
            {
                case "op":
                {
                    if (args.Length is > 1 or 0)
                    {
                        Alt.Log("Usage: op <ID>");
                        break;
                    }

                    var player = (IAltPlayer)Alt.GetPlayerById(uint.Parse(args[0]));
                    PlayerController.Op(player, null);
                    Alt.Log("Given operator permissions to " + player.Serialize());
                    break;
                }


                case "deop":
                {
                    if (args.Length is > 1 or 0)
                    {
                        Alt.Log("Usage: deop <ID>");
                        break;
                    }

                    var player = (IAltPlayer)Alt.GetPlayerById(uint.Parse(args[0]));
                    PlayerController.Deop(player, null);
                    Alt.Log("Removed operator permissions from " + player.Serialize());
                    break;
                }
            }
        }

        [AsyncScriptEvent(ScriptEventType.WeaponDamage)]
        public async Task OnWeaponDamage(IAltPlayer player, IEntity target, uint weapon, ushort damage,
            Position shotOffset, BodyPart bodyPart)
        {
            if (!player.EnableWeaponUsage) return;
            
            if (!Misc.BlacklistedWeapons.Contains(weapon) || player is not { } damagePlayer) return;

            var name = player.Serialize();
            PlayerController.Ban(player);
            ChatController.BroadcastAdmins($"Banned player {name} for using illegal weapon!");
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

        [ClientEvent("tp_to_waypoint")]
        public void TeleportToWaypoint(IAltPlayer player, int x, int y, int z)
        {
            if (!player.IsAdmin)
            {
                player.SendChatMessage(ChatConstants.NoPermissions);
                return;
            }
            
            if (player.IsInVehicle) player.Vehicle.Position = new Vector3(x, y, z);
            else player.Position = new Vector3(x, y, z);

            player.SendChatMessage(ChatConstants.SuccessPrefix + $"You were teleported to waypoint on {x}, {y}, {z}!");
        }
        
        [ClientEvent("tp_to_coords")]
        public void TeleportToCoords(IAltPlayer player, int x, int y, int z)
        {
            if (!player.IsAdmin)
            {
                player.SendChatMessage(ChatConstants.NoPermissions);
                return;
            }

            if (player.IsInVehicle) player.Vehicle.Position = new Vector3(x, y, z);
            else player.Position = new Vector3(x, y, z);
        }
    }
}