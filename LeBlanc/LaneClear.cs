#region

using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace LeBlanc
{
    internal class LaneClear
    {
        private const string Name = "LaneClear";
        public static Menu LocalMenu;

        static LaneClear()
        {
            #region Menu

            var laneclear = new Menu("Farm Settings", "LaneClear");

            var lcQ = laneclear.AddSubMenu(new Menu("Q", "Q"));

            lcQ.AddItem(new MenuItem("LaneClearQ", "Use Q").SetValue(true));
            lcQ.AddItem(new MenuItem("LaneClearQMana", "Minimum Q Mana Percent").SetValue(new Slider(30)));


            var lcW = laneclear.AddSubMenu(new Menu("W", "W"));

            lcW.AddItem(new MenuItem("LaneClearW", "Use W").SetValue(true));
            lcW.AddItem(new MenuItem("LaneClearRW", "Use RW").SetValue(true));
            lcW.AddItem(new MenuItem("LaneClearWHits", "Min Enemies Hit").SetValue(new Slider(2, 0, 5)));
            lcW.AddItem(new MenuItem("LaneClearWMana", "Minimum W Mana Percent").SetValue(new Slider(30)));

            laneclear.AddItem(
                new MenuItem("LaneClearKey", "Farm Key").SetValue(new KeyBind((byte) 'V', KeyBindType.Press)));

            #endregion

            LocalMenu = laneclear;

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

        private static Spell R
        {
            get { return Spells.E; }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (!Enabled)
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

            if (CastW(true)) {}
        }

        public static bool CastQ()
        {
            var canCast = CanCast("Q") && Q.IsReady();
            var isLowMana = Player.ManaPercentage() < LocalMenu.Item("LaneClearQMana").GetValue<Slider>().Value;

            if (!canCast || isLowMana)
            {
                return false;
            }

            var unit =
                ObjectManager.Get<Obj_AI_Minion>()
                    .FirstOrDefault(
                        minion =>
                            minion.IsValidTarget(Q.Range) &&
                            minion.Health < Player.GetDamageSpell(minion, SpellSlot.Q).CalculatedDamage);

            return unit.IsValidTarget(Q.Range) && Q.Cast(unit).IsCasted();
        }

        public static bool CastW(bool ult = false)
        {
            var canCast = CanCast("W") && W.IsReady(1);
            var canCastUlt = ult && CanCast("RW") && R.IsReady(SpellSlot.W);
            var isLowMana = Player.ManaPercentage() <= Menu.Item("LaneClearWMana").GetValue<Slider>().Value;
            var minions = MinionManager.GetMinions(W.Range).Select(m => m.ServerPosition.To2D()).ToList();
            var minionPrediction = MinionManager.GetBestCircularFarmLocation(minions, W.Width, W.Range);
            var castPosition = minionPrediction.Position.To3D();
            var notEnoughHits = minionPrediction.MinionsHit < Menu.Item("LaneClearWHits").GetValue<Slider>().Value;

            if (notEnoughHits)
            {
                return false;
            }

            if (canCastUlt)
            {
                return R.Cast(SpellSlot.W, castPosition);
            }

            return canCast && !isLowMana && W.Cast(castPosition);
        }

        public static bool CanCast(string spell)
        {
            return LocalMenu.Item(Name + spell).GetValue<bool>();
        }
    }
}