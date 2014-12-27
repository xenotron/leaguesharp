using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

namespace LeBlanc
{
    internal class Program
    {
        public static Menu Menu;
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static Orbwalking.Orbwalker Orbwalker;
        public static Obj_AI_Hero Target;
        public static Obj_AI_Hero Player = ObjectManager.Player;
        public static bool LCombo;
        public static Obj_AI_Base Clone;

        public static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Menu = new Menu("LeBlanc The Schemer", "LeBlanc", true);

            Menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(Menu.SubMenu("Orbwalker"));

            var ts = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(ts);
            Menu.AddSubMenu(ts);

            var combo = new Menu("Combo Settings", "Combo");
            combo.AddItem(new MenuItem("SmartW", "Use Smart W").SetValue(true));
            combo.AddItem(new MenuItem("Combo", "Combo Key").SetValue(new KeyBind(32, KeyBindType.Press)));
            Menu.AddSubMenu(combo);

            var harass = Menu.AddSubMenu(new Menu("Harass Settings", "Harass"));
            harass.AddItem(
                new MenuItem("SecondW", "Second W Setting").SetValue(
                    new StringList(new[] { "Auto", "Manual", "After E" })));
            harass.AddItem(new MenuItem("Harass", "Harass Key").SetValue(new KeyBind((byte) 'C', KeyBindType.Press)));

            var laneclear = Menu.AddSubMenu(new Menu("LaneClear", "LaneClear"));
            laneclear.AddItem(new MenuItem("LaneClearQ", "Use Q").SetValue(true));
            laneclear.AddItem(new MenuItem("LaneClearQManaPercent", "Minimum Q Mana Percent").SetValue(new Slider(30)));
            laneclear.AddItem(
                new MenuItem("LaneClearActive", "LaneClear").SetValue(new KeyBind((byte) 'V', KeyBindType.Press)));

            Menu.AddToMainMenu();

            Q = new Spell(SpellSlot.Q, 700);
            Q.SetTargetted(.401f, 2000);

            W = new Spell(SpellSlot.W, 550);
            W.SetSkillshot(.5f, 100, 2000, false, SkillshotType.SkillshotCircle);

            E = new Spell(SpellSlot.E, 970);
            E.SetSkillshot(.366f, 70, 1600, true, SkillshotType.SkillshotLine);

            R = new Spell(SpellSlot.R);

            Game.OnGameUpdate += Game_OnGameUpdate;
            GameObject.OnCreate += Obj_AI_Base_OnCreate;
        }

        private static void Obj_AI_Base_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.IsValid && sender.Name.Equals(Player.Name))
            {
                Clone = sender as Obj_AI_Base;
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }

            Target = TargetSelector.GetTarget(2000, TargetSelector.DamageType.Magical);

            if (Target == null || !Target.IsValid)
            {
                return;
            }

            if (LCombo && R.IsReady())
            {
                //dfg cast
                ObjectManager.Player.Spellbook.CastSpell(SpellSlot.R, Target.Position);
                E.Cast(Target);
                LCombo = false;
            }

            if (Menu.SubMenu("Combo").Item("Combo").GetValue<KeyBind>().Active)
            {
                //LauraCombo();
                   Combo();
            }


            if (Menu.Item("Harass").GetValue<KeyBind>().Active)
            {
                Harass();
                return;
            }


            if (Menu.Item("LaneClear").GetValue<KeyBind>().Active)
            {
                LaneClear();
            }
        }

        private static void LauraCombo()
        {
            if (!W.IsReady() || !IsFirstW() || !R.IsReady() || Target.Distance(Player.Position) < W.Range * 2)
            {
                return;
            }

            LCombo = true;
            var pos = Player.ServerPosition.To2D().Extend(Target.ServerPosition.To2D(), W.Range);
            W.Cast(pos);
        }

        private static void Combo()
        {
            var combo = new List<SpellSlot> { SpellSlot.Q, SpellSlot.E };
            //var dmg = LeagueSharp.Common.Damage.GetComboDamage();
            //  Game.PrintChat("COMBO");
            if (W.IsReady() && W.InRange(Target) && IsFirstW())
            {
                W.Cast(Target);
            }

            if (Q.IsReady() && Q.InRange(Target))
            {
                Q.CastOnUnit(Target);

                if (R.IsReady())
                {
                    Player.Spellbook.CastSpell(SpellSlot.R, Target);
                    //R.CastOnUnit(Target);
                }
            }

            if (E.IsReady() && E.InRange(Target))
            {
                E.Cast(Target);
            }
        }

        private static void Harass()
        {
            if (Q.IsReady() && Q.InRange(Target))
            {
                Q.Cast(Target);
            }

            if (W.IsReady())
            {
                if (IsFirstW() && W.InRange(Target))
                {
                    W.Cast(Target);
                    if (E.IsReady() && E.InRange(Target))
                    {
                        E.Cast(Target);
                    }
                }
                else if (IsSecondW() && GetWMode() == 0)
                {
                    Player.Spellbook.CastSpell(SpellSlot.W);
                    //W.Cast();
                }
            }
        }

        private static void LaneClear() {}

        private static bool IsFirstW()
        {
            return W.Instance.Name == "LeblancSlide";
        }

        private static bool IsSecondW()
        {
            return W.Instance.Name == "leblancslidereturn";
        }

        private static int GetWMode()
        {
            return Menu.SubMenu("Harass").Item("SecondW").GetValue<StringList>().SelectedIndex;
        }

        private static SpellSlot UltType()
        {
            switch (R.Instance.Name)
            {
                case "LeblancChaosOrbM":
                    return SpellSlot.Q;
                case "LeblancSlideM":
                    return SpellSlot.W;
                case "LeblancSoulShackleM":
                    return SpellSlot.E;
                default:
                    return SpellSlot.R;
            }
        }
    }
}