using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace LeBlanc
{
    internal class Clone
    {
        public static Menu LocalMenu;

        static Clone()
        {
            #region Menu

            var clone = new Menu("Clone Settings", "Clone");
            clone.AddItem(new MenuItem("CloneEnabled", "Enabled").SetValue(true));
            clone.AddItem(
                new MenuItem("CloneMode", "Mode").SetValue(
                    new StringList(new[] { "To Player", "To Target", "Away from Player" })));

            #endregion

            LocalMenu = clone;

            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        private static Menu Menu
        {
            get { return Program.Menu; }
        }

        public static bool Enabled
        {
            get { return !Player.IsDead && Menu.Item("CloneEnabled").GetValue<bool>(); }
        }

        private static Obj_AI_Hero Target
        {
            get { return Utils.GetTarget(); }
        }

        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (!Enabled)
            {
                return;
            }

            var pet = Player.Pet as Obj_AI_Base;
            var mode = Menu.Item("CloneMode").GetValue<StringList>().SelectedIndex;

            if (pet == null || !pet.IsValidPet())
            {
                return;
            }

            switch (mode)
            {
                case 0: // toward player
                    var pos = Player.GetWaypoints().Count > 1 ? Player.GetWaypoints()[1].To3D() : Player.ServerPosition;
                    Utility.DelayAction.Add(200, () => { pet.IssueOrder(GameObjectOrder.MovePet, pos); });
                    break;
                case 1: //toward target
                    if (pet.CanAttack && !pet.IsWindingUp && !pet.Spellbook.IsAutoAttacking &&
                        Target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(pet)))
                    {
                        pet.IssueOrder(GameObjectOrder.AutoAttackPet, Target);
                    }
                    break;
                case 2: //away from player
                    Utility.DelayAction.Add(
                        100,
                        () =>
                        {
                            pet.IssueOrder(
                                GameObjectOrder.MovePet,
                                (pet.Position + 500 * ((pet.Position - Player.Position).Normalized())));
                        });
                    break;
            }
        }
    }
}