#region

using System;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace SpellHumanizer
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            LeagueSharp.Common.SpellHumanizer.Enabled = true;
        }
    }
}