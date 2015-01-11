using System;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace LeBlanc
{
    internal class Combo
    {
        private const string Name = "Combo";
        public static Menu LocalMenu;
        public static Vector3 WPosition;
        public static readonly string LeBlancWObject = "LeBlanc_Base_W_return_indicator.troy";

        static Combo()
        {
            #region Menu

            var combo = new Menu(Name + " Settings", Name);

            var gapclose = combo.AddSubMenu(new Menu("GapClose", "Gap Close Combo"));
            //gapclose.AddItem(new MenuItem("Spacer", "This doesn't work yet"));
            //gapclose.AddItem(new MenuItem("GapCloseEnabled", "Use GapClose Combo").SetValue(true));
            //replace with damage calcs
            gapclose.AddItem(new MenuItem("TargetHP", "At Target HP %").SetValue(new Slider(40)));
            gapclose.AddItem(new MenuItem("PlayerHP", "Min Player HP %").SetValue(new Slider(40)));

            var comboQ = combo.AddSubMenu(new Menu("Q", "Q"));
            comboQ.AddItem(new MenuItem("ComboQ", "Use Q").SetValue(true));

            var comboW = combo.AddSubMenu(new Menu("W", "W"));
            comboW.AddItem(new MenuItem("ComboW", "Use W").SetValue(true));
            comboW.AddItem(new MenuItem("Spacer", "Set to 0% To Always W"));
            comboW.AddItem(new MenuItem("ComboWMinHP", "Min HP To Use W").SetValue(new Slider(20)));
            comboW.AddItem(new MenuItem("ComboW2", "Use Second W").SetValue(true));
            comboW.AddItem(new MenuItem("ComboW2Spells", "Use After Spells on CD").SetValue(true));

            var comboE = combo.AddSubMenu(new Menu("E", "E"));
            comboE.AddItem(new MenuItem("ComboE", "Use E").SetValue(true));
            comboE.AddItem(
                new MenuItem("ComboEHC", "MinHitChance").SetValue(
                    new StringList(
                        new[] { HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString() }, 1)));

            var comboR = combo.AddSubMenu(new Menu("R", "R"));
            comboR.AddItem(new MenuItem("ComboR", "Use R").SetValue(true));
            comboR.AddItem(
                new MenuItem("ComboUltMode", "Ult Mode").SetValue(new StringList(new[] { SpellSlot.Q.ToString() })));
            // SpellSlot.W.ToString(), SpellSlot.E.ToString() })));

            combo.AddItem(new MenuItem("ComboQRange", "Only Combo in Q Range").SetValue(true));
            combo.AddItem(new MenuItem("ComboItems", "Use Items").SetValue(true));
            combo.AddItem(new MenuItem("ComboKey", "Combo Key").SetValue(new KeyBind(32, KeyBindType.Press)));

            #endregion

            LocalMenu = combo;

            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        private static Menu Menu
        {
            get { return Program.Menu; }
        }

        public static bool Enabled
        {
            get { return !Player.IsDead && Menu.Item(Name + "Key").GetValue<KeyBind>().Active; }
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
            get { return Program.Q; }
        }

        private static Spell W
        {
            get { return Program.W; }
        }

        private static Spell E
        {
            get { return Program.E; }
        }

        private static Spell R
        {
            get { return Program.R; }
        }

        private static void ComboLogic()
        {
            var spellsUp = Q.IsReady() && W.IsReady() && E.IsReady() && R.IsReady();
            var d = Player.Distance(Target);

            #region Items

            if (CanCast("Items"))
            {
                if (spellsUp && d < Items.DFG.Range && Items.DFG.Cast(Target))
                {
                    return;
                }

                if (spellsUp && d < Items.BFT.Range && Items.BFT.Cast(Target))
                {
                    return;
                }

                if (d < Items.BOTRK.Range && Items.BOTRK.Cast(Target))
                {
                    return;
                }

                if (d < Items.FQC.Range && Items.FQC.Cast(Target))
                {
                    return;
                }
            }

            #endregion

     /*       if (CastSecondW())
            {
                return;
            }
            */
            if (CastR())
            {
                return;
            }

            if (CastQ())
            {
                return;
            }

            if (CastW())

            {
                return;
            }


            if (CastE(Utils.GetHitChance("ComboEHC"))) {}
        }

        private static bool CastQ()
        {
            return CanCast("Q") && Q.IsReady() && Q.CanCast(Target) && Q.Cast(Target).IsCast();
        }

        private static bool CastW()
        {
            var canCast = CanCast("W") && W.IsReady() && W.GetState(1);
            var wRange = Target.IsValidTarget(W.Range + 100);
            var lowHealth = Player.HealthPercentage() <= Menu.Item("ComboWMinHP").GetValue<Slider>().Value;
            return canCast && wRange && !lowHealth && W.Cast(Target).IsCast();
        }

        private static bool CastSecondW()
        {
            var canCast = CanCast("W2") && W.IsReady() && W.GetState(2);
            var isLowHP = Player.HealthPercentage() <= Menu.Item("MiscW2HP").GetValue<Slider>().Value;
            var moreEnemiesInRange = WPosition.CountEnemysInRange(600) > Player.CountEnemysInRange(600);
            var isFleeing = Program.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.None;
            var spellDown = Menu.Item("ComboW2Spells").GetValue<bool>() && !Q.IsReady() && !E.IsReady() && !R.IsReady();
            var cast = canCast && (isLowHP || spellDown) && !moreEnemiesInRange && !isFleeing;
            return cast && W.Cast();
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

        private static bool CastR()
        {
            return CanCast("R") && R.IsReady(SpellSlot.Q) && Q.IsInRange(Target) && R.Cast(SpellSlot.Q, Target).IsCast();
        }

        public static bool CanCast(string spell)
        {
            return Menu.Item(Name + spell).GetValue<bool>();
        }

        private static float GetComboRange()
        {
            return Menu.Item("ComboQRange").GetValue<bool>() ? W.Range * 2 : E.Range;
        }

        #region Events 

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (!Enabled || !Target.IsValidTarget(1500))
            {
                return;
            }

            if (Target.IsValidTarget(GetComboRange()))
            {
                ComboLogic();
                return;
            }

            if (Target.IsValidTarget(W.Range * 2))
            {
                var canCast = CanCast("W") && W.IsReady() && W.GetState(1) && R.IsReady();
                var isTargetLow = Target.HealthPercentage() <= Menu.Item("TargetHP").GetValue<Slider>().Value;
                var isPlayerLow = Player.HealthPercentage() <= Menu.Item("PlayerHP").GetValue<Slider>().Value;

                if (!canCast || !isTargetLow || isPlayerLow)
                {
                    return;
                }

                var pos = Player.Position.Extend(Target.Position, W.Range + 100);
                if (W.Cast(pos)) {}
            }
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender == null || !sender.IsValid || !sender.Name.Equals(LeBlancWObject))
            {
                return;
            }

            WPosition = sender.Position;
        }

        private static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            if (sender == null || !sender.IsValid || !sender.Name.Equals(LeBlancWObject))
            {
                return;
            }

            WPosition = Vector3.Zero;
        }

        #endregion
    }
}