using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace LeBlanc
{
    internal static class Utils
    {
        public static bool Cast(this ItemData.Item item)
        {
            return item._cast(ObjectManager.Player);
        }

        public static bool Cast(this ItemData.Item item, Obj_AI_Base target)
        {
            return item._cast(target);
        }

        private static bool _cast(this ItemData.Item item, GameObject target)
        {
            var slot = item.GetItemSlot();
            return slot.IsReady() &&
                   ObjectManager.Player.Spellbook.CastSpell(slot.SpellSlot, target);
        }

        public static bool IsReady(this InventorySlot slot)
        {
            return slot.IsValidSlot() && slot.IsReady();
        }

        public static bool IsReady(this ItemData.Item item)
        {
            var slot = item.GetItemSlot();
            return slot.IsValidSlot() && slot.SpellSlot.IsReady();
        }


        private static InventorySlot GetItemSlot(this ItemData.Item item)
        {
            return ObjectManager.Player.InventoryItems.FirstOrDefault(i => i.Id == (ItemId)item.Id);
        }

        public static void RandomizeCast(this Spell spell, Vector3 position)
        {
            var rnd = new Random(Environment.TickCount);
            var randomVector = new Vector2(rnd.Next(75), rnd.Next(75)).To3D();
            spell.Cast(position + randomVector);
        }

        public static bool GetState(this Spell spell, int state)
        {
            return spell.Instance.ToggleState == state;
        }

        public static SpellSlot GetSpellSlot(this Spell spell)
        {
            if (!spell.IsReady() || spell.Instance.Name == null)
            {
                return SpellSlot.R;
            }

            //leblancslidereturnm
            switch (spell.Instance.Name)
            {
                case "LeblancChaosOrbM":
                    return SpellSlot.Q;
                case "LeblancSlideM":
                    return SpellSlot.W;
                case "LeblancSoulShackleM":
                    return SpellSlot.E;
                default:
                    return SpellSlot.R;
            }
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

        public static int GetToggleState(this Spell spell)
        {
            return spell.Instance.ToggleState;
        }

        public static Spell.CastStates Cast(this Spell spell, SpellSlot slot, Obj_AI_Base unit)
        {
            spell.SetSpell(slot);
            return spell.Cast(unit);
        }

        public static void Cast(this Spell spell, SpellSlot slot, Vector3 position)
        {
            spell.SetSpell(slot);
            spell.Cast(position);
        }

        public static bool CastIfHitchanceEquals(this Spell spell, SpellSlot slot, Obj_AI_Base unit, HitChance hitchance)
        {
            spell.SetSpell(slot);
            return spell.CastIfHitchanceEquals(unit, hitchance);
        }

        public static bool IsCast(this Spell.CastStates state)
        {
            return state == Spell.CastStates.SuccessfullyCasted;
        }

        public static bool IsReady(this Spell spell, SpellSlot slot)
        {
            return spell.IsReady() && spell.GetSpellSlot() == slot;
        }

        public static bool IsGoodCastTarget(this Obj_AI_Hero hero, float range)
        {
            return hero != null && hero.IsValidTarget(range);
        }
    }
}