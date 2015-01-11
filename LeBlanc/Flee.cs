using System;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace LeBlanc
{
    internal class Flee
    {
        private const string Name = "Flee";
        public static Menu LocalMenu;

        static Flee()
        {
            #region Menu

            var flee = new Menu(Name + " Settings", Name);

            var fleeW = flee.AddSubMenu(new Menu("W", "W"));
            fleeW.AddItem(new MenuItem("FleeW", "Use W").SetValue(true));

            var fleeE = flee.AddSubMenu(new Menu("E", "E"));
            fleeE.AddItem(new MenuItem("FleeE", "Use E").SetValue(true));
            fleeE.AddItem(
                new MenuItem("FleeEHC", "MinHitChance").SetValue(
                    new StringList(
                        new[] { HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString() }, 1)));

            var fleeR = flee.AddSubMenu(new Menu("R", "R"));
            fleeR.AddItem(new MenuItem("FleeRW", "Use W Ult").SetValue(true));
            flee.AddItem(new MenuItem("FleeKey", "Flee Key").SetValue(new KeyBind((byte) 'T', KeyBindType.Press)));

            #endregion

            LocalMenu = flee;

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
            if (!Enabled)
            {
                return;
            }

            Program.Orbwalker.ActiveMode = Orbwalking.OrbwalkingMode.None;

            if (CastW())
            {
                MoveTo();
                return;
            }

            if (CastR())
            {
                MoveTo();
                return;
            }

            if (CastE(Utils.GetHitChance("FleeEHC")))
            {
                MoveTo();
                return;
            }

            MoveTo();
        }

        private static void MoveTo()
        {
            Utils.Troll();
            var d = Player.ServerPosition.Distance(Game.CursorPos);
            Player.IssueOrder(GameObjectOrder.MoveTo, Player.ServerPosition.Extend(Game.CursorPos, d + 250));
        }

        private static bool CastW()
        {
            return CanCast("W") && W.IsReady() && W.GetState(1) && W.Cast(GetCastPosition());
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
            var canCast = CanCast("RW") && R.IsReady(SpellSlot.W);
            return canCast && R.Cast(SpellSlot.W, GetCastPosition());
        }

        public static Vector3 GetCastPosition()
        {
            return Player.ServerPosition.Extend(Game.CursorPos, W.Range + new Random().Next(100));
        }

        public static bool CanCast(string spell)
        {
            return Menu.Item(Name + spell).GetValue<bool>();
        }
    }
}