using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace Animator
{
    class Program
    {
        private static Menu Menu;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Menu = new Menu("Animator", "Animator", true);
            Menu.AddItem(new MenuItem("Count", "Receive Count").SetValue(10));

            Game.OnGameProcessPacket += GameOnOnGameProcessPacket;
        }

        private static void GameOnOnGameProcessPacket(GamePacketEventArgs args)
        {
            if (args.PacketData[0] != 0x87)
                return;

            for (var i = 0; i < Menu.Item("Count").GetValue<int>(); i++)
                Game.ProcessPacket(args.PacketData, PacketChannel.S2C);
        }
    }
}
