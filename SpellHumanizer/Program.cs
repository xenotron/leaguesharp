#region

using System;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace SpellHumanizer
{
    internal class Program
    {
        public static Menu Menu;
        public static float LastSent = 0;

        public static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Menu = new Menu("SpellHumanizer", "SpellHumanizer", true);
            Menu.AddItem(new MenuItem("Enabled", "Enabled").SetValue(true));
            Menu.AddItem(new MenuItem("Debug", "Debug").SetValue(false));
            Menu.AddItem(new MenuItem("CameraPacket", "Delay Send Camera Packet").SetValue(true));
            Menu.AddToMainMenu();

            Game.OnGameSendPacket += Game_OnGameSendPacket;
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        private static void Game_OnGameSendPacket(GamePacketEventArgs args)
        {
            if (Menu.Item("CameraPacket").GetValue<bool>() && args.PacketData[0] == 0x81 &&
                (Environment.TickCount - LastSent) < 100)
            {
                args.Process = false;
            }

            LastSent = Environment.TickCount;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            LeagueSharp.Common.SpellHumanizer.Enabled = Menu.Item("Enabled").GetValue<bool>();
            LeagueSharp.Common.SpellHumanizer.Debug = Menu.Item("Debug").GetValue<bool>();
        }
    }
}