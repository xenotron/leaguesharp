#region Includes
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using SharpDX;
#endregion

namespace AutoIgnite
{

    class Program
    {
        private static SpellDataInst Ignite;
        private static Obj_AI_Hero Player;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }
        
        private static void OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            
            Ignite = GetIgnite();
            if (Ignite == null)
             return;
             
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (!CanIgnite())
                return;
            int dmg = 50 + 20 * Player.Level;
            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero != null && hero.IsValid && hero.IsVisible && !hero.IsDead && hero.Health < dmg && Vector3.Distance(Player.ServerPosition, hero.ServerPosition) <= 600))
                CastIgnite(hero);
        }

        private static SpellDataInst GetIgnite()
        {
            var spells = Player.SummonerSpellbook.Spells;
            return spells.FirstOrDefault(spell => spell.Name == "SummonerDot");
        }

        private static bool CanIgnite()
        {
            if (Ignite != null && Ignite.Slot != SpellSlot.Unknown && Ignite.State == SpellState.Ready &&
               Player.CanCast)
                return true;
            return false;
        }

        private static void CastIgnite(Obj_AI_Hero enemy)
        {
            if (enemy == null || !enemy.IsValid || !enemy.IsVisible || !enemy.IsTargetable || enemy.IsDead)
                return;

            if (CanIgnite())
                Player.SummonerSpellbook.CastSpell(Ignite.Slot, enemy);
        }


    }
}
