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
            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValid && hero.IsEnemy))
            {
                var circle = new Render.Circle(hero, Orbwalking.GetRealAutoAttackRange(hero), Color.Red, 4, true);
                circle.Add();
                CircleDictionary.Add(hero, circle);
            }

            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_DomainUnload;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Game.PrintChat("Attack Range Loaded!");
        }

        private static void CurrentDomain_DomainUnload(object sender, EventArgs e)
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
                Obj_AI_Hero hero = entry.Key;
                Render.Circle circle = entry.Value;
                float AARange = Orbwalking.GetRealAutoAttackRange(hero);

                circle.Radius = AARange;
                circle.Color = ObjectManager.Player.Distance(hero) < AARange ? Color.Red : Color.LimeGreen;
            }
        }
    }
}