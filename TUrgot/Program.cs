#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace TUrgot
{
    internal class Program
    {
        public const string ChampName = "Urgot";
        public static Orbwalking.Orbwalker Orbwalker;
        public static readonly Obj_AI_Hero Player = ObjectManager.Player;

        public static List<Spell> SpellList = new List<Spell>();
        public static Spell Q, Q2, W, E;
        public static SpellDataInst Ignite;
        public static Menu Menu;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.BaseSkinName != ChampName) return;

            Q = new Spell(SpellSlot.Q, 1000);
            Q2 = new Spell(SpellSlot.Q, 1200);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 900);

            Q.SetSkillshot(0.10f, 100f, 1600f, true, SkillshotType.SkillshotLine);
            Q2.SetSkillshot(0.10f, 100f, 1600f, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.283f, 0f, 1750f, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(Q2);
            SpellList.Add(W);
            SpellList.Add(E);


            Ignite = Player.Spellbook.GetSpell(Player.GetSpellSlot("summonerdot"));

            Menu = new Menu("Trees " + ChampName, ChampName, true);

            Menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(Menu.SubMenu("Orbwalker"));

            var ts = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(ts);
            Menu.AddSubMenu(ts);

            Menu.AddSubMenu(new Menu("Combo", "Combo"));
            Menu.SubMenu("Combo").AddItem(new MenuItem("ComboQ", "Use Q").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("ComboE", "Use E").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("ComboEChance", "E HitChance").SetValue(new Slider(2, 1, 3)));
            Menu.SubMenu("Combo")
                .AddItem(new MenuItem("ComboActive", "Combo").SetValue(new KeyBind(32, KeyBindType.Press)));

            Menu.AddSubMenu(new Menu("Harass", "Harass"));
            Menu.SubMenu("Harass").AddItem(new MenuItem("HarassQ", "Use Q").SetValue(true));
            Menu.SubMenu("Harass").AddItem(new MenuItem("HarassE", "Use E").SetValue(true));
            Menu.SubMenu("Harass").AddItem(new MenuItem("HarassEChance", "E HitChance").SetValue(new Slider(3, 1, 3)));
            Menu.SubMenu("Harass")
                .AddItem(new MenuItem("HarassActive", "Harass").SetValue(new KeyBind((byte)'C', KeyBindType.Press)));

            Menu.AddSubMenu(new Menu("Drawings", "Drawings"));
            Menu.SubMenu("Drawings")
                .AddItem(new MenuItem("QRange", "Q").SetValue(new Circle(false, Color.Red)));
            Menu.SubMenu("Drawings")
                .AddItem(new MenuItem("ERange", "E").SetValue(new Circle(false, Color.Blue)));

            Menu.AddItem((new MenuItem("AutoQ", "Smart Q").SetValue(true)));
            Menu.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;

            Game.PrintChat("Trees" + ChampName + " loaded!");
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            CastLogic();
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var DrawQ = Menu.Item("QRange").GetValue<Circle>();
            var DrawE = Menu.Item("ERange").GetValue<Circle>();

            if (DrawQ.Active) Utility.DrawCircle(Player.Position, Q.Range, DrawQ.Color);
            if (DrawE.Active) Utility.DrawCircle(Player.Position, E.Range, DrawQ.Color);
        }

        private static void CastLogic()
        {
            KSLogic();
            SmartQ();

            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);
            if (target == null ||
                (!Menu.Item("ComboActive").GetValue<KeyBind>().Active &&
                 !Menu.Item("HarassActive").GetValue<KeyBind>().Active)) return;

            var mode = Menu.Item("ComboActive").GetValue<KeyBind>().Active ? "Combo" : "Harass";

            CastE(target, mode);
            CastQ(target, mode);
        }

        private static void SmartQ()
        {
            if (!Q.IsReady() || !Menu.Item("AutoQ").GetValue<bool>())
                return;

            foreach (
                var obj in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(
                            obj =>
                                obj.IsValid && obj.IsEnemy && obj.HasBuff("UrgotPlasmaGrenadeBoom") &&
                                obj.IsValidTarget(Q2.Range, true, Player.ServerPosition)))
            {
                W.Cast();
                Q2.Cast(obj.ServerPosition);
            }
        }

        private static void CastQ(Obj_AI_Base target, string mode)
        {
            if (Q.IsReady() && Menu.Item(mode + "Q").GetValue<bool>() &&
                Player.Distance(target) < Q.Range)
                Q.Cast(target);
        }

        private static void CastE(Obj_AI_Base target, string mode)
        {
            if (!E.IsReady() || !Menu.Item(mode + "E").GetValue<bool>())
                return;

            var hitchance = GetHitchance(Menu.Item(mode + "EChance").GetValue<int>());

            if (Player.ServerPosition.Distance(target.ServerPosition) < E.Range)
                E.CastIfHitchanceEquals(target, hitchance);
            else
                E.CastIfHitchanceEquals(SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Physical), HitChance.High);
        }

        private static void KSLogic()
        {
            if (Ignite != null && Ignite.Slot != SpellSlot.Unknown && Ignite.State == SpellState.Ready &&
                Player.CanCast)
                KSIgnite();
        }


        private static void KSIgnite()
        {
            var dmg = 50 + 20 * Player.Level;
            foreach (
                var obj in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(
                            obj => obj.IsValid && obj.IsEnemy && obj.IsValidTarget(600, true, Player.ServerPosition) &&
                                   obj.Health < dmg))
            {
                Player.SummonerSpellbook.CastSpell(Ignite.Slot, obj);
            }
        }


        private static HitChance GetHitchance(int num)
        {
            switch (num)
            {
                case 1:
                    return HitChance.Low;
                case 2:
                    return HitChance.Medium;
                case 3:
                    return HitChance.High;
                default:
                    return HitChance.High;
            }
        }
    }
}