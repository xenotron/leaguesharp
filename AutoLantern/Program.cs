#region

using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Network.Packets;

#endregion

namespace AutoLantern
{
    internal class Program
    {
        private const String Lantern = "ThreshLantern";
        private static Menu _menu;
        private static Obj_AI_Hero _player;
        private static GameObject _threshLantern;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad(EventArgs args)
        {
            if (!ThreshInGame())
            {
                return;
            }

            _menu = new Menu("AutoLantern", "AutoLantern", true);
            _menu.AddItem(new MenuItem("Auto", "Auto-Lantern at Low HP").SetValue(true));
            _menu.AddItem(new MenuItem("Low", "Low HP Percent").SetValue(new Slider(20, 30, 5)));
            _menu.AddItem(new MenuItem("Hotkey", "Hotkey").SetValue(new KeyBind(32, KeyBindType.Press, false)));
            _menu.AddToMainMenu();

            Game.OnGameUpdate += OnGameUpdate;
            GameObject.OnCreate += OnMinionCreation;
            GameObject.OnDelete += OnMinionDeletion;

            Game.PrintChat("AutoLantern by Trees loaded.");
            _player = ObjectManager.Player;
        }

        private static void OnGameUpdate(EventArgs args)
        {
            if (IsValid(_threshLantern) &&
                ((IsLow() && _menu.Item("Auto").GetValue<bool>()) || (_menu.Item("Hotkey").GetValue<KeyBind>().Active)))
            {
                InteractObject(_threshLantern);
            }
        }

        private static void OnMinionCreation(GameObject obj, EventArgs args)
        {
            if (obj != null && obj.IsValid && obj.IsAlly && obj.Name.Contains(Lantern))
            {
                _threshLantern = obj;
            }
        }

        private static void OnMinionDeletion(GameObject obj, EventArgs args)
        {
            if (obj != null && obj.IsValid && obj.IsAlly && obj.Name.Contains(Lantern))
            {
                _threshLantern = null;
            }
        }

        private static bool IsLow()
        {
            return _player.Health < _player.MaxHealth * _menu.Item("Low").GetValue<Slider>().Value / 100;
        }

        private static bool IsValid(GameObject lant)
        {
            return lant != null && lant.IsValid && _player.ServerPosition.Distance(lant.Position) <= 500;
        }

        private static bool ThreshInGame()
        {
            return ObjectManager.Get<Obj_AI_Hero>().Any(h => h.IsAlly && !h.IsMe && h.ChampionName == "Thresh");
        }

        private static void InteractObject(GameObject obj)
        {
            var p = new PKT_InteractReq { NetworkId = _player.NetworkId, TargetNetworkId = obj.NetworkId };
            Game.SendPacket(p.Encode(), PacketChannel.C2S, PacketProtocolFlags.Reliable);
        }
    }
}