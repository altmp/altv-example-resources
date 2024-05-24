using System.Text.Json;
using AltV.Net;
using AltV.Net.Elements.Entities;
using Freeroam_Extended.Factories;

namespace Freeroam_Extended;

public static class PlayerController
{
    private static readonly string BannedPlayersFile = "BannedPlayers.json";
    private static readonly string PlayerDataFile = "PlayersData.json";

    public static HashSet<string> Banned = new();
    public static Dictionary<string, PlayerData> PlayerData = new();

    public static void Init()
    {
        if (File.Exists(PlayerDataFile))
        {
            var json = File.ReadAllText(PlayerDataFile);
            try
            {
                PlayerData = JsonSerializer.Deserialize<Dictionary<string, PlayerData>>(json) ?? new();
            }
            catch (Exception e)
            {
                Alt.LogError("Failed to parse operators file!");
                Alt.LogError(e.ToString());
            }
        }

        if (File.Exists(BannedPlayersFile))
        {
            var json = File.ReadAllText(BannedPlayersFile);
            try
            {
                Banned = JsonSerializer.Deserialize<HashSet<string>>(json) ?? new HashSet<string>();
            }
            catch (Exception e)
            {
                Alt.LogError("Failed to parse banned players file!");
                Alt.LogError(e.ToString());
            }
        }
    }

    private static void SaveBanned()
    {
        File.WriteAllText(BannedPlayersFile, JsonSerializer.Serialize(Banned));
    }

    public static void Ban(IAltPlayer player)
    {
        player.Kick("You are banned!");
        Banned.Add(player.CloudId);
        SaveBanned();
    }

    public static bool IsBanned(string cloudId)
    {
        return Banned.Contains(cloudId);
    }

    private static void SavePlayerData()
    {
        File.WriteAllText(PlayerDataFile, JsonSerializer.Serialize(PlayerData));
    }

    private static void ApplyPlayerData(IAltPlayer player)
    {
        PlayerData[player.CloudId] = player.Data;
        player.SetLocalMetaData("operator", player.Data.Operator);
        if (player.Data.Muted) VoiceController.MutePlayer(player);
        else VoiceController.UnmutePlayer(player);
        SavePlayerData();
    }

    public static void Op(IAltPlayer target, IAltPlayer? executor)
    {
        target.Data.Operator = true;
        ApplyPlayerData(target);
        ChatController.BroadcastAdmins((executor?.Serialize() ?? "SERVER") + " gave " + target.Serialize() + " operator permissions");
    }

    public static void Deop(IAltPlayer target, IAltPlayer? executor)
    {
        target.Data.Operator = false;
        ApplyPlayerData(target);
        ChatController.BroadcastAdmins((executor?.Serialize() ?? "SERVER") + " removed " + target.Serialize() + " operator permissions");
    }

    public static void Mute(IAltPlayer target, IAltPlayer? executor)
    {
        target.Data.Muted = true;
        ApplyPlayerData(target);
        ChatController.BroadcastAdmins((executor?.Serialize() ?? "SERVER") + " muted " + target.Serialize());
    }

    public static void Unmute(IAltPlayer target, IAltPlayer? executor)
    {
        target.Data.Muted = false;
        ApplyPlayerData(target);
        ChatController.BroadcastAdmins((executor?.Serialize() ?? "SERVER") + " unmuted " + target.Serialize());
    }
}