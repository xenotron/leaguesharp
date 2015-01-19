using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace LeBlanc
{
    internal class Harass
    {
        private const string Name = "Harass";
        public static Menu LocalMenu;
        private static Obj_AI_Hero CurrentTarget;

        static Harass()
        {
            #region Menu

            var harass = new Menu(Name + " Settings", Name);
            var harassQ = harass.AddSubMenu(new Menu("Q", "Q"));
            harassQ.AddItem(new MenuItem("HarassQ", "Use Q").SetValue(true));
            harassQ.AddItem(new MenuItem("HarassQMana", "Min Mana %").SetValue(new Slider(40)));

            var harassW = harass.AddSubMenu(new Menu("W", "W"));
            harassW.AddItem(new MenuItem("HarassW", "Use W").SetValue(true));
            harassW.AddItem(new MenuItem("HarassW2", "Use Second W").SetValue(true));
            harassW.AddItem(
                new MenuItem("HarassW2Mode", "Second W Setting").SetValue(new StringList(new[] { "Auto", "After E" })));
            harassW.AddItem(new MenuItem("HarassWMana", "Min Mana %").SetValue(new Slider(40)));

            var harassE = harass.AddSubMenu(new Menu("E", "E"));
            harassE.AddItem(new MenuItem("HarassE", "Use E").SetValue(true));
            harassE.AddItem(
                new MenuItem("HarassEHC", "MinHitChance").SetValue(
                    new StringList(
                        new[] { HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString() }, 1)));
            harassE.AddItem(new MenuItem("HarassEMana", "Min Mana %").SetValue(new Slider(40)));

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
            CurrentTarget = Target;
            if (!Enabled || !CurrentTarget.IsValidTarget(Q.Range))
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

            if (CastE())
            {
                return;
            }

            if (CastSecondW()) {}
        }

        private static bool CastQ()
        {
            return CanCast("Q") && Q.IsReady() && Q.CanCast(CurrentTarget) && Q.Cast(CurrentTarget).IsCasted();
        }

        private static bool CastW()
        {
            var canCast = CanCast("W") && W.IsReady(1);
            var qwRange = CurrentTarget.IsValidTarget(Q.Range + W.Range);
            var wRange = CurrentTarget.IsValidTarget(W.Range);

            if (!canCast)
            {
                return false;
            }

            if (wRange)
            {
                return W.Cast(CurrentTarget).IsCasted();
            }

            if (qwRange)
            {
                return W.Cast(Player.ServerPosition.Extend(CurrentTarget.ServerPosition, W.Range));
            }

            return false;
        }

        private static bool CastSecondW()
        {
            var canCast = LocalMenu.Item("HarassW2").GetValue<bool>() && W.IsReady(2);

            if (!canCast)
            {
                return false;
            }

            var mode = Menu.Item("HarassW2Mode").GetValue<StringList>().SelectedIndex;

            return mode == 1 ? W.Cast() : CurrentTarget.HasEBuff() && W.Cast();
        }

        private static bool CastE(HitChance hc = HitChance.Low)
        {
            if (!CanCast("E") || !E.IsReady() || !E.CanCast(CurrentTarget) || Player.IsDashing())
            {
                return false;
            }

            var chance = hc == HitChance.Low ? Utils.GetHitChance("HarassEHC") : hc;
            var pred = E.GetPrediction(CurrentTarget);
            return pred.Hitchance >= chance && E.Cast(pred.CastPosition);
        }

        public static bool CanCast(string spell)
        {
            var cast = Menu.Item(Name + spell).GetValue<bool>();
            var lowMana = Player.ManaPercentage() < Menu.Item(Name + spell + "Mana").GetValue<Slider>().Value;
            return cast && !lowMana;
        }
    }
}