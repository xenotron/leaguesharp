#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace AttackDrawer
{
    internal class Program
    {
        private static readonly Dictionary<Obj_AI_Hero, Render.Circle> CircleDictionary =
            new Dictionary<Obj_AI_Hero, Render.Circle>();

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValid && hero.IsEnemy))
            {
                var circle = new Render.Circle(hero, Orbwalking.GetRealAutoAttackRange(hero), Color.Red, 4, true);
                circle.Add();
                CircleDictionary.Add(hero, circle);
            }

            AppDomain.CurrentDomain.DomainUnload += CurrentDomainDomainUnload;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomainDomainUnload;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Game.PrintChat("Attack Range Loaded!");
        }

        private static void CurrentDomainDomainUnload(object sender, EventArgs e)
        {
            foreach (var circle in CircleDictionary)
            {
                circle.Value.Dispose();
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            foreach (var entry in CircleDictionary)
            {
                var hero = entry.Key;
                var circle = entry.Value;
                var aaRange = Orbwalking.GetRealAutoAttackRange(hero);

                circle.Radius = aaRange;
                circle.Color = ObjectManager.Player.Distance(hero) < aaRange ? Color.Red : Color.LimeGreen;
            }
        }
    }
}