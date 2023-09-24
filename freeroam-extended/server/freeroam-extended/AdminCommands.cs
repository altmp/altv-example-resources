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

namespace Freeroam_Extended
{
    public class AdminCommands : IScript
    {
        private void SendVehListHelp(IAltPlayer player)
        {
            player.SendChatMessage("/vehlist mode [blacklist,whitelist] - change vehicle list mode");
            player.SendChatMessage("/vehlist allow [vehicle model] - allow vehicle (add to whitelist or remove from blacklist)");
            player.SendChatMessage("/vehlist block [vehicle model] - block vehicle (add to blacklist or remove from whitelist)");
            player.SendChatMessage("/vehlist clear - remove all vehicles from list");
            player.SendChatMessage("/vehlist status - show status and vehicle list");
        }

        [Command("vehlist")]
        public void VehList(IAltPlayer player, params string[] args)
        {
            if (!player.IsAdmin)
            {
                player.SendChatMessage(ChatConstants.NoPermissions);
                return;
            }
            
            if (args.Length < 1)
            {
                SendVehListHelp(player);
                return;
            }

            switch (args[0])
            {
                case "mode" when args.Length > 1 && args[1] == "blacklist":
                    VehicleController.UpdateState(false);
                    ChatController.BroadcastAdmins($"Vehicle list was changed to blacklist by {player.Serialize()}");
                    break;
                
                case "mode" when args.Length > 1 && args[1] == "whitelist":
                    VehicleController.UpdateState(true);
                    ChatController.BroadcastAdmins($"Vehicle list was changed to whitelist by {player.Serialize()}");
                    break;
                
                case "allow" when args.Length > 1:
                    if (!Enum.IsDefined(typeof(VehicleModel), Alt.Hash(args[1])))
                    {
                        player.SendChatMessage(ChatConstants.ErrorPrefix + "Invalid vehicle model!");
                        return;
                    }
                    VehicleController.Allow(args[1]);
                    ChatController.BroadcastAdmins($"Vehicle {args[1]} was {(VehicleController.IsWhitelist ? "added to whitelist" : "removed from blacklist")} by {player.Serialize()}");
                    break;
                
                case "block" when args.Length > 1:
                    if (!Enum.IsDefined(typeof(VehicleModel), Alt.Hash(args[1])))
                    {
                        player.SendChatMessage(ChatConstants.ErrorPrefix + "Invalid vehicle model!");
                        return;
                    }
                    VehicleController.Block(args[1]);
                    ChatController.BroadcastAdmins($"Vehicle {args[1]} was {(VehicleController.IsWhitelist ? "removed from whitelist" : "added to blacklist")} by {player.Serialize()}");
                    break;
                
                case "clear":
                    VehicleController.Clear();
                    ChatController.BroadcastAdmins($"Vehicle list was cleared by {player.Serialize()}");
                    break;
                
                case "list":
                    player.SendChatMessage($"{(VehicleController.IsWhitelist ? "Whitelisted" : "Blacklisted")} vehicles: {string.Join(", ", VehicleController.List)}");
                    break;
                
                default:    
                    SendVehListHelp(player);
                    break;
            }
        }
        
        #region Punishments
        [Command("ban")]
        public void Ban(IAltPlayer player, int id)
        {
            if (!player.IsAdmin)
            {
                player.SendChatMessage(ChatConstants.NoPermissions);
                return;
            }

            if (player.Id == id)
            {
                player.SendChatMessage(ChatConstants.ErrorPrefix + "You can't ban yourself!");
                return;
            }

            if (Alt.GetPlayerById((uint) id) is not IAltPlayer target)
            {
                player.SendChatMessage(ChatConstants.ErrorPrefix + $"Player with id {id} not found!");
                return;
            }

            var name = target.Serialize();
            PlayerController.Ban(target);
            ChatController.BroadcastAdmins($"Player {name} was banned by {player.Serialize()}!");
        }
        
        [Command("kick")]
        public void Kick(IAltPlayer player, int id)
        {
            if (!player.IsAdmin)
            {
                player.SendChatMessage(ChatConstants.NoPermissions);
                return;
            }

            if (player.Id == id)
            {
                player.SendChatMessage(ChatConstants.ErrorPrefix + "You can't kick yourself!");
                return;
            }

            var target = (IAltPlayer) Alt.GetPlayerById((uint) id);
            if (target == null)
            {
                player.SendChatMessage(ChatConstants.ErrorPrefix + $"Player with id {id} not found!");
                return;
            }

            var name = target.Serialize();
            target.Kick("You've been kicked from this server!");

            ChatController.BroadcastAdmins($"Player {name} was kicked by {player.Serialize()}!");
        }
        
        [Command("mutelist")]
        public void Mutelist(IAltPlayer player)
        {
            if (!player.IsAdmin)
            {
                player.SendChatMessage(ChatConstants.NoPermissions);
                return;
            }

            var found = new HashSet<string>();
            var muted = new List<string>();
            foreach (var p in Alt.GetAllPlayers())
            {
                if (p is not IAltPlayer target || !target.Data.Muted) continue;
                muted.Add(target.CloudID + " - " + target.Serialize());
                found.Add(target.CloudID);
            }
            
            foreach (var (key, value) in PlayerController.PlayerData)
            {
                if (!value.Muted) continue;
                if (found.Contains(key)) continue;
                muted.Add(key + " - Offline");
            }
            
            player.SendChatMessage("Muted players: " + string.Join(", ", muted));
        }
        
        [Command("mute")]
        public void Mute(IAltPlayer player, int id)
        {
            if (!player.IsAdmin)
            {
                player.SendChatMessage(ChatConstants.NoPermissions);
                return;
            }

            if (player.Id == id)
            {
                player.SendChatMessage(ChatConstants.ErrorPrefix + "You can't mute yourself!");
                return;
            }

            var target = (IAltPlayer) Alt.GetPlayerById((uint) id);
            if (target == null)
            {
                player.SendChatMessage(ChatConstants.ErrorPrefix + $"Player with id {id} not found!");
                return;
            }
            PlayerController.Mute(target, player);
        }
        
        [Command("unmute")]
        public void Unmute(IAltPlayer player, int id)
        {
            if (!player.IsAdmin)
            {
                player.SendChatMessage(ChatConstants.NoPermissions);
                return;
            }

            var target = (IAltPlayer) Alt.GetPlayerById((uint) id);
            if (target == null)
            {
                player.SendChatMessage(ChatConstants.ErrorPrefix + $"Player with id {id} not found!");
                return;
            }
            PlayerController.Unmute(target, player);
        }
        #endregion

        #region Teleports
        [Command("dimension")]
        public void Dimension(IAltPlayer player, int dimension = 0)
        {
            if (!player.IsAdmin)
            {
                player.SendChatMessage(ChatConstants.NoPermissions);
                return;
            }

            player.Dimension = dimension;
        }
        
        [Command("tpallhere")]
        public void TpAllHere(IAltPlayer player)
        {
            if (!player.IsAdmin)
            {
                player.SendChatMessage(ChatConstants.NoPermissions);
                return;
            }

            foreach (var p in Alt.GetAllPlayers())
            {
                if (p is not IAltPlayer target || p.Id == player.Id || target.IsAdmin) continue;
                target.Position = player.Position;
                target.SendChatMessage(ChatConstants.SuccessPrefix + "You were teleported to " + player.Serialize() + "!");
            }
        }

        [Command("tphere")]
        public void TpHere(IAltPlayer player, int target)
        {
            if (!player.IsAdmin)
            {
                player.SendChatMessage(ChatConstants.NoPermissions);
                return;
            }
            
            var targetPlayer = (IAltPlayer)Alt.GetPlayerById((uint) target);
            if (targetPlayer == null)
            {
                player.SendChatMessage(ChatConstants.ErrorPrefix + "Player not found!");
                return;
            }

            targetPlayer.Position = player.Position;
            targetPlayer.SendChatMessage(ChatConstants.SuccessPrefix + "You were teleported to " + player.Serialize() + "!");
        }

        [Command("tpto")]
        public void TpTo(IAltPlayer player, int target)
        {
            if (!player.IsAdmin)
            {
                player.SendChatMessage(ChatConstants.NoPermissions);
                return;
            }

            var targetPlayer = (IAltPlayer)Alt.GetPlayerById((uint) target);
            if (targetPlayer == null)
            {
                player.SendChatMessage(ChatConstants.ErrorPrefix + "Player not found!");
                return;
            }

            player.Position = targetPlayer.Position;
            player.SendChatMessage(ChatConstants.SuccessPrefix + "You were teleported to " + targetPlayer.Serialize() + "!");
        }
        
        [Command("tpcoords")]
        public void TpCoords(IAltPlayer player, int x, int y, int z)
        {
            if (!player.IsAdmin)
            {
                player.SendChatMessage(ChatConstants.NoPermissions);
                return;
            }

            player.Position = new Vector3(x, y, z);
            player.SendChatMessage(ChatConstants.SuccessPrefix + $"You were teleported to {x}, {y}, {z}!");
        }
        #endregion
        
        #region World state
        [Command("settime")]
        public void SetTime(IAltPlayer player, int hour)
        {
            if (!player.IsAdmin)
            {
                player.SendChatMessage(ChatConstants.NoPermissions);
                return;
            }

            if (hour > 23 || hour < 0)
            {
                player.SendChatMessage(ChatConstants.ErrorPrefix + "Invalid hour!");
                return;
            }

            foreach (var p in Alt.GetAllPlayers())
            {
                p.SetDateTime(0, 0, 0, hour, 0, 0);
            }
            
            ChatController.BroadcastAdmins($"{player.Serialize()} set time to {hour}");
        }
        
        [Command("togglechat")]
        public void ToggleChat(IAltPlayer player)
        {
            if (!player.IsAdmin)
            {
                player.SendChatMessage(ChatConstants.NoPermissions);
                return;
            }
            
            ChatController.ChatState = !ChatController.ChatState;
            player.SendChatMessage(ChatConstants.SuccessPrefix + "Chat is now " + (ChatController.ChatState ? "enabled" : "disabled") + "!");
        }
        
        [Command("overridespawnpos")]
        public void OverrideSpawnPos(IAltPlayer player, bool mode)
        {
            if (!player.IsAdmin)
            {
                player.SendChatMessage(ChatConstants.NoPermissions);
                return;
            }

            if (mode)
            {
                var pos = player.Position;
                Misc.AdminOverridedSpawnPos = pos;

                player.SendChatMessage(
                    ChatConstants.SuccessPrefix + $"You overrode spawn position for all players on {pos.X}, {pos.Y}, {pos.Z}!");
            }
            else
            {
                Misc.AdminOverridedSpawnPos = null;
                player.SendChatMessage(ChatConstants.ErrorPrefix + $"You reset overridden spawn position!");
            }
        }
        
        [Command("godmodeall")]
        public void GodmodeAllPlayers(IAltPlayer player, bool mode)
        {
            if (!player.IsAdmin)
            {
                player.SendChatMessage(ChatConstants.NoPermissions);
                return;
            }

            var targets = Alt.GetAllPlayers().ToList();

            foreach (var target in targets)
            {
                target.SetLocalMetaData("godmode", mode);
                target.SendChatMessage(ChatConstants.SuccessPrefix + $"Godmode for all players is {(mode ? "activated" : "deactivated")}!");
            }
        }
        #endregion

        #region Local state
        [Command("noclip")]
        public void NoClip(IAltPlayer player)
        {
            if (!player.IsAdmin)
            {
                player.SendChatMessage(ChatConstants.NoPermissions);
                return;
            }

            player.NoClip = !player.NoClip;
            player.Streamed = !player.NoClip;
            player.Visible = !player.NoClip;
            player.SendChatMessage(ChatConstants.SuccessPrefix + $"NoClip is now {(player.NoClip ? "enabled" : "disabled")}!");

            player.Emit("noclip", player.NoClip);
        }
        
        [Command("godmode")]
        public void Godmode(IAltPlayer player, int id = 0)
        {
            if (!player.IsAdmin)
            {
                player.SendChatMessage(ChatConstants.NoPermissions);
                return;
            }

            var target = id == 0 ? player : Alt.GetPlayerById((uint) id);
            if (target == null)
            {
                player.SendChatMessage(ChatConstants.ErrorPrefix + $"Player with id {id} not found!");
                return;
            }

            var hadValue = target.GetLocalMetaData("godmode", out bool prevValue);
            var newValue = !hadValue || !prevValue;
            target.SetLocalMetaData("godmode", newValue);
            target.Invincible = !target.Invincible;
            var msg = ChatConstants.SuccessPrefix + $"Godmode {(newValue ? "on" : "off")}!";
            target.SendChatMessage(msg);

            if (player.Id != target.Id)
                player.SendChatMessage(msg);
        }
        
        // [Command("esp")]
        // public void Esp(IAltPlayer player, bool mode)
        // {
        //     if (!player.IsAdmin)
        //     {
        //         player.SendChatMessage(ChatConstants.NoPermissions);
        //         return;
        //     }
        //
        //     player.Emit("esp", mode);
        // }
        
        [Command("globalvoice")]
        public void GlobalVoice(IAltPlayer player)
        {
            if (!player.IsAdmin)
            {
                player.SendChatMessage(ChatConstants.NoPermissions);
                return;
            }

            if (VoiceController.IsGlobalVoiceEnabled(player))
            {
                VoiceController.EnableGlobalVoice(player);
                player.SendChatMessage(ChatConstants.SuccessPrefix + "Global voice enabled!");
                return;
            }
            else
            {
                VoiceController.DisableGlobalVoice(player);
                player.SendChatMessage(ChatConstants.ErrorPrefix + "Global voice disabled!");
                return;
            }
        }
        #endregion

        #region Actions
        [Command("announce")]
        public void Announce(IAltPlayer player, string header, int time, params string[] body)
        {
            if (!player.IsAdmin)
            {
                player.SendChatMessage(ChatConstants.NoPermissions);
                return;
            }

            var message = string.Join(" ", body);
            Alt.EmitAllClients("announce", header.Replace("_", " "), message, time);
        }
        
        [Command("clearallvehicles")]
        public void ClearAllVehicles(IAltPlayer player, int distance = 0)
        {
            if (!player.IsAdmin)
            {
                player.SendChatMessage(ChatConstants.NoPermissions);
                return;
            }
            
            var distSqr = distance * distance;
            foreach (var altVehicle in Alt.GetAllVehicles())
            {
                var veh = (IAltVehicle) altVehicle;
                if (distance == 0 || Vector3.DistanceSquared(veh.Position, player.Position) <= distSqr)
                {
                    if (veh.Owner is { Exists: true })
                    {
                        veh.Owner.SendChatMessage(ChatConstants.ErrorPrefix + "Your vehicle was removed!");
                    }
                    
                    veh.Destroy();
                }
            }
        }
        #endregion
    }
}