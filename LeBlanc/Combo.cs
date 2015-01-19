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
        public static WPosition WBackPosition;
        public static readonly string LeBlancWObject = "LeBlanc_Base_W_return_indicator.troy";
        private static Obj_AI_Hero CurrentTarget;

        static Combo()
        {
            #region Menu

            var combo = new Menu(Name + " Settings", Name);

            var gapclose = combo.AddSubMenu(new Menu("GapClose", "Gap Close Combo"));
            //gapclose.AddItem(new MenuItem("Spacer", "This doesn't work yet"));
            gapclose.AddItem(new MenuItem("GapCloseEnabled", "Use GapClose Combo").SetValue(true));
            //replace with damage calcs
            gapclose.AddItem(new MenuItem("TargetHP", "On Target HP < %").SetValue(new Slider(40)));
            gapclose.AddItem(new MenuItem("PlayerHP", "On Self HP > %").SetValue(new Slider(40)));

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
            comboE.AddItem(new MenuItem("ComboEStart", "Start Combo with E").SetValue(false));
            comboE.AddItem(
                new MenuItem("ComboEHC", "MinHitChance").SetValue(
                    new StringList(
                        new[] { HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString() }, 1)));

            var comboR = combo.AddSubMenu(new Menu("R", "R"));
            comboR.AddItem(new MenuItem("ComboR", "Use R").SetValue(true));
            comboR.AddItem(
                new MenuItem("ComboRMode", "Ult Mode").SetValue(
                    new StringList(new[] { SpellSlot.Q.ToString(), SpellSlot.W.ToString(), SpellSlot.E.ToString() })));
            // ));

            combo.AddItem(new MenuItem("ComboItems", "Use Items").SetValue(true));
            combo.AddItem(new MenuItem("ComboKey", "Combo Key").SetValue(new KeyBind(32, KeyBindType.Press)));

            #endregion

            LocalMenu = combo;

            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
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

        private static HitChance EHitChance
        {
            get { return Utils.GetHitChance("ComboEHC"); }
        }

        private static void ComboLogic()
        {
            var spellsUp = Q.IsReady() && W.IsReady() && E.IsReady() && R.IsReady();
            var d = Player.Distance(CurrentTarget);
            var eFirst = Menu.Item("ComboEStart").GetValue<bool>();
            var castRE = R.IsReady(SpellSlot.E) && GetMenuUlt() == SpellSlot.E;
            var qFirst = Q.IsInRange(CurrentTarget) && !castRE;

            #region Items

            if (CanCast("Items"))
            {
                if (spellsUp && d < Items.DFG.Range && Items.DFG.Cast(CurrentTarget))
                {
                    return;
                }

                if (spellsUp && d < Items.BFT.Range && Items.BFT.Cast(CurrentTarget))
                {
                    return;
                }

                if (d < Items.BOTRK.Range && Items.BOTRK.Cast(CurrentTarget))
                {
                    return;
                }

                if (d < Items.FQC.Range && Items.FQC.Cast(CurrentTarget))
                {
                    return;
                }
            }

            #endregion

            /*if (CastSecondW())
            {
                return;
            }*/

            if (eFirst && CastE())
            {
                return;
            }

            if (qFirst && CastQ())
            {
                return;
            }

            if (CastR())
            {
                return;
            }

            if (CastW())
            {
                return;
            }

            if (CastE()) {}
        }

        private static bool CastQ()
        {
            return CanCast("Q") && Q.IsReady() && Q.CanCast(CurrentTarget) && Q.Cast(CurrentTarget).IsCasted();
        }

        private static bool CastW()
        {
            var canCast = CanCast("W") && W.IsReady(1);
            var wRange = CurrentTarget.IsValidTarget(W.Range);
            var lowHealth = Player.HealthPercentage() <= Menu.Item("ComboWMinHP").GetValue<Slider>().Value;
            return canCast && wRange && !lowHealth && W.Cast(CurrentTarget).IsCasted();
        }

        private static bool CastSecondW()
        {
            var canCast = CanCast("W2") && W.IsReady(2);
            var isLowHP = Player.HealthPercentage() <= Menu.Item("MiscW2HP").GetValue<Slider>().Value;
            var moreEnemiesInRange = WBackPosition.Position.CountEnemysInRange(600) > Player.CountEnemysInRange(600);
            var isFleeing = Program.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.None;
            var spellDown = Menu.Item("ComboW2Spells").GetValue<bool>() && !Q.IsReady() && !E.IsReady() && !R.IsReady();
            var cast = canCast && (isLowHP || spellDown) && !moreEnemiesInRange && !isFleeing;
            return cast && W.Cast();
        }

        private static bool CastE()
        {
            if (!CanCast("E") || !E.IsReady() || !E.CanCast(CurrentTarget) || Player.IsDashing())
            {
                return false;
            }

            var pred = E.GetPrediction(CurrentTarget);
            return pred.Hitchance >= EHitChance && E.Cast(pred.CastPosition);
        }

        private static bool CastR()
        {
            var slot = GetMenuUlt();
            var canCast = CanCast("R") && R.IsReady(slot);

            if (!canCast)
            {
                return false;
            }

            if (slot == SpellSlot.Q && Q.IsInRange(CurrentTarget))
            {
                return R.Cast(SpellSlot.Q, CurrentTarget).IsCasted();
            }

            if (slot == SpellSlot.W && W.IsInRange(CurrentTarget))
            {
                return R.Cast(SpellSlot.W, CurrentTarget).IsCasted();
            }

            if (slot == SpellSlot.E && E.IsInRange(CurrentTarget))
            {
                E.Slot = SpellSlot.R;
                var cast = E.CastIfHitchanceEquals(CurrentTarget, EHitChance);
                E.Slot = SpellSlot.E;
                return cast;
            }

            return false;
        }

        public static bool CanCast(string spell)
        {
            return Menu.Item(Name + spell).GetValue<bool>();
        }

        private static float GetComboRange()
        {
            return Menu.Item("ComboEStart").GetValue<bool>() ? E.Range : Q.Range;
        }

        #region Events 

        private static void Game_OnGameUpdate(EventArgs args)
        {
            CurrentTarget = Target;
            if (!Enabled || !CurrentTarget.IsValidTarget(1500))
            {
                return;
            }

            if (CurrentTarget.IsValidTarget(GetComboRange()))
            {
                ComboLogic();
            }

            if (Menu.Item("GapCloseEnabled").GetValue<bool>() && CurrentTarget.IsValidTarget(W.Range * 2))
            {
                var canCast = CanCast("W") && W.IsReady(1) && R.IsReady();
                var isTargetLow = CurrentTarget.HealthPercentage() <= Menu.Item("TargetHP").GetValue<Slider>().Value;
                var isPlayerLow = Player.HealthPercentage() < Menu.Item("PlayerHP").GetValue<Slider>().Value;
                var canDFG = (Items.DFG.HasItem() && Items.DFG.IsReady()) ||
                             (Items.BFT.HasItem() && Items.BFT.IsReady());

                if (!canCast || !isTargetLow || isPlayerLow || !canDFG)
                {
                    //Console.WriteLine("return");
                    return;
                }

                var pos = Player.Position.Extend(CurrentTarget.ServerPosition, W.Range + 100);
                if (W.Cast(pos)) {}
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var unit = sender as Obj_AI_Hero;

            if (unit == null || !unit.IsValid || !unit.IsMe || !Enabled)
            {
                return;
            }

            if (args.SData.IsAutoAttack())
            {
                return;
            }

            var name = args.SData.Name;


            if (name.Equals("LeblancSlide"))
            {
                Utility.DelayAction.Add(
                    400, () =>
                    {
                        var castDFG = CanCast("Items") && Items.DFG.HasItem() && Items.DFG.IsReady();
                        var castBFT = CanCast("Items") && Items.BFT.HasItem() && Items.BFT.IsReady();

                        if (castDFG && Items.DFG.Cast(CurrentTarget))
                        {
                            return;
                        }

                        if (castBFT && Items.BFT.Cast(CurrentTarget)) {}
                    });
                return;
            }


            Utility.DelayAction.Add(
                400, () =>
                {
                    var canCastR = (name.Equals("DeathfireGrasp") || name.Equals("ItemBlackfireTorch")) &&
                                   CurrentTarget.IsValidTarget(W.Range) && R.IsReady(SpellSlot.W);

                    if (!canCastR)
                    {
                        Console.WriteLine(Player.Distance(CurrentTarget));
                        Console.WriteLine("can't r");
                        return;
                    }

                    R.Cast(SpellSlot.R, CurrentTarget);
                });
        }

        private static SpellSlot GetMenuUlt()
        {
            return (SpellSlot) Menu.Item("ComboRMode").GetValue<StringList>().SelectedIndex;
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender == null || !sender.IsValid || !sender.Name.Equals(LeBlancWObject))
            {
                return;
            }

            WBackPosition = new WPosition(sender);
        }

        private static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            if (sender == null || !sender.IsValid || !sender.Name.Equals(LeBlancWObject))
            {
                return;
            }

            WBackPosition = new WPosition();
        }

        #endregion
    }

    public class WPosition
    {
        public float EndTick;
        public Vector3 Position;
        public float StartTick;
        public GameObject Unit;

        public WPosition()
        {
            Position = Vector3.Zero;
            StartTick = 0;
            EndTick = 0;
        }

        public WPosition(GameObject unit)
        {
            Unit = unit;
            Position = unit.Position;
            StartTick = Environment.TickCount;
            EndTick = StartTick + 8000f;
        }

        public bool IsActive()
        {
            return Unit != null && Unit.IsValid && Environment.TickCount - EndTick < 0;
        }

        public float GetTime()
        {
            return (EndTick - Environment.TickCount) / 1000f;
        }
    }
}