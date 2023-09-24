using System.Text.Json;
using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Resources.Chat.Api;
using Freeroam_Extended.Factories;

namespace Freeroam_Extended;

public static class ChatController
{
    public static void Init()
    {
        Alt.OnClient<IAltPlayer, string[]>("chat:message", OnChatMessage);
    }
    
    public static bool ChatState = false;

    public static void Broadcast(string message)
    {
        Alt.EmitAllClients("chat:message", null, (object) message);
    }

    public static void BroadcastAdmins(string message)
    {
        message = ChatConstants.AdminPrefix + message;
        
        foreach (var p in Alt.GetAllPlayers().Where(e => ((IAltPlayer) e).IsAdmin))
        {
            p.SendChatMessage(message);
        }
    }
    
    private static void OnChatMessage(IAltPlayer player, params string[] args)
    {
        var message = string.Join("", args);
        if (args.Length == 0 || message.Length == 0) return;
        if (args[0].StartsWith("/")) return;

        if (player.Data.Muted)
        {
            player.SendChatMessage(ChatConstants.ErrorPrefix + "You are muted!");
            return;
        }
        
        if (!ChatState && !player.IsAdmin)
        {
            player.SendChatMessage(ChatConstants.ErrorPrefix + "Chat is disabled!");
            return;
        }

        foreach (var p in Alt.GetAllPlayers())
        {
            p.Emit("chat:message", (player.IsAdmin ? "{008736}" : "{FFFFFF}") + player.Serialize(), "{FFFFFF}" + message);
        }
    }

    public static void Mute(IAltPlayer player)
    {
        
    }
    
}