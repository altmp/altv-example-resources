using System.Collections.Generic;
using AltV.Net.Data;

namespace Freeroam_Extended.Misc
{
    public static class Misc
    {
        public static IList<uint> BlacklistedWeapons = new List<uint>
        {
            125959754, // Compact Grenade Launcher
            2726580491, // Grenade Launcher
            1672152130, // Homing Launcher
            1834241177, // Railgun
            2982836145, // Rocket Launcher
            2481070269, // Grenade
            3125143736, // Pipe Bomb
            2874559379, // Proximity Mine
            741814745, // Sticky Bomb
        };

        public static IList<Position> SpawnPositions = new List<Position>
        {
            new Position(-386.86154f, -1832.123f, 21.596313f), // Arena
            new Position(-1204.8923f, -3090.844f, 14.182373f), // Airport runway
            new Position(-1010.7165f, -2717.789f, 13.693726f), // Airport outside
        };
        
        public static IList<ulong> BannedPlayers = new List<ulong>
        {
            // Add banned players here
        };
        
        public static IList<int> Operators = new List<int>
        {
            // Add operators here
        };
        
    }
}