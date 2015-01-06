using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace LeBlanc
{
    internal static class Utils
    {
        public static bool Cast(this ItemId id)
        {
            return id.GetItemSlot()._cast(ObjectManager.Player);
        }

        public static bool Cast(this ItemId id, Obj_AI_Base target)
        {
            return id.GetItemSlot()._cast(target);
        }

        private static bool _cast(this InventorySlot slot, GameObject target)
        {
            return slot.CanCast() && ObjectManager.Player.Spellbook.CastSpell(slot.SpellSlot, target);
        }

        public static bool IsReady(this ItemId id)
        {
            return id.GetItemSlot().CanCast();
        }

        private static InventorySlot GetItemSlot(this ItemId id)
        {
            return ObjectManager.Player.InventoryItems.FirstOrDefault(i => i.Id == id);
        }

        public static bool CanCast(this InventorySlot slot)
        {
            return slot != null && slot.SpellSlot != SpellSlot.Unknown &&
                   ObjectManager.Player.Spellbook.GetSpell(slot.SpellSlot).IsReady();
        }

        public static void RandomizeCast(this Spell spell, Vector3 position)
        {
            var rnd = new Random(Environment.TickCount);
            var pos = new Vector2(position.X + rnd.Next(90), position.Y + rnd.Next(90)).To3D();
            spell.Cast(pos);
        }

        public static void SetSpell(this Spell spell, SpellSlot slot)
        {
            switch (slot)
            {
                case SpellSlot.Q:
                    spell = new Spell(spell.Slot, 700);
                    spell.SetTargetted(.401f, 2000);
                    return;
                case SpellSlot.W:
                    spell = new Spell(spell.Slot, 600);
                    spell.SetSkillshot(.5f, 100, 2000, false, SkillshotType.SkillshotCircle);
                    return;
                case SpellSlot.E:
                    spell = new Spell(spell.Slot, 970);
                    spell.SetSkillshot(.366f, 70, 1600, true, SkillshotType.SkillshotLine);
                    return;
            }
        }

        public static bool IsGoodCastTarget(this Obj_AI_Hero hero, float range)
        {
            return hero != null && hero.IsValidTarget(range);
        }
    }
}