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
    static internal class Voice
    {
        static IVoiceChannel localChannel;
        static IVoiceChannel globalChannel;

        public static void Init()
        {
            localChannel = Alt.CreateVoiceChannel(true, 100);
            globalChannel = Alt.CreateVoiceChannel(false, 0);
        }

        public static void AddPlayer(IPlayer player)
        {
            localChannel.AddPlayer(player);
            globalChannel.AddPlayer(player);

            globalChannel.MutePlayer(player);
        }

        public static void RemovePlayer(IPlayer player)
        {
            localChannel.RemovePlayer(player);
            globalChannel.RemovePlayer(player);

            globalChannel.MutePlayer(player);
        }

        public static void EnableGlobalVoice(IPlayer player)
        {
            globalChannel.UnmutePlayer(player);
        }

        public static void DisableGlobalVoice(IPlayer player)
        {
            globalChannel.MutePlayer(player);
        }

        public static bool IsGlobalVoiceEnabled(IPlayer player)
        {
            return globalChannel.IsPlayerMuted(player);
        }
    }
}
