using System.Text.Json;
using AltV.Net;
using AltV.Net.Resources.Chat.Api;
using Freeroam_Extended.Factories;

namespace Freeroam_Extended;

class VehicleListFile
{
    public bool IsWhitelist { get; set; }
    public List<string> List { get; set; }
}

public static class VehicleController
{
    private static readonly string VehicleListFile = "VehicleList.json";
    
    public static bool IsWhitelist { get; set; }
    public static string State => IsWhitelist ? "whitelist" : "blacklist";

    public static List<string> List { get; set; } = new();
    public static HashSet<uint> ListHashes { get; set; } = new();
    
    public static void Init()
    {
        IsWhitelist = false;
        List = VehicleConstants.DefaultBlacklistModels;
        
        if (File.Exists(VehicleListFile))
        {
            var json = File.ReadAllText(VehicleListFile);
            try
            {
                var data = JsonSerializer.Deserialize<VehicleListFile>(json) ?? new();
                IsWhitelist = data.IsWhitelist;
                List = data.List;
            }
            catch (Exception e)
            {
                Alt.LogError("Failed to parse vehicle list file!");
                Alt.LogError(e.ToString());
            }
        }
        
        UpdateHashes();
    }

    public static void Save()
    {
        var data = new VehicleListFile
        {
            IsWhitelist = IsWhitelist,
            List = List
        };
        File.WriteAllText(VehicleListFile, JsonSerializer.Serialize(data));
    }

    public static bool CheckVehicle(string model)
    {
        if (IsWhitelist)
        {
            return ListHashes.Contains(Alt.Hash(model));
        }
        else
        {
            return !ListHashes.Contains(Alt.Hash(model));
        }
    }

    public static bool CheckVehicle(uint model)
    {
        if (IsWhitelist)
        {
            return ListHashes.Contains(model);
        }
        else
        {
            return !ListHashes.Contains(model);
        }
    }

    private static void UpdateHashes()
    {
        ListHashes = List.Select(Alt.Hash).ToHashSet();
    }
    
    public static void UpdateVehicles()
    {
        foreach (var altVehicle in Alt.GetAllVehicles())
        {
            var vehicle = (IAltVehicle)altVehicle;
            if (vehicle.Owner is { IsAdmin: true }) continue;
            if (CheckVehicle(vehicle.Model)) continue;
            
            vehicle.Owner.SendChatMessage(ChatConstants.ErrorPrefix + "Your vehicle was removed!");
            vehicle.Destroy();
        }
    }

    public static void Allow(string model)
    {
        if (IsWhitelist)
        {
            List.Add(model);
        }
        else
        {
            List.Remove(model);
        }
        
        UpdateHashes();
        Save();
        UpdateVehicles();
    }
    
    public static void Block(string model)
    {
        if (IsWhitelist)
        {
            List.Remove(model);
        }
        else
        {
            List.Add(model);
        }
        
        UpdateHashes();
        Save();
        UpdateVehicles();
    }
    
    public static void UpdateState(bool whitelist)
    {
        IsWhitelist = whitelist;
        List = whitelist ? VehicleConstants.DefaultWhitelistModels : VehicleConstants.DefaultBlacklistModels;
        UpdateHashes();
        Save();
        UpdateVehicles();
    }
    
    public static void Clear()
    {
        List = new();
        UpdateHashes();
        Save();
        UpdateVehicles();
    }
}