using System;
using System.Collections.Generic;
using AltV.Net.Data;

namespace Freeroam_Extended
{
    public static class Misc
    {
        public static HashSet<uint> BlacklistedWeapons = new()
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
            new (-1734.69885f,-1108.47033f, 14.05346f ),    // Pier
            new (-2162.94067f, -398.45275f,14.373657f),     // Parking Lot at the beach-highway
            new (-1687.70104f,-311.49890f,52.63952f),       // Church
            new (-1304.20214f,111.66593f,57.55969f),        // Golf Club
            new (-542.14947f,252.72528f, 84.04760f),        // intercept at tequilala bar
            new (-81.27033f,-611.86810f, 37.30627f),        // Arcadius
            new (165.87692f,-986.98022f, 31.08862f ),       // Good old lesion square
            new (402.21099f,-981.62634f, 30.39782f),        // LSPD Mission Row
            new (6.14505f,-1749.01977f, 30.29675f),         // Mega Mall near Groove Street
            new (102.63296f,-1939.50329f,-1939.50329f),     // Groove Street
            new (-279.12527f,-2579.48559f, 6.99340f),       // Harbour
            new (883.93848f, -43.96484f, 79.75098f),        // Casino parkin lot
            new (660.51428f, 29.47253f, 86.37292f),         // Vinewood PD
            new (67.49011f, -726.80438f, 45.20874f),        // FIB Tower
            new (-633.78461f, -1297.49011f, 11.66077f),     // La puerta intersect
            new (684.59338f, 577.60876f, 131.44617f),       // Theatre
            new (-75.01978f, -1084.23291f, 27.81982f),      // motorsports car dealer
            new (257.82858f, -574.00879f, 44.29895f),       // Pillbox hospital
            new (-1092.71204f, -402.59341f, 37.62634f),     // TV Show Production thing
            new (-926.16266f, 295.51648f, 71.86523f),       // little park in Rockford Hills
            new (-410.33408f, 1178.50549f, 326.63440f),     // Observatory
            new (-1732.06152f, 159.11209f, 65.36121f),      // Sports field
        };
        
        public static readonly Position[] AirportSpawnPositions = {
           
        };

        public static HashSet<Tuple<ulong, ulong>> BannedPlayers = new()
        {
            
        };
        
        public static HashSet<int> Operators = new()
        {
            
        };
        
        public static bool ChatState = false;
        public static int Hour = 11;
        public static uint Weather = 0;

    }
}