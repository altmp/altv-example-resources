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
            
        };
    }
}