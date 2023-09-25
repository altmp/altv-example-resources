﻿using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Enums;

namespace Freeroam_Extended
{
    public static class Misc
    {
        public static Random random = new Random();
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

        public static HashSet<uint> WhitelistedWeapons = new()
        {
            94989220,
            100416529,
            137902532,
            171789620,
            177293209,
            205991906,
            317205821,
            324215364,
            419712736,
            453432689,
            487013001,
            584646201,
            736523883,
            911657153,
            940833800,
            961495388,
            984333226,
            1141786504,
            1305664598,
            1317494643,
            1432025498,
            1470379660,
            1593441988,
            1627465347,
            1649403952,
            1737195953,
            1785463520,
            2017895192,
            2024373456,
            2132975508,
            2144741730,
            2210333304,
            2227010557,
            2228681469,
            2285322324,
            2343591895,
            2460120199,
            2484171525,
            2508868239,
            2526821735,
            2548703416,
            2578377531,
            2578778090,
            2634544996,
            2636060646,
            2640438543,
            2725352035,
            2828843422,
            2937143193,
            3173288789,
            3218215474,
            3219281620,
            3220176749,
            3231910285,
            3249783761,
            3342088282,
            3415619887,
            3441901897,
            3523564046,
            3638508604,
            3675956304,
            3686625920,
            3696079510,
            3713923289,
            3756226112,
            3800352039,
            4019527611,
            4024951519,
            4191993645,
            4192643659,
            4208062921,
            4222310262
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
            new (102.63296f,-1939.50329f, 20.7964f),        // Groove Street
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
            new (1958.4264f, 3722.1626f, 32.363403f),
            new (2026.6285f, 4756.3647f, 41.041016f),
            new (150.73846f, 6424.958f, 31.285034f),
            new (-1997.4198f, 3073.0945f, 32.801514f),
        };
        
        public static readonly Position[] AirportSpawnPositions = {
            new (-1100.89990234375f, -2659.896240234375f, 13.756650924682617f),
            new (-960.344970703125f, -2753.627685546875f, 13.83371639251709f),
            new (-964.8075561523438f, -3002.284912109375f, 13.945064544677734f),
            new (-1776.1871337890625f, -2773.560546875f, 13.944681167602539f),
            new (-1216.00244140625f, -2799.224609375f, 13.945316314697266f),
            new (-1276.5977783203125f, -3385.822021484375f, 13.940142631530762f),
            new (-1655.9744873046875f, -3149.0458984375f, 13.985773086547852f),
            new (-1460.2476806640625f, -3307.091552734375f, 13.945180892944336f),
            new (-1319.1428f, -3274.2197f, 13.23877f),
            new (-1461.8638f, -3203.8682f, 13.23877f),
            new (-1754.6505f, -3078.1187f, 13.542114f),
            new (-1697.0374f, -2894.2944f, 13.575806f),
            new (-1554.2373f, -2664.356f, 14.064453f),
            new (-1212.9495f, -2572.1538f, 13.23877f),
            new (-1195.0022f, -2274.6858f, 13.23877f),
            new (-958.0615f, -2596.8396f, 13.154541f),
            new (-892.89233f, -2729.5254f, 13.12085f),
            new (-807.5604f, -2664.6594f, 13.104004f),
            new (-675.2044f, -2378.4658f, 13.087158f), 
        };

        public static int Hour = 11;
        public static uint Weather = 0;

        public static Position DMPos = new Position(-1216.839599609375f, -2832.514404296875f, 13.9296875f);
        public static int DMRadius = 800;

        public static Position? AdminOverridedSpawnPos = null;

        public static bool IsResourceLoaded(string resourceName)
        {
            var allResources = Alt.GetAllResources();
            return allResources.Count(x => x.Name == resourceName) > 0;
        }

        public static int RandomInt(int min, int max)
        {
            int randomNumber = random.Next(min, max + 1);
            return randomNumber;
        }
    }
}