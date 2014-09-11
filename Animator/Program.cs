#region

using System;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Animator
{
    internal class Program
    {
        private static Menu _menu;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            _menu = new Menu("Animator", "Animator", true);
            _menu.AddItem(new MenuItem("Count", "Receive Count").SetValue(new Slider(10, 2, 20)));
            _menu.AddToMainMenu();

            Game.OnGameProcessPacket += GameOnOnGameProcessPacket;
        }

        private static void GameOnOnGameProcessPacket(GamePacketEventArgs args)
        {
            if (args.PacketData[0] != 0x87)
                return;

            for (var i = 0; i < _menu.Item("Count").GetValue<Slider>().Value; i++)
                Game.ProcessPacket(args.PacketData, PacketChannel.S2C);
        }
    }
}