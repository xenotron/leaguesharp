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

        public static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Menu = new Menu("SpellHumanizer", "SpellHumanizer", true);
            Menu.AddItem(new MenuItem("Enabled", "Enabled").SetValue(true));
            Menu.AddItem(new MenuItem("Debug", "Debug").SetValue(false));
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            LeagueSharp.Common.SpellHumanizer.Enabled = Menu.Item("Enabled").GetValue<bool>();
            LeagueSharp.Common.SpellHumanizer.Debug = Menu.Item("Debug").GetValue<bool>();
        }
    }
}