using System;
using System.Linq;
using System.Runtime.CompilerServices;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace LeBlanc
{
    internal static class Utils
    {
        public static void Cast(this ItemId id)
        {
            id.GetItemSlot()._cast(ObjectManager.Player);
        }

        public static void Cast(this ItemId id, Obj_AI_Base target)
        {
            id.GetItemSlot()._cast(target);
        }

        private static void _cast(this InventorySlot slot, Obj_AI_Base target)
        {
            if (slot != null && slot.CanCast())
            {
                ObjectManager.Player.Spellbook.CastSpell(slot.SpellSlot, target);
            }
        }

        private static InventorySlot GetItemSlot(this ItemId id)
        {
            return ObjectManager.Player.InventoryItems.FirstOrDefault(i => i.Id == id);
        }

        public static bool CanCast(this InventorySlot slot)
        {
            return ObjectManager.Player.Spellbook.GetSpell(slot.SpellSlot).IsReady();
        }

        public static void RandomizeCast(this Spell spell, Vector3 position)
        {
            var rnd = new Random(Environment.TickCount);
            var pos = new Vector2(position.X + rnd.Next(25), position.Y + rnd.Next(25)).To3D();
            spell.Cast(pos);
        }
    }
}