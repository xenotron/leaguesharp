#region

using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace OathswornCaster
{
    internal class Program
    {
        public static int State = 0;
        public static Spell Oathsworn;
        public static Menu Menu;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (!KalistaInGame())
            {
                return;
            }

            Oathsworn = new Spell((SpellSlot) 0x3C, 300);
            Oathsworn.SetSkillshot(.862f, 20, float.MaxValue, false, SkillshotType.SkillshotLine);

            Menu = new Menu("OathswornCaster", "OathswornCaster", true);
            Menu.AddItem(new MenuItem("Enabled", "Enabled").SetValue(new KeyBind(32, KeyBindType.Press)));
            Menu.AddItem(new MenuItem("Health", "Min Health %").SetValue(new Slider(30)));
            Menu.AddItem(new MenuItem("BlockCamera", "Block Camera Packet").SetValue(true));
            Menu.AddToMainMenu();

            Game.PrintChat(
                "<b><font color =\"#FFFFFF\">Oathsworn Caster by </font><font color=\"#5C00A3\">Trees</font><font color =\"#FFFFFF\"> loaded!</font></b>");

         //   Game.OnGameProcessPacket += Game_OnGameProcessPacket;
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        private static void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            if (args.PacketData[0] != Packet.MultiPacket.Header ||
                args.PacketData[5] != (byte) Packet.MultiPacketType.LockCamera ||
                !Menu.Item("BlockCamera").GetValue<bool>())
            {
                return;
            }
            args.Process = false;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (!Menu.Item("Enabled").GetValue<KeyBind>().Active ||
                ObjectManager.Player.HealthPercentage() > Menu.Item("Health").GetValue<Slider>().Value)
            {
                return;
            }

            if (Oathsworn.Instance == null || Oathsworn.Instance.Name == null || Oathsworn.Instance.Name == "BaseSpell" ||
                (Oathsworn.Instance.Name == "KalistaRAllyDash" && State != 3))
            {
                return;
            }

            SetState();

            if (State != 0)
            {
                return;
            }

            var targ = SimpleTs.GetTarget(350, SimpleTs.DamageType.Magical);
            var pos = targ.IsValidTarget() ? targ.ServerPosition : Game.CursorPos;
            var p = new Packet.C2S.Cast.Struct(0, Oathsworn.Slot, -1, pos.X, pos.Y, pos.X, pos.Y, 0xF2);

            Packet.C2S.Cast.Encoded(p).Send();
        }

        private static void SetState()
        {
            switch (Oathsworn.Instance.Name)
            {
                case "KalistaRAllyDashCantCast1":
                    State = 1;
                    break;
                case "KalistaRAllyDashCantCast2":
                    State = 2;
                    break;
                case "KalistaRAllyDashCantCast3":
                    State = 3;
                    break;
                case "KalistaRAllyDash":
                    State = 0;
                    break;
            }
        }

        private static bool KalistaInGame()
        {
            return ObjectManager.Get<Obj_AI_Hero>().Any(hero => hero.IsAlly && hero.ChampionName == "Kalista");
        }
    }
}