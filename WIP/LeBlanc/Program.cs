using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
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
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static Orbwalking.Orbwalker Orbwalker;
        public static Obj_AI_Hero Target;
        public static Obj_AI_Hero Player = ObjectManager.Player;
        public static SpellDataInst Ignite;
        public static Vector3 WPosition;
        public static float LastTroll;
        public static readonly string LeBlancWObject = "LeBlanc_Base_W_return_indicator.troy";

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

            #region SpellData

            Ignite = Player.Spellbook.GetSpell(Player.GetSpellSlot("summonerdot"));

            Q = new Spell(SpellSlot.Q, 700);
            Q.SetTargetted(.401f, 2000);

            W = new Spell(SpellSlot.W, 600);
            W.SetSkillshot(.5f, 100, 2000, false, SkillshotType.SkillshotCircle);

            E = new Spell(SpellSlot.E, 970);
            E.SetSkillshot(.366f, 70, 1600, true, SkillshotType.SkillshotLine);

            R = new Spell(SpellSlot.R);

            #endregion

            #region Menu

            Menu = new Menu("LeBlanc The Schemer", "LeBlanc", true);

            Menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(Menu.SubMenu("Orbwalker"));

            var ts = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(ts);
            Menu.AddSubMenu(ts);

            var combo = new Menu("Combo Settings", "Combo");

            var gapclose = combo.AddSubMenu(new Menu("GapClose", "Gap Close Combo"));
            //gapclose.AddItem(new MenuItem("Spacer", "This doesn't work yet"));
            //gapclose.AddItem(new MenuItem("GapCloseEnabled", "Use GapClose Combo").SetValue(true));
            //replace with damage calcs
            gapclose.AddItem(new MenuItem("TargetHP", "At Target HP %").SetValue(new Slider(40)));
            gapclose.AddItem(new MenuItem("PlayerHP", "Min Player HP %").SetValue(new Slider(40)));

            var comboQ = combo.AddSubMenu(new Menu("Q", "Q"));
            comboQ.AddItem(new MenuItem("ComboQ", "Use Q").SetValue(true));

            var comboW = combo.AddSubMenu(new Menu("W", "W"));
            comboW.AddItem(new MenuItem("ComboW", "Use W").SetValue(true));
            comboW.AddItem(new MenuItem("Spacer", "Set to 0% To Always W"));
            comboW.AddItem(new MenuItem("WMinHP", "Min HP To Use W").SetValue(new Slider(20)));

            var comboE = combo.AddSubMenu(new Menu("E", "E"));
            comboE.AddItem(new MenuItem("ComboE", "Use E").SetValue(true));
            comboE.AddItem(
                new MenuItem("eComboHitChance", "MinHitChance").SetValue(
                    new StringList(
                        new[] { HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString() }, 1)));

            var comboR = combo.AddSubMenu(new Menu("R", "R"));
            comboR.AddItem(new MenuItem("ComboR", "Use R").SetValue(true));
            comboR.AddItem(
                new MenuItem("ComboUltMode", "Ult Mode").SetValue(new StringList(new[] { SpellSlot.Q.ToString() })));
            // SpellSlot.W.ToString(), SpellSlot.E.ToString() })));

            combo.AddItem(new MenuItem("ComboQRange", "Only Combo in Q Range").SetValue(true));
            combo.AddItem(new MenuItem("ComboKey", "Combo Key").SetValue(new KeyBind(32, KeyBindType.Press)));
            Menu.AddSubMenu(combo);

            var harass = Menu.AddSubMenu(new Menu("Harass Settings", "Harass"));

            var harassQ = harass.AddSubMenu(new Menu("Q", "Q"));
            harassQ.AddItem(new MenuItem("HarassQ", "Use Q").SetValue(true));

            var harassW = harass.AddSubMenu(new Menu("W", "W"));
            harassW.AddItem(new MenuItem("HarassW", "Use W").SetValue(true));

            var harassE = harass.AddSubMenu(new Menu("E", "E"));
            harassE.AddItem(new MenuItem("HarassE", "Use E").SetValue(true));
            harassE.AddItem(
                new MenuItem("eHarassHitChance", "MinHitChance").SetValue(
                    new StringList(
                        new[] { HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString() }, 1)));


            /* var harassR = harass.AddSubMenu(new Menu("R", "R"));
            harassR.AddItem(new MenuItem("HarassR", "Use R").SetValue(true));
            */

            harass.AddItem(
                new MenuItem("SecondW", "Second W Setting").SetValue(
                    new StringList(new[] { "Manual", "Auto", "After E" })));
            harass.AddItem(new MenuItem("Harass", "Harass Key").SetValue(new KeyBind((byte) 'C', KeyBindType.Press)));

            var laneclear = Menu.AddSubMenu(new Menu("Farm Settings", "LaneClear"));
            laneclear.AddItem(new MenuItem("LaneClearQ", "Use Q").SetValue(true));
            laneclear.AddItem(new MenuItem("LaneClearQPercent", "Minimum Q Mana Percent").SetValue(new Slider(30)));
            laneclear.AddItem(
                new MenuItem("LaneClearActive", "Farm Key").SetValue(new KeyBind((byte) 'V', KeyBindType.Press)));

            var flee = Menu.AddSubMenu(new Menu("Flee Settings", "Flee"));
            var fleeW = flee.AddSubMenu(new Menu("W", "W"));
            fleeW.AddItem(new MenuItem("FleeW", "Use W").SetValue(true));

            var fleeE = flee.AddSubMenu(new Menu("E", "E"));
            fleeE.AddItem(new MenuItem("FleeE", "Use E").SetValue(true));
            fleeE.AddItem(
                new MenuItem("eFleefHitChance", "MinHitChance").SetValue(
                    new StringList(
                        new[] { HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString() }, 1)));

            var fleeR = flee.AddSubMenu(new Menu("R", "R"));
            fleeR.AddItem(new MenuItem("FleeRW", "Use W Ult").SetValue(true));
            flee.AddItem(new MenuItem("FleeKey", "Flee Key").SetValue(new KeyBind((byte) 'T', KeyBindType.Press)));

            var clone = Menu.AddSubMenu(new Menu("Clone Settings", "Clone"));
            clone.AddItem(new MenuItem("Enabled", "Enabled").SetValue(true));
            clone.AddItem(
                new MenuItem("Mode", "Mode").SetValue(
                    new StringList(new[] { "To Player", "To Target", "Away from Player" })));

            var draw = Menu.AddSubMenu(new Menu("Draw Settings", "Draw"));
            draw.AddItem(new MenuItem("DrawQ", "Draw Q Range").SetValue(new Circle(true, Color.Red, Q.Range)));
            draw.AddItem(new MenuItem("DrawW", "Draw W Range").SetValue(new Circle(false, Color.Red, W.Range)));
            draw.AddItem(new MenuItem("DrawE", "Draw E Range").SetValue(new Circle(true, Color.Purple, E.Range)));
            draw.AddItem(new MenuItem("DamageIndicator", "Damage Indicator").SetValue(true));
            draw.Item("DamageIndicator").ValueChanged += Program_ValueChanged;

            var misc = Menu.AddSubMenu(new Menu("Misc Settings", "Misc"));
            misc.AddItem(new MenuItem("MiscItems", "Use Items (DFG)").SetValue(true));
            misc.AddItem(new MenuItem("KS", "KillSteal with Spells").SetValue(true));
            misc.AddItem(new MenuItem("MiscW2", "Use Second W").SetValue(true));
            misc.AddItem(new MenuItem("MiscW2HP", "HP% to Use Second W").SetValue(new Slider(20)));
            misc.AddItem(new MenuItem("Interrupt", "Interrupt Spells").SetValue(true));
            misc.AddItem(new MenuItem("AntiGapcloser", "AntiGapCloser").SetValue(true));
            misc.AddItem(new MenuItem("Sounds", "Sounds").SetValue(true));
            misc.AddItem(new MenuItem("Troll", "Troll").SetValue(true));

            Menu.AddToMainMenu();

            #endregion

            DamageIndicator.DamageToUnit = GetComboDamage;
            DamageIndicator.Enabled = Menu.Item("DamageIndicator").GetValue<bool>();

            if (misc.Item("Sounds").GetValue<bool>())
            {
                var sound = new SoundObject(Resources.OnLoad);
                sound.Play();
            }

            Game.PrintChat(
                "<b><font color =\"#FFFFFF\">LeBlanc the Schemer by </font><font color=\"#0033CC\">Trees</font><font color =\"#FFFFFF\"> loaded!</font></b>");

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        #endregion

        #region Harass

        private static void Harass()
        {
            if (!Menu.Item("Harass").GetValue<KeyBind>().Active)
            {
                return;
            }

            var castQ = Menu.Item("HarassQ").GetValue<bool>() && Q.IsReady();
            var castW = Menu.Item("HarassW").GetValue<bool>() && W.IsReady();
            var castE = Menu.Item("HarassE").GetValue<bool>() && E.IsReady();
            //  var castR = Menu.Item("HarassR").GetValue<bool>();

            if (castQ && Q.CanCast(Target) && Q.Cast(Target).IsCast())
            {
                return;
            }

            if (castW && Player.HealthPercentage() > 20 && W.GetState(1) && W.InRange(Target, Q.Range) &&
                W.Cast(Target).IsCast())
            {
                return;
            }

            if (castE && E.CanCast(Target) && E.CastIfHitchanceEquals(Target, GetHitChance("eHarassHitChance")))
            {
                return;
            }

            if (W.GetState(1) || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.None)
            {
                return;
            }

            switch (W.GetToggleState())
            {
                case 1:
                    Player.Spellbook.CastSpell(SpellSlot.W);
                    break;
                case 2:
                    if (Target.Buffs.Any(buff => buff.Name.ToLower().Contains("leblancsoulshackle")))
                    {
                        Player.Spellbook.CastSpell(SpellSlot.W);
                    }
                    break;
            }
        }

        #endregion

        #region LaneClear

        private static void LaneClear()
        {
            var lcActive = Menu.Item("LaneClear").GetValue<KeyBind>().Active;
            var lcQ = Menu.Item("LaneClearQ").GetValue<bool>() && Q.IsReady();
            var lcQMana = Menu.Item("LaneClearQPercent").GetValue<Slider>().Value;
            var lowMana = Player.ManaPercentage() < lcQMana;

            if (!lcActive || !lcQ || lowMana)
            {
                return;
            }

            var unit =
                ObjectManager.Get<Obj_AI_Minion>()
                    .FirstOrDefault(
                        minion =>
                            minion.IsValidTarget(Q.Range) &&
                            minion.Health < Player.GetDamageSpell(minion, SpellSlot.Q).CalculatedDamage);

            if (unit.IsValidTarget(Q.Range))
            {
                Q.Cast(unit);
            }
        }

        #endregion

        #region Clone

        private static void CloneLogic()
        {
            var pet = Player.Pet as Obj_AI_Base;
            var usePet = Menu.SubMenu("Clone").Item("Enabled").GetValue<bool>();
            var petMode = Menu.SubMenu("Clone").Item("Mode").GetValue<StringList>().SelectedIndex;
            var valid = pet != null && pet.IsValid && !pet.IsDead && pet.Health > 1 && !pet.IsImmovable;

            if (!usePet || !valid)
            {
                return;
            }

            switch (petMode)
            {
                case 0: // toward player
                    var pos = Player.GetWaypoints().Count > 1 ? Player.GetWaypoints()[1].To3D() : Player.ServerPosition;
                    Utility.DelayAction.Add(200, () => { pet.IssueOrder(GameObjectOrder.MovePet, pos); });
                    break;
                case 1: //toward target
                    if (pet.CanAttack && !pet.IsWindingUp && Target.IsValidTarget(800)) // && !pet.IsAutoAttacking)
                    {
                        pet.IssueOrder(GameObjectOrder.AutoAttackPet, Target);
                    }
                    break;
                case 2: //away from player
                    Utility.DelayAction.Add(
                        100,
                        () =>
                        {
                            pet.IssueOrder(
                                GameObjectOrder.MovePet,
                                (pet.Position + 500 * ((pet.Position - Player.Position).Normalized())));
                        });
                    break;
            }
        }

        #endregion

        #region Flee

        private static void Flee()
        {
            var flee = Menu.Item("FleeKey").GetValue<KeyBind>().Active;

            if (!flee)
            {
                return;
            }

            var fleeW = Menu.Item("FleeW").GetValue<bool>() && W.IsReady() && W.GetState(1);
            var fleeE = Menu.Item("FleeE").GetValue<bool>() && E.IsReady();
            var fleeR = Menu.Item("FleeRW").GetValue<bool>() && R.IsReady(SpellSlot.W) &&
                        (W.GetState(2) || W.Instance.State != SpellState.Ready);
            var eHitChance = GetHitChance("eFleeHitChance");

            Orbwalker.ActiveMode = Orbwalking.OrbwalkingMode.None;

            if (fleeW)
            {
                W.Cast(Player.ServerPosition.Extend(Game.CursorPos, W.Range + 100));
                Utility.DelayAction.Add(
                    (int) (W.Delay * 1000f + 100f), () =>
                    {
                        Troll();
                        var d = Player.Distance(Game.CursorPos);
                        Player.IssueOrder(GameObjectOrder.MoveTo, Player.ServerPosition.Extend(Game.CursorPos, d + 250));
                    });
                return;
            }

            if (fleeE && Target.IsGoodCastTarget(E.Range) && E.GetPrediction(Target).Hitchance >= eHitChance)
            {
                E.Cast(Target);
                return;
            }

            if (!fleeR)
            {
                return;
            }

            Troll();

            R.Cast(SpellSlot.W, Player.ServerPosition.Extend(Game.CursorPos, W.Range + 100));

            Utility.DelayAction.Add(
                (int) (W.Delay * 1000f + 100f), () =>
                {
                    Troll();
                    var d = Player.Distance(Game.CursorPos);
                    Player.IssueOrder(GameObjectOrder.MoveTo, Player.ServerPosition.Extend(Game.CursorPos, d + 250));
                });
        }

        #endregion

        #region Events

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender == null || !sender.IsValid || !sender.Name.Equals(LeBlancWObject))
            {
                return;
            }

            WPosition = sender.Position;
        }

        private static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            if (sender == null || !sender.IsValid || !sender.Name.Equals(LeBlancWObject))
            {
                return;
            }

            WPosition = Vector3.Zero;
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var unit = gapcloser.Sender as Obj_AI_Hero;

            if (!Menu.Item("Interrupt").GetValue<bool>() || !unit.IsGoodCastTarget(E.Range) || !E.IsReady())
            {
                return;
            }

            E.CastIfHitchanceEquals(unit, HitChance.Medium);

            Utility.DelayAction.Add(
                (int) E.Delay * 1000 + 100, () =>
                {
                    if (R.IsReady(SpellSlot.E))
                    {
                        R.CastIfHitchanceEquals(SpellSlot.E, unit, HitChance.Medium);
                    }
                });
        }

        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            var interruptunit = unit as Obj_AI_Hero;

            if (!Menu.Item("Interrupt").GetValue<bool>() || !interruptunit.IsGoodCastTarget(E.Range) ||
                spell.DangerLevel < InterruptableDangerLevel.High || !E.IsReady())
            {
                return;
            }

            E.CastIfHitchanceEquals(unit, HitChance.Medium);

            Utility.DelayAction.Add(
                (int) E.Delay * 1000 + 100, () =>
                {
                    if (R.IsReady(SpellSlot.E))
                    {
                        R.CastIfHitchanceEquals(SpellSlot.E, interruptunit, HitChance.Medium);
                    }
                });
        }

        private static void Obj_AI_Base_OnProcessSpellCast(GameObject sender, GameObjectProcessSpellCastEventArgs args)
        {
            var s = sender as Obj_AI_Hero;

            if (s == null || !s.IsValid || !s.IsMe || args.SData == null)
            {
                return;
            }

            var targ = args.Target as Obj_AI_Hero;
            var comboMode = Menu.Item("ComboKey").GetValue<KeyBind>().Active;
            var harassMode = Menu.Item("Harass").GetValue<KeyBind>().Active;
            var isQSpell = args.SData.Name == Q.Instance.Name;
            var isWSpell = args.SData.Name == "LeblancSlide";
            var castR = Target.IsGoodCastTarget(400) && R.IsReady(SpellSlot.W);

            if (comboMode)
            {
                if (isQSpell && R.IsReady() && targ.IsValidTarget(Q.Range))
                {
                    Utility.DelayAction.Add(150, () => { R.Cast(SpellSlot.W, targ); });
                    return;
                }

                if (isWSpell && castR)
                {
                    R.Cast(SpellSlot.W, Target).IsCast();
                    return;
                }
            }

            if (harassMode && isWSpell)
            {
                Harass();
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }

            Flee();
            KSLogic();
            CloneLogic();
            SecondWLogic();

            Target = TargetSelector.GetTarget(1500, TargetSelector.DamageType.Magical);
            Target = Target.IsGoodCastTarget(1500)
                ? Target
                : TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (!Target.IsValidTarget(1500))
            {
                return;
            }

            LaneClear();
            Comboes();
            Harass();
        }

        #endregion

        #region Drawing

        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (var circle in
                new List<string> { "Q", "W", "E" }.Select(spell => Menu.Item("Draw" + spell).GetValue<Circle>())
                    .Where(circle => circle.Active))
            {
                Drawing.DrawCircle(Player.Position, circle.Radius, circle.Color);
            }
        }

        private static void Program_ValueChanged(object sender, OnValueChangeEventArgs e)
        {
            DamageIndicator.Enabled = e.GetNewValue<bool>();
        }

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (Q.IsReady())
            {
                var d = Player.GetSpellDamage(enemy, SpellSlot.Q);

                if (enemy.HasBuff("LeblancChaosOrb", true) || enemy.HasBuff("LeblancChaosOrbM", true))
                {
                    d *= 2;
                }

                damage += d;
            }

            if (R.IsReady())
            {
                var d = 0d;

                switch (R.GetSpellSlot())
                {
                    case SpellSlot.Q:
                        d = Player.GetSpellDamage(enemy, SpellSlot.Q);
                        break;
                    case SpellSlot.W:
                        d = Player.GetSpellDamage(enemy, SpellSlot.W);
                        break;
                    case SpellSlot.E:
                        d = Player.GetSpellDamage(enemy, SpellSlot.E);
                        break;
                }

                if (enemy.HasBuff("LeblancChaosOrb", true) || enemy.HasBuff("LeblancChaosOrbM", true))
                {
                    d *= 2;
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

            if (ItemData.Deathfire_Grasp.IsReady())
            {
                damage += .2f * damage + Player.GetItemDamage(enemy, Damage.DamageItems.Dfg);
            }

            if (ItemData.Blackfire_Torch.IsReady())
            {
                damage += .2f * damage + Player.GetItemDamage(enemy, Damage.DamageItems.BlackFireTorch);
            }

            if (ItemData.Frost_Queens_Claim.IsReady())
            {
                damage += Player.GetItemDamage(enemy, Damage.DamageItems.FrostQueenClaim);
            }

            if (Ignite.IsReady())
            {
                damage += Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
            }

            return (float) damage;
        }

        #endregion

        #region Combo

        private static void Comboes()
        {
            if (!Menu.Item("ComboKey").GetValue<KeyBind>().Active)
            {
                return;
            }

//            Console.WriteLine("COMBO");
            if (Target.IsValidTarget(Q.Range))
            {
                Combo();
                return;
            }

            var d = Player.Distance(Target);
            if (d > W.Range + 150 && d < W.Range * 2)
            {
                WCombo();
            }
        }

        #region Items

        private static void Items() {}

        #endregion

        private static void Combo()
        {
            var castQ = Menu.Item("ComboQ").GetValue<bool>() && Q.IsReady();
            var castW = Menu.Item("ComboW").GetValue<bool>() && W.IsReady();
            var castE = Menu.Item("ComboE").GetValue<bool>() && E.IsReady();
            var castR = Menu.Item("ComboR").GetValue<bool>() && R.IsReady(SpellSlot.Q);

            var castItems = Menu.Item("MiscItems").GetValue<bool>();

            if (!castItems)
            {
                return;
            }

            var spellsUp = Q.IsReady() && W.IsReady() && E.IsReady() && R.IsReady();
            var d = Player.Distance(Target);

            if (d <= 750 && spellsUp && (ItemData.Deathfire_Grasp.Cast(Target) || ItemData.Blackfire_Torch.Cast(Target)))
            {
                return;
            }

            if (d < ItemData.Frost_Queens_Claim.Range)
            {
                ItemData.Frost_Queens_Claim.Cast(Target);
            }


            if (castR && R.Cast(SpellSlot.Q, Target).IsCast())
            {
                return;
            }

            if (castQ && Q.CanCast(Target) && Q.Cast(Target).IsCast())
            {
                return;
            }

            if (castW && W.InRange(Target, Q.Range) && W.GetState(1) &&
                Player.HealthPercentage() >= Menu.Item("WMinHP").GetValue<Slider>().Value)
            {
                W.RandomizeCast(Target.Position);
                return;
            }

            if ((!W.IsReady() || W.GetState(2)) && castE && E.IsReady() && E.InRange(Target, 800))
            {
                E.CastIfHitchanceEquals(Target, GetHitChance("eComboHitChance"));
            }
        }

        private static void WCombo()
        {
            if (!W.IsReady() || W.GetState(2) || !R.IsReady() || !Target.IsGoodCastTarget(W.Range * 2 - 100) ||
                Target.HealthPercentage() > Menu.Item("TargetHP").GetValue<Slider>().Value ||
                Player.HealthPercentage() < Menu.Item("PlayerHP").GetValue<Slider>().Value)
            {
                return;
            }

            //  Console.WriteLine("LCOMBO");
            var pos = Player.Position.Extend(Target.Position, W.Range);
            W.Cast(pos);
        }

        #endregion

        #region Utilities

        private static void Troll()
        {
            if (!Menu.Item("Troll").GetValue<bool>() || Environment.TickCount - LastTroll < 1500)
            {
                return;
            }

            LastTroll = Environment.TickCount;
            Game.Say("/l");
        }

        private static int GetWMode()
        {
            return Menu.SubMenu("Harass").Item("SecondW").GetValue<StringList>().SelectedIndex;
        }

        private static HitChance GetHitChance(string name)
        {
            var hc = Menu.Item(name).GetValue<StringList>();
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

        private static void SecondWLogic()
        {
            if (!W.IsReady() || W.GetState(1) || Q.IsReady() || E.IsReady())
            {
                return;
            }

            var useSecondW = Menu.Item("MiscW2").GetValue<bool>();
            var wMinHP = Menu.Item("MiscW2HP").GetValue<Slider>().Value;
            var belowMinHealth = Player.HealthPercentage() < wMinHP;
            var moreEnemiesInRange = WPosition.CountEnemysInRange(500) > Player.CountEnemysInRange(500);
            var isFleeing = Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.None;

            if (!useSecondW || belowMinHealth || moreEnemiesInRange || isFleeing)
            {
                return;
            }

            Player.Spellbook.CastSpell(SpellSlot.W);
        }

        #endregion

        #region KS

        private static void KSLogic()
        {
            KSIgnite();

            if (!Menu.Item("KS").GetValue<bool>())
            {
                return;
            }

            KSR();

            if (Q.IsReady())
            {
                KSQ();
            }

            if (W.IsReady() && W.GetState(1))
            {
                KSW();
            }

            if (E.IsReady())
            {
                KSE();
            }
        }

        private static void KSQ(bool ult = false)
        {
            var unit =
                ObjectManager.Get<Obj_AI_Hero>()
                    .FirstOrDefault(
                        obj => obj.IsValidTarget(Q.Range) && obj.Health < Player.GetSpellDamage(obj, SpellSlot.Q));

            if (!unit.IsValidTarget(Q.Range))
            {
                return;
            }

            if (ult && R.Cast(SpellSlot.Q, unit).IsCast())
            {
                return;
            }

            Q.Cast(unit);
        }

        private static void KSW(bool ult = false)
        {
            var unit =
                ObjectManager.Get<Obj_AI_Hero>()
                    .FirstOrDefault(
                        obj => obj.IsValidTarget(W.Range) && obj.Health < Player.GetSpellDamage(obj, SpellSlot.W));

            if (!unit.IsValidTarget(W.Range))
            {
                return;
            }

            if (ult && R.Cast(SpellSlot.W, unit).IsCast())
            {
                R.Cast(unit);
                return;
            }

            W.Cast(unit);
        }

        private static void KSE(bool ult = false)
        {
            var unit =
                ObjectManager.Get<Obj_AI_Hero>()
                    .FirstOrDefault(
                        obj => obj.IsValidTarget(E.Range) && obj.Health < Player.GetSpellDamage(obj, SpellSlot.E));

            if (!unit.IsValidTarget(E.Range) || E.GetPrediction(unit).Hitchance < HitChance.High)
            {
                return;
            }

            if (ult && R.Cast(SpellSlot.E, unit).IsCast())
            {
                return;
            }

            E.Cast(unit);
        }

        private static void KSR()
        {
            if (!R.IsReady())
            {
                return;
            }

            switch (R.GetSpellSlot())
            {
                case SpellSlot.Q:
                    KSQ(true);
                    return;
                case SpellSlot.W:
                    KSW(true);
                    return;
                case SpellSlot.E:
                    KSE(true);
                    return;
                case SpellSlot.R:
                    return;
            }
        }

        private static void KSIgnite()
        {
            if (Ignite == null || Ignite.Slot == SpellSlot.Unknown || !Ignite.Slot.IsReady())
            {
                return;
            }

            var unit =
                ObjectManager.Get<Obj_AI_Hero>()
                    .FirstOrDefault(
                        obj =>
                            obj.IsValidTarget(600) &&
                            obj.Health < Player.GetSummonerSpellDamage(obj, Damage.SummonerSpell.Ignite));

            if (unit.IsValidTarget(600))
            {
                Player.Spellbook.CastSpell(Ignite.Slot, unit);
            }
        }

        #endregion
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