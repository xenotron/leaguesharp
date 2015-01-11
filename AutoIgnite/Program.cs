#region Imports

using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace AutoIgnite
{
    internal class Program
    {
        private static SpellDataInst _ignite;
        private static Obj_AI_Hero _player;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad(EventArgs args)
        {
            _player = ObjectManager.Player;

            _ignite = _player.Spellbook.GetSpell(_player.GetSpellSlot("summonerdot"));
            if (_ignite == null || _ignite.Slot == SpellSlot.Unknown)
                return;

            Game.PrintChat("AutoIgnite loaded.");
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (!CanIgnite())
                return;
            var dmg = 50 + 20 * _player.Level;
            foreach (
                var hero in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(
                            hero =>
                                hero != null && hero.IsValid && hero.IsVisible && !hero.IsDead && hero.Health < dmg &&
                                _player.ServerPosition.Distance(hero.ServerPosition) <= 600))
                CastIgnite(hero);
        }

        private static bool CanIgnite()
        {
            return _ignite != null && _ignite.Slot != SpellSlot.Unknown && _ignite.State == SpellState.Ready &&
                   _player.CanCast;
        }

        private static void CastIgnite(AttackableUnit enemy)
        {
            if (enemy == null || !enemy.IsValid || !enemy.IsVisible || !enemy.IsTargetable || enemy.IsDead)
                return;

            if (CanIgnite())
                _player.Spellbook.CastSpell(_ignite.Slot, enemy);
        }
    }
}
