using System;
using System.IO;
using System.Linq;
using System.Media;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace LeBlanc
{
    internal static class Utils
    {
        public static float LastTroll;

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
            return slot != null && slot.SpellSlot != SpellSlot.Unknown && slot.SpellSlot.IsReady() &&
                   ObjectManager.Player.Spellbook.CastSpell(slot.SpellSlot, target);
        }

        public static bool HasItem(this ItemData.Item item)
        {
            var slot = item.GetItemSlot();
            return slot != null && slot.IsValidSlot();
        }

        public static bool IsReady(this InventorySlot slot)
        {
            return slot != null && slot.IsValidSlot() && slot.IsReady();
        }

        public static bool IsReady(this ItemData.Item item)
        {
            var slot = item.GetItemSlot();
            return slot != null && slot.IsValidSlot() && slot.SpellSlot.IsReady();
        }

        private static InventorySlot GetItemSlot(this ItemData.Item item)
        {
            return ObjectManager.Player.InventoryItems.FirstOrDefault(i => i.Id == (ItemId) item.Id);
        }

        public static void RandomizeCast(this Spell spell, Vector3 position)
        {
            var rnd = new Random(Environment.TickCount);
            var randomVector = new Vector2(rnd.Next(75), rnd.Next(75)).To3D();
            spell.Cast(position + randomVector);
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

        private static void SetSpell(this Spell spell, SpellSlot slot)
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

        public static bool IsActive(this Orbwalking.OrbwalkingMode mode)
        {
            return mode != Orbwalking.OrbwalkingMode.None;
        }

        public static bool HasEBuff(this Obj_AI_Base unit)
        {
            return unit.HasBuff("leblancsoulshackle", true, true);
        }

        public static bool HasQBuff(this Obj_AI_Base unit)
        {
            return unit.HasBuff("leblancchaosorb", true, true);
        }

        public static bool HasQRBuff(this Obj_AI_Base unit)
        {
            return unit.HasBuff("leblancchaosorbm", true, true);
        }

        public static SpellDataInst[] GetMainSpells(this Spellbook spellbook)
        {
            return new[]
            {
                spellbook.GetSpell(SpellSlot.Q), spellbook.GetSpell(SpellSlot.W), spellbook.GetSpell(SpellSlot.E),
                spellbook.GetSpell(SpellSlot.R)
            };
        }

        private static int GetToggleState(this Spell spell)
        {
            return spell.Instance.ToggleState;
        }

        public static Spell.CastStates Cast(this Spell spell, SpellSlot slot, Obj_AI_Base unit)
        {
            spell.SetSpell(slot);
            return spell.Cast(unit);
        }

        public static bool Cast(this Spell spell, SpellSlot slot, Vector3 position)
        {
            spell.SetSpell(slot);
            return spell.Cast(position);
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

        public static bool IsReady(this Spell spell, int stage)
        {
            return spell.IsReady() && spell.GetToggleState() == stage;
        }

        public static HitChance GetHitChance(string name)
        {
            var hc = Program.Menu.Item(name).GetValue<StringList>();
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

        public static Obj_AI_Hero GetTarget()
        {
            var target = TargetSelector.GetTarget(1500, TargetSelector.DamageType.Magical);
            return target.IsValidTarget(1500)
                ? target
                : TargetSelector.GetTarget(Spells.W.Range * 2, TargetSelector.DamageType.Magical);
        }

        public static void Troll()
        {
            if (!Program.Menu.Item("Troll").GetValue<bool>() || Environment.TickCount - LastTroll < 1500)
            {
                return;
            }

            LastTroll = Environment.TickCount;
            Game.Say("/l");
        }
    }

    internal class SoundObject
    {
        public static float LastPlayed;
        private static SoundPlayer _sound;

        public SoundObject(Stream sound)
        {
            LastPlayed = 0;
            _sound = new SoundPlayer(sound);
        }

        public void Play()
        {
            if (Environment.TickCount - LastPlayed < 1500)
            {
                return;
            }
            _sound.Play();
            LastPlayed = Environment.TickCount;
        }
    }
}