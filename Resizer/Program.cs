#region

using System;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Resizer
{
    internal class Program
    {
        private static float Size = 1;
        private static Menu _menu;
        private static int LastSequence = 1;
        private static long LastProcess;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            _menu = new Menu("Resizer", "Resizer", true);
            _menu.AddItem(
                new MenuItem("Increase", "Increase Size").SetValue(new KeyBind(0x6B, KeyBindType.Toggle)));
            _menu.AddItem(
                new MenuItem("Decrease", "Decrease Size").SetValue(new KeyBind(0x6D, KeyBindType.Toggle)));
            _menu.AddItem(new MenuItem("Count", "Count").SetValue(0f));
            _menu.AddToMainMenu();

            Game.PrintChat("Resizer loaded.");

            Game.OnGameUpdate += Game_OnGameUpdate;
            Game.OnGameSendPacket += Game_OnGameSendPacket;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            //Small Delay
            if (Environment.TickCount - LastProcess < 600)
                return;

            if (_menu.Item("Increase").GetValue<KeyBind>().Active)
            {
                var val = _menu.Item("Increase").GetValue<KeyBind>();
                val.Active = false;
                _menu.Item("Increase").SetValue(val);

                Size += .25f;
                ChangeSize();
            }
            else if (_menu.Item("Decrease").GetValue<KeyBind>().Active)
            {
                var val = _menu.Item("Decrease").GetValue<KeyBind>();
                val.Active = false;
                _menu.Item("Decrease").SetValue(val);

                Size -= .25f;
                ChangeSize();
            }
        }

        private static void Game_OnGameSendPacket(GamePacketEventArgs args)
        {
            if (args.PacketData[0] != 0xA8)
                return;

            var p = new GamePacket(args.PacketData) { Position = 5 };
            LastSequence = p.ReadInteger();
        }

        private static void ChangeSize()
        {
            var p = new GamePacket(0xC4);
            p.WriteInteger(0);
            p.WriteInteger(LastSequence);
            p.WriteByte(0x1);
            p.WriteByte(0x8);
            p.WriteInteger(ObjectManager.Player.NetworkId);
            p.WriteInteger(0x800);
            p.WriteByte(0x8);
            p.WriteFloat(Size);
            p.Process();

            LastProcess = Environment.TickCount;
        }
    }
}