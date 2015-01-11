using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using LeBlanc.Properties;
using SharpDX;
using Color = System.Drawing.Color;

namespace LeBlanc
{
    internal class Program
    {
        public static Menu Menu;
        public static Orbwalking.Orbwalker Orbwalker;
        public static Obj_AI_Hero Player = ObjectManager.Player;

        public static Spell Q
        {
            get { return Spells.Q; }
        }

        public static Spell E
        {
            get { return Spells.E; }
        }

        public static Spell W
        {
            get { return Spells.W; }
        }

        public static Spell R
        {
            get { return Spells.R; }
        }

        public static void Main(string[] args)
        {
            LeagueSharp.Common.Utils.ClearConsole();
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        #region Load

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (!Player.ChampionName.Equals("Leblanc"))
            {
                return;
            }

            #region Menu

            Menu = new Menu("LeBlanc The Schemer", "LeBlanc", true);

            Menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(Menu.SubMenu("Orbwalker"));

            var ts = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(ts);
            Menu.AddSubMenu(ts);

            var combo = new Combo();
            Menu.AddSubMenu(Combo.LocalMenu);

            var harass = new Harass();
            Menu.AddSubMenu(Harass.LocalMenu);

            var laneclear = new LaneClear();
            Menu.AddSubMenu(LaneClear.LocalMenu);

            var flee = new Flee();
            Menu.AddSubMenu(Flee.LocalMenu);

            var clone = new Clone();
            Menu.AddSubMenu(Clone.LocalMenu);

            var draw = Menu.AddSubMenu(new Menu("Draw Settings", "Draw"));
            draw.AddItem(new MenuItem("Draw0", "Draw Q Range").SetValue(new Circle(true, Color.Red, Q.Range)));
            draw.AddItem(new MenuItem("Draw1", "Draw W Range").SetValue(new Circle(false, Color.Red, W.Range)));
            draw.AddItem(new MenuItem("Draw2", "Draw E Range").SetValue(new Circle(true, Color.Purple, E.Range)));
            draw.AddItem(new MenuItem("DrawCD", "Draw on CD").SetValue(true));
            draw.AddItem(new MenuItem("DamageIndicator", "Damage Indicator").SetValue(true));

            var misc = Menu.AddSubMenu(new Menu("Misc Settings", "Misc"));

            var ks = new KillSteal();
            misc.AddSubMenu(KillSteal.LocalMenu);

            misc.AddItem(new MenuItem("Interrupt", "Interrupt Spells").SetValue(true));
            misc.AddItem(new MenuItem("AntiGapcloser", "AntiGapCloser").SetValue(true));
            misc.AddItem(new MenuItem("Sounds", "Sounds").SetValue(true));
            misc.AddItem(new MenuItem("Troll", "Troll").SetValue(true));

            Menu.AddToMainMenu();

            #endregion

            DamageIndicator.DamageToUnit = GetComboDamage;

            if (misc.Item("Sounds").GetValue<bool>())
            {
                var sound = new SoundObject(Resources.OnLoad);
                sound.Play();
            }

            Game.PrintChat(
                "<b><font color =\"#FFFFFF\">LeBlanc the Schemer by </font><font color=\"#0033CC\">Trees</font><font color =\"#FFFFFF\"> loaded!</font></b>");

            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
        }

        #endregion

        #region Events

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var unit = gapcloser.Sender as Obj_AI_Hero;

            if (!Menu.Item("Interrupt").GetValue<bool>() || !unit.IsValidTarget(E.Range) || !E.IsReady())
            {
                return;
            }

            E.CastIfHitchanceEquals(unit, HitChance.Medium);

            Utility.DelayAction.Add(
                (int) E.Delay * 1000 + Game.Ping / 2 + 50, () =>
                {
                    if (R.IsReady(SpellSlot.E))
                    {
                        R.CastIfHitchanceEquals(SpellSlot.E, unit, HitChance.Medium);
                    }
                });
        }

        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            var hero = unit as Obj_AI_Hero;

            if (!Menu.Item("Interrupt").GetValue<bool>() || !hero.IsValidTarget(E.Range) ||
                spell.DangerLevel < InterruptableDangerLevel.High || !E.IsReady())
            {
                return;
            }

            E.CastIfHitchanceEquals(hero, HitChance.Medium);

            Utility.DelayAction.Add(
                (int) E.Delay * 1000 + Game.Ping / 2 + 50, () =>
                {
                    if (R.IsReady(SpellSlot.E))
                    {
                        R.CastIfHitchanceEquals(SpellSlot.E, hero, HitChance.Medium);
                    }
                });
        }

        #endregion

        #region Drawing

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Combo.WPosition != Vector3.Zero)
            {
                Render.Circle.DrawCircle(Combo.WPosition, 200, Color.Red, 8);
            }

            foreach (var spell in
                Player.Spellbook.GetMainSpells().Where(s => s.IsReady() || Menu.Item("DrawCD").GetValue<bool>()))
            {
                try
                {
                    var circle = Menu.Item("Draw" + (int) spell.Slot).GetValue<Circle>();
                    if (circle.Active && spell.Level > 0)
                    {
                        Render.Circle.DrawCircle(
                            Player.Position, circle.Radius, spell.IsReady() ? circle.Color : Color.Red);
                    }
                }
                catch {}
            }
        }

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (Q.IsReady())
            {
                var d = Player.GetSpellDamage(enemy, SpellSlot.Q);

                if (enemy.HasQBuff() || enemy.HasQRBuff())
                {
                    d *= 2;
                }

                damage += d;
            }

            if (R.IsReady())
            {
                var d = 0d;
                var level = R.Instance.Level;
                var maxDamage = new double[] { 200, 400, 600 }[level] + 1.3f * Player.FlatMagicDamageMod;

                switch (R.GetSpellSlot())
                {
                    case SpellSlot.Q:
                        var qDmg = Player.CalcDamage(
                            enemy, Damage.DamageType.Magical,
                            new double[] { 100, 200, 300 }[level] + .65f * Player.FlatMagicDamageMod);
                        d = qDmg > maxDamage ? maxDamage : qDmg;
                        break;
                    case SpellSlot.W:
                        d = Player.CalcDamage(
                            enemy, Damage.DamageType.Magical,
                            new double[] { 150, 300, 450 }[level] + .975f * Player.FlatMagicDamageMod);
                        break;
                    case SpellSlot.E:
                        var eDmg = Player.CalcDamage(
                            enemy, Damage.DamageType.Magical,
                            new double[] { 100, 200, 300 }[level] + .65f * Player.FlatMagicDamageMod);
                        d = eDmg > maxDamage ? maxDamage : eDmg;
                        break;
                }

                if (enemy.HasQBuff() || enemy.HasQRBuff())
                {
                    d += Player.GetSpellDamage(enemy, SpellSlot.Q);
                }

                damage += d;
            }

            if (E.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);
            }

            if (W.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.W);
            }

            if (Items.DFG.IsReady())
            {
                damage += .2f * damage + Player.GetItemDamage(enemy, Damage.DamageItems.Dfg);
            }

            if (Items.BFT.IsReady())
            {
                damage += .2f * damage + Player.GetItemDamage(enemy, Damage.DamageItems.BlackFireTorch);
            }

            if (Items.FQC.IsReady())
            {
                damage += Player.GetItemDamage(enemy, Damage.DamageItems.FrostQueenClaim);
            }

            if (Items.BOTRK.IsReady())
            {
                damage += Player.GetItemDamage(enemy, Damage.DamageItems.Botrk);
            }

            if (Items.LT.HasItem())
            {
                damage += Player.GetItemDamage(enemy, Damage.DamageItems.LiandrysTorment);
            }

            if (Spells.Ignite.IsReady())
            {
                damage += Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
            }

            return (float) damage;
        }

        #endregion
    }
}