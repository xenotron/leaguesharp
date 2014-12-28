using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

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
    }
}