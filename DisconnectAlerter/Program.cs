using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace DisconnectAlerter
{
    class Program
    {
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Game.OnGameProcessPacket += Game_OnGameProcessPacket;
        }

        private static void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            if (args.PacketData[0] == Packet.S2C.PlayerDisconnect.Header)
            {

                Game.PrintChat(
                    "<b><font color=\"#FF0000\">" +
                    Packet.S2C.PlayerDisconnect.Decoded(args.PacketData).Player.ChampionName +
                    "</font></b><font color=\"#FFFFFF\" has disconnected!</font></b>");
            }
        }
    }
}
