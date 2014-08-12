using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;


namespace AutoLantern
{
    class Program
    {
        private static Menu Menu;
        private static Obj_AI_Hero Player;
        private static GameObject ThreshLantern;
        private const String lantern = "ThreshLantern";

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad(EventArgs args)
        {
            Menu = new Menu("AutoLantern", "AutoLantern", true);
            Menu.AddItem(new MenuItem("Auto", "Auto-Lantern at Low HP").SetValue(true));
            Menu.AddItem(new MenuItem("Low", "Low HP Percent").SetValue(new Slider(20, 30, 5)));
            Menu.AddItem(new MenuItem("Hotkey", "Hotkey").SetValue(new KeyBind(32, KeyBindType.Press, false)));
            Menu.AddToMainMenu();
            
            Game.OnGameUpdate += OnGameUpdate;
            Obj_AI_Minion.OnCreate += OnMinionCreation;
            Obj_AI_Minion.OnDelete += OnMinionDeletion;
            
            Game.PrintChat("AutoLantern by Trees loaded.");
            Player = ObjectManager.Player;
        }

        private static void OnGameUpdate(EventArgs args)
        {
            if (IsValid(ThreshLantern) && (IsLow() || (Menu.Item("Hotkey").GetValue<bool>())))
                InteractObject(ThreshLantern);
        }

        private static void OnMinionCreation(GameObject obj, EventArgs args)
        {
            if (obj != null && obj.IsValid && obj.IsAlly && obj.Name.Contains(lantern))
                ThreshLantern = obj;
        }

        private static void OnMinionDeletion(GameObject obj, EventArgs args)
        {
            if (obj != null && obj.IsValid && obj.IsAlly && obj.Name.Contains(lantern))
                ThreshLantern = null;
        }

        private static bool IsLow()
        {
            return Player.Health < Player.MaxHealth * Menu.Item("Low").GetValue<int>() / 100);
        }

        private static bool IsValid(GameObject lant)
        {
            return lant != null && lant.IsValid && Vector3.Distance(Player.ServerPosition, lant.Position) <= 300);
        }

        private static void InteractObject(GameObject obj)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(memoryStream))
                {
                    binaryWriter.Write((byte)0x3A);
                    binaryWriter.Write((int)Player.NetworkId);
                    binaryWriter.Write((int)obj.NetworkId);
                    Game.SendPacket(memoryStream.ToArray(), PacketChannel.C2S, PacketProtocolFlags.NoFlags);
                }
            }
        }


    }
}
