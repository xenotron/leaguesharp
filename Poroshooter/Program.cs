using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing;

namespace PoroShooter
{
    class Program
    {
        public static Spell PoroThrow;
        public static Menu Menu;

        private static void Main(string[] args)
        {
            Game.OnGameUpdate += Game_OnGameUpdate;
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            var spell = ObjectManager.Player.GetSpellSlot("summonerporothrow");
            if (spell == SpellSlot.Unknown)
            {
                Console.WriteLine("RETURN");
                return;
            }

            PoroThrow = new Spell(spell, 2500f);
            PoroThrow.SetSkillshot(.25f, 75f, 1600, true, SkillshotType.SkillshotLine);

            Menu = new Menu("PoroShooter", "PoroShooter", true);
            Menu.AddItem(new MenuItem("DecreaseRange", "Decrease Range by").SetValue(new Slider(10)));
            Menu.AddItem(
                new MenuItem("HitChance", "MinHitChance").SetValue(
                    new StringList(
                        new[] { HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString() }, 1)));
            Menu.Item("HitChance").ValueChanged += Program_ValueChanged;
            Menu.Item("DecreaseRange").ValueChanged += Program_ValueChanged1;
            Menu.AddToMainMenu();

            PoroThrow.MinHitChance = GetHitChance();
        }

        private static void Program_ValueChanged1(object sender, OnValueChangeEventArgs e)
        {
            PoroThrow.Range = 2500f - e.GetNewValue<Slider>().Value;
        }

        private static void Program_ValueChanged(object sender, OnValueChangeEventArgs e)
        {
            PoroThrow.MinHitChance = GetHitChance();
        }

        private static HitChance GetHitChance()
        {
            var hc = Menu.Item("HitChance").GetValue<StringList>();
            switch (hc.SList[hc.SelectedIndex])
            {
                case "Low":
                    return HitChance.Low;
                case "Medium":
                    return HitChance.Medium;
                case "High":
                    return HitChance.High;
            }
            return HitChance.Medium;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (!CanCast())
            {
                return;
            }

            foreach (var champ in
                ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValidTarget(PoroThrow.Range)).Where(champ => CanCast()))
            {
                PoroThrow.Cast(champ);
            }
        }

        private static bool CanCast()
        {
            return PoroThrow != null && !ObjectManager.Player.IsDead && PoroThrow.IsReady() &&
                   PoroThrow.Instance.Name != "porothrowfollowupcast";
        }
    }
}