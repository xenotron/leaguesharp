using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace LeBlanc
{
    internal class Harass
    {
        private const string Name = "Harass";
        public static Menu LocalMenu;

        static Harass()
        {
            #region Menu

            var harass = new Menu(Name + " Settings", Name);
            var harassQ = harass.AddSubMenu(new Menu("Q", "Q"));
            harassQ.AddItem(new MenuItem("HarassQ", "Use Q").SetValue(true));

            var harassW = harass.AddSubMenu(new Menu("W", "W"));
            harassW.AddItem(new MenuItem("HarassW", "Use W").SetValue(true));
            harassW.AddItem(new MenuItem("HarassW2", "Use Second W").SetValue(true));
            harassW.AddItem(
                new MenuItem("HarassW2Mode", "Second W Setting").SetValue(new StringList(new[] { "Auto", "After E" })));

            var harassE = harass.AddSubMenu(new Menu("E", "E"));
            harassE.AddItem(new MenuItem("HarassE", "Use E").SetValue(true));
            harassE.AddItem(
                new MenuItem("HarassEHC", "MinHitChance").SetValue(
                    new StringList(
                        new[] { HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString() }, 1)));


          //  harass.AddItem(new MenuItem("HarassCombo", "W->Q->E->W Combo").SetValue(true));

            /* var harassR = harass.AddSubMenu(new Menu("R", "R"));
            harassR.AddItem(new MenuItem("HarassR", "Use R").SetValue(true));
            */

            harass.AddItem(new MenuItem("HarassKey", "Harass Key").SetValue(new KeyBind((byte) 'C', KeyBindType.Press)));

            #endregion

            LocalMenu = harass;

            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        private static Menu Menu
        {
            get { return Program.Menu; }
        }

        public static bool Enabled
        {
            get { return !Player.IsDead && Program.Menu.Item("HarassKey").GetValue<KeyBind>().Active; }
        }

        private static Obj_AI_Hero Target
        {
            get { return Utils.GetTarget(); }
        }

        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        private static Spell Q
        {
            get { return Spells.Q; }
        }

        private static Spell W
        {
            get { return Spells.W; }
        }

        private static Spell E
        {
            get { return Spells.E; }
        }

        private static Spell R
        {
            get { return Spells.R; }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (!Enabled || !Target.IsValidTarget(1500))
            {
                return;
            }

            if (CastQ())
            {
                return;
            }

            if (CastE(HitChance.High))
            {
                return;
            }

            if (CastW())
            {
                return;
            }

            if (CastE(Utils.GetHitChance("HarassEHC")))
            {
                return;
            }

            if (CastSecondW()) {}
        }

        private static bool CastQ()
        {
            return CanCast("Q") && Q.IsReady() && Q.CanCast(Target) && Q.Cast(Target).IsCast();
        }

        private static bool CastW()
        {
            var canCast = CanCast("W") && W.IsReady() && W.GetState(1);
            var qwRange = Target.IsValidTarget(Q.Range + W.Range);
            var wRange = Target.IsValidTarget(W.Range + 100);

            if (!canCast)
            {
                return false;
            }

            if (wRange)
            {
                return W.Cast(Target).IsCast();
            }

            if (qwRange)
            {
                return W.Cast(Player.ServerPosition.Extend(Target.ServerPosition, W.Range + 100));
            }

            return false;
        }

        private static bool CastSecondW()
        {
            var canCast = CanCast("W2") && W.IsReady() && W.GetState(2);
            if (!canCast)
            {
                return false;
            }

            var mode = Menu.Item("HarassW2Mode").GetValue<StringList>().SelectedIndex;

            return mode == 1 ? W.Cast() : Target.HasEBuff() && W.Cast();
        }

        private static bool CastE(HitChance hc)
        {
            if (!CanCast("E") || !E.IsReady() || !E.CanCast(Target))
            {
                return false;
            }

            var pred = E.GetPrediction(Target);
            return pred.Hitchance >= hc && E.Cast(pred.CastPosition);
        }

        public static bool CanCast(string spell)
        {
            return Menu.Item(Name + spell).GetValue<bool>();
        }
    }
}