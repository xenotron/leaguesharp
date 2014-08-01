using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

using LeagueSharp;
using SharpDX;

namespace UnitTest
{
    class Program
    {
        private static Obj_AI_Hero Player;
        private static GameObject ThreshLantern;
        private static string lantern = "ThreshLantern";
        private static int hotkey = 32; //Spacebar
        private static bool hotkeyPressed;
        private static int HPPercent = 20; //IsLow() HP percent

        [STAThread]
        static void Main(string[] args)
        {
            Game.OnGameStart += OnGameStart;
            Game.OnGameUpdate += OnGameUpdate;
            Obj_AI_Minion.OnCreate += OnMinionCreation;
            Obj_AI_Minion.OnDelete += OnMinionDeletion;
            Game.OnWndProc += OnWndProc;
        }

        static void OnGameStart(EventArgs args)
        {
            Game.PrintChat("AutoLantern by Trees loaded.");
            Player = ObjectManager.Player;
        }

        private static void OnGameUpdate(EventArgs args)
        {
            if (IsValid(ThreshLantern) && (IsLow() || hotkeyPressed))
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

        private static void OnWndProc(WndEventArgs args)
        {
            if (args.Msg == hotkey)
            {
                if (args.WParam == 0x0100)
                    hotkeyPressed = true;
                else
                    hotkeyPressed = false;
            }
        }

        private static bool IsLow()
        {
            if (Player.Health < Player.MaxHealth * HPPercent / 100)
                return true;
            return false;
        }

        private static bool IsValid(GameObject lant)
        {
            if (lant != null && lant.IsValid && Vector3.Distance(ObjectManager.Player.ServerPosition, lant.Position) <= 300)
                return true;
            return false;
        }

        private static void InteractObject(GameObject obj)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(memoryStream))
                {
                    binaryWriter.Write((byte)0x3A);
                    binaryWriter.Write((int)ObjectManager.Player.NetworkId);
                    binaryWriter.Write((int)obj.NetworkId);
                    Game.SendPacket(memoryStream.ToArray(), PacketChannel.C2S, PacketProtocolFlags.NoFlags);
                }
            }
        }


    }
}
