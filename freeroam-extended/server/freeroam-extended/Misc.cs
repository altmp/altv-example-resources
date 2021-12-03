using System;
using System.Collections.Generic;
using AltV.Net.Data;

namespace Freeroam_Extended
{
    public static class Misc
    {
        public static readonly IList<uint> BlacklistedWeapons = new List<uint>
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
            1119849093, // Minigun
            2138347493, // Firework
            3056410471, // Widowmaker
            615608432, // Molotov
            883325847, // Jerry Can
            2939590305, // Up n Atomizer
            1198256469, // UnholyHellbringer    
            4256991824, // Teargas
            2694266206, // BZ-Gas
            126349499, // Snowballs
            101631238, // Fire Extinguisher
            600439132, // Baseball
            1233104067, // Flare
            1198879012, // Flaregun
        };

        public static readonly Position[] SpawnPositions = {
            new(-386.86154f, -1832.123f, 21.596313f), // Arena
            new(-1204.8923f, -3090.844f, 14.182373f), // Airport runway
            new(-1010.7165f, -2717.789f, 13.693726f) // Airport outside
        };

        public static HashSet<Tuple<ulong, ulong>> BannedPlayers = new()
        {
            
        };
        
        public static HashSet<int> Operators = new()
        {
            
        };
        
    }
}