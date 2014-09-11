#region

using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace QuickTeleport
{
    internal class Program
    {
        private static Menu _menu;
        private static SpellDataInst _teleport;
        private static Obj_AI_Hero _player;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad(EventArgs args)
        {
            _player = ObjectManager.Player;
            _teleport = _player.Spellbook.GetSpell(_player.GetSpellSlot("SummonerTeleport"));
            if (_teleport == null || _teleport.Slot == SpellSlot.Unknown)
                return;

            _menu = new Menu("QuickTeleport", "QuickTeleport", true);
            _menu.AddItem(new MenuItem("Hotkey", "Hotkey").SetValue(new KeyBind(16, KeyBindType.Press, false)));
            _menu.AddItem(new MenuItem("Turret", "QT to Turrets Only").SetValue(true));
            _menu.AddToMainMenu();
            Game.OnGameUpdate += Game_OnGameUpdate;
            Game.PrintChat("QuickTeleport by Trees loaded.");
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (!CanTeleport() || !_menu.Item("Hotkey").GetValue<KeyBind>().Active)
                return;

            Obj_AI_Base closestObject = _player;
            float d = 2000;

            foreach (
                var obj in
                    ObjectManager.Get<Obj_AI_Base>()
                        .Where(
                            obj =>
                                obj != null && obj.IsValid && obj.IsVisible && !obj.IsDead && obj.Team == _player.Team &&
                                obj.Type != _player.Type &&
                                (obj is Obj_AI_Turret || !_menu.Item("Turret").GetValue<bool>()) &&
                                obj.ServerPosition.Distance(Game.CursorPos) < d))
            {
                closestObject = obj;
                d = obj.ServerPosition.Distance(Game.CursorPos);
            }

            if (closestObject != _player && closestObject != null)
                CastTeleport(closestObject);
        }

        private static bool CanTeleport()
        {
            return _teleport != null && _teleport.Slot != SpellSlot.Unknown && _teleport.State == SpellState.Ready &&
                   _player.CanCast;
        }

        private static void CastTeleport(Obj_AI_Base unit)
        {
            if (CanTeleport())
                _player.SummonerSpellbook.CastSpell(_teleport.Slot, unit);
        }
    }
}