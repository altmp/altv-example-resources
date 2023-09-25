using AltV.Net.Async;
using AltV.Net;
using AltV.Net.Elements.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Freeroam_Extended
{
    static internal class VoiceController
    {
        public static IVoiceChannel LocalChannel = null!;
        public static IVoiceChannel GlobalChannel = null!;

        public static void Init()
        {
            LocalChannel = Alt.CreateVoiceChannel(true, 100);
            GlobalChannel = Alt.CreateVoiceChannel(false, 0);
        }

        public static void AddPlayer(IPlayer player)
        {
            LocalChannel.AddPlayer(player);
            GlobalChannel.AddPlayer(player);

            GlobalChannel.MutePlayer(player);
        }

        public static void RemovePlayer(IPlayer player)
        {
            LocalChannel.RemovePlayer(player);
            GlobalChannel.RemovePlayer(player);

            GlobalChannel.MutePlayer(player);
        }

        public static void MutePlayer(IPlayer player)
        {
            LocalChannel.MutePlayer(player);
        }

        public static void UnmutePlayer(IPlayer player)
        {
            LocalChannel.UnmutePlayer(player);
        }

        public static void EnableGlobalVoice(IPlayer player)
        {
            GlobalChannel.UnmutePlayer(player);
        }

        public static void DisableGlobalVoice(IPlayer player)
        {
            GlobalChannel.MutePlayer(player);
        }

        public static bool IsGlobalVoiceEnabled(IPlayer player)
        {
            return GlobalChannel.IsPlayerMuted(player);
        }
    }
}
