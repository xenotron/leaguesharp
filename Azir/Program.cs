using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Network.Packets;
using SharpDX;
using Color = System.Drawing.Color;
using Packet = LeagueSharp.Network.Packets.Packet;

namespace Azir
{
    internal class Program
    {
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static Spell Ignite;
        public static List<Obj_AI_Minion> AzirSoldier = new List<Obj_AI_Minion>();
        public static Obj_AI_Hero Player = ObjectManager.Player;
        public static Menu Menu;
        public static Orbwalking.Orbwalker Orbwalker;
        public static float SoldierLeashRange = 1500f;

        private static void Main(string[] args)
        {
            LeagueSharp.Common.Utils.ClearConsole();
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Azir")
            {
                return;
            }

            //lane clear using line vectors
            Q = new Spell(SpellSlot.Q, 800);
            Q.SetSkillshot(.25f, 0, 500, false, SkillshotType.SkillshotLine);

            //leash range
            W = new Spell(SpellSlot.W, 450);
            W.SetSkillshot(.25f, 100, 500, false, SkillshotType.SkillshotCircle);

            //e knockup range
            E = new Spell(SpellSlot.E);
            E.SetTargetted(.2f, 800f);

            //knock back range
            R = new Spell(SpellSlot.R, 250);
            R.SetSkillshot(.5f, 200f, 500f, false, SkillshotType.SkillshotLine);

            var slot = Player.GetSpellSlot("summonerdot");
            if (slot != SpellSlot.Unknown)
            {
                Ignite = new Spell(slot, 600);
                //Ignite.SetTargetted();
            }

            #region Menu

            Menu = new Menu("Azir", "Azir", true);

            Menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(Menu.SubMenu("Orbwalker"));

            var combo = new Menu("Combo Settings", "Combo");

            var comboQ = combo.AddSubMenu(new Menu("Q", "Q"));
            comboQ.AddItem(new MenuItem("ComboQ", "Use Q").SetValue(true));

            var comboW = combo.AddSubMenu(new Menu("W", "W"));
            comboW.AddItem(new MenuItem("ComboW", "Use W").SetValue(true));

            var comboE = combo.AddSubMenu(new Menu("E", "E"));
            comboE.AddItem(new MenuItem("ComboE", "Use E").SetValue(true));

            var comboR = combo.AddSubMenu(new Menu("R", "R"));
            comboR.AddItem(new MenuItem("ComboR", "Use R").SetValue(true));

            combo.AddItem(new MenuItem("ComboKey", "Combo Key").SetValue(new KeyBind(32, KeyBindType.Press)));

            Menu.AddSubMenu(combo);

            var harass = Menu.AddSubMenu(new Menu("Harass Settings", "Harass"));

            var harassQ = harass.AddSubMenu(new Menu("Q", "Q"));
            harassQ.AddItem(new MenuItem("HarassQ", "Use Q").SetValue(true));

            var harassW = harass.AddSubMenu(new Menu("W", "W"));
            harassW.AddItem(new MenuItem("HarassW", "Use W").SetValue(true));

            var harassE = harass.AddSubMenu(new Menu("E", "E"));
            harassE.AddItem(new MenuItem("HarassE", "Use E").SetValue(false));

            harass.AddItem(new MenuItem("HarassKey", "Harass Key").SetValue(new KeyBind((byte) 'C', KeyBindType.Press)));

            var flee = Menu.AddSubMenu(new Menu("Flee Settings", "Flee"));

            var fleeQ = flee.AddSubMenu(new Menu("Q", "Q"));
            fleeQ.AddItem(new MenuItem("FleeQ", "Use Q").SetValue(true));

            var fleeW = flee.AddSubMenu(new Menu("W", "W"));
            fleeW.AddItem(new MenuItem("FleeW", "Use W").SetValue(true));

            var fleeE = flee.AddSubMenu(new Menu("E", "E"));
            fleeE.AddItem(new MenuItem("FleeE", "Use E").SetValue(true));

            flee.AddItem(new MenuItem("FleeKey", "Flee Key").SetValue(new KeyBind((byte) 'T', KeyBindType.Press)));

            var misc = Menu.AddSubMenu(new Menu("Misc", "Misc Settings"));
            misc.AddItem(new MenuItem("WTurret", "Use W to kill Turrets").SetValue(true));
            misc.AddItem(new MenuItem("Passive", "Use Passive").SetValue(true));
            misc.AddItem(new MenuItem("Sounds", "Sounds").SetValue(true));

            Menu.AddToMainMenu();

            #endregion

            if (Menu.Item("Sounds").GetValue<bool>())
            {
                var sound = new SoundObject(Properties.Resources.OnLoad);
                sound.Play();
            }

            Game.PrintChat(
                "<b><font color =\"#FFFFFF\">Azir the Emperor by </font><font color=\"#0033CC\">Trees</font><font color =\"#FFFFFF\"> loaded!</font></b>");


            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
            Game.OnGameUpdate += Game_OnGameUpdate;
            //Game.OnGameSendPacket += Game_OnGameSendPacket;
            // Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Game_OnGameSendPacket(GamePacketEventArgs args)
        {
            if (args.GetPacketId().Equals(Packet.GetPacketId<PKT_InteractReq>()))
            {
                var dp = new PKT_InteractReq();
                dp.Decode(args);
                var unit = ObjectManager.GetUnitByNetworkId<GameObject>(dp.TargetNetworkId);
                Console.WriteLine(unit.Name);
                Console.Write(unit.Type);
            }
        }

        #region Drawing

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Q.IsReady() && (Player.Spellbook.SelectedSpellSlot == SpellSlot.Q))
            {
                var targetPos = Game.CursorPos;

                foreach (var soldier in AzirSoldier.Where(h => h.IsValid && !h.IsDead))
                {
                    var d = Player.Distance(soldier);
                    d = d > Q.Range ? soldier.Distance(targetPos) : Q.Range;

                    var pos1 = Drawing.WorldToScreen(soldier.ServerPosition);
                    var vector = soldier.ServerPosition.Extend(targetPos, d);
                    var pos2 = Drawing.WorldToScreen(vector);

                    Drawing.DrawCircle(soldier.Position, 200, Color.Green);
                    Drawing.DrawCircle(vector, 200, Color.Blue);
                    Drawing.DrawLine(pos1, pos2, 3, Color.Red);
                }
            }
        }

        #endregion

        #region Harass

        private static void Harass()
        {
            var qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (Menu.Item("HarassKey").GetValue<KeyBind>().Active)
            {
                return;
            }

            if (W.IsReady() && Menu.Item("HarassW").GetValue<bool>())
            {
                var wTarget = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);

                if (wTarget.IsGoodCastTarget(W.Range))
                {
                    W.Cast(wTarget);
                }
            }

            if (AzirSoldier.Count < 1)
            {
                return;
            }

            if (Q.IsReady() && Menu.Item("HarassQ").GetValue<bool>())
            {
                if (qTarget.IsGoodCastTarget(Q.Range))
                {
                    Q.Cast(qTarget);
                    return;
                }
            }

            if (!E.IsReady() || !Menu.Item("HarassE").GetValue<bool>() || !qTarget.IsGoodCastTarget(Q.Range))
            {
                return;
            }

            var eTarget = GetETargets(qTarget.ServerPosition).FirstOrDefault();

            if (eTarget == null || !eTarget.IsValid)
            {
                return;
            }

            E.Cast(eTarget.ServerPosition);
        }

        #endregion

        #region Combo

        private static void Combo()
        {
            var qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (!Menu.Item("ComboKey").GetValue<KeyBind>().Active)
            {
                return;
            }

            if (W.IsReady() && Menu.Item("ComboW").GetValue<bool>())
            {
                var wTarget = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);

                if (wTarget.IsGoodCastTarget(W.Range))
                {
                    W.Cast(wTarget);
                    return;
                }
            }

            if (R.IsReady() && Menu.Item("ComboR").GetValue<bool>())
            {
                var rTarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

                if (rTarget.IsGoodCastTarget(R.Range))
                {
                    R.Cast(rTarget);
                    return;
                }
            }

            if (AzirSoldier.Count < 1)
            {
                return;
            }

            if (Q.IsReady() && Menu.Item("ComboQ").GetValue<bool>())
            {
                if (qTarget.IsGoodCastTarget(Q.Range))
                {
                    Q.Cast(qTarget);
                    return;
                }
            }

            if (!E.IsReady() || !Menu.Item("ComboE").GetValue<bool>() || !qTarget.IsGoodCastTarget(Q.Range))
            {
                return;
            }

            var eTarget = GetETargets(qTarget.ServerPosition).FirstOrDefault();

            if (eTarget == null || !eTarget.IsValid)
            {
                return;
            }

            E.Cast(eTarget.ServerPosition);
        }

        #endregion

        #region TurretLogic

        private static void TurretLogic()
        {
            if (!Menu.Item("WTurret").GetValue<bool>() || !W.IsReady())
            {
                return;
            }

            var turret =
                ObjectManager.Get<Obj_AI_Turret>()
                    .FirstOrDefault(h => h.IsValidTarget(W.Range) && h.Health < GetWDamage(h));

            if (turret != null && turret.IsValid)
            {
                W.Cast(turret.ServerPosition);
            }

            UsePassive();
        }

        #endregion

        #region Flee

        public static void Flee()
        {
            if (!Menu.Item("FleeKey").GetValue<KeyBind>().Active)
            {
                return;
            }

            Orbwalker.ActiveMode = Orbwalking.OrbwalkingMode.None;

            if (E.IsReady() && Menu.Item("FleeE").GetValue<bool>())
            {
                var eTarget = GetETargets(Game.CursorPos).FirstOrDefault();

                if (eTarget == null || !eTarget.IsValid)
                {
                    if (W.IsReady() && Menu.Item("FleeW").GetValue<bool>())
                    {
                        W.Cast(Player.ServerPosition.Extend(Game.CursorPos, W.Range));
                    }
                    Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                    return;
                }

                if (Q.IsReady() && Menu.Item("FleeQ").GetValue<bool>())
                {
                    Q.Cast(Game.CursorPos);
                    return;
                }

                E.Cast(eTarget.ServerPosition);
                var d = Player.ServerPosition.Distance(Game.CursorPos);
                Player.IssueOrder(GameObjectOrder.MoveTo, Player.ServerPosition.Extend(Game.CursorPos, d + 250));
            }
        }

        #endregion

        #region Events

        private static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            if (sender == null || !sender.IsValid<Obj_AI_Minion>() || !sender.Name.Equals("AzirSoldier"))
            {
                return;
            }
            foreach (var soldier in AzirSoldier.Where(soldier => soldier.NetworkId == sender.NetworkId))
            {
                AzirSoldier.Remove(soldier);
                return;
            }
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender == null || !sender.IsValid<Obj_AI_Minion>() || !sender.Name.Equals("AzirSoldier"))
            {
                return;
            }

            var s = sender as Obj_AI_Minion;

            if (s == null || !s.IsValid || !s.BaseSkinName.Equals("AzirSoldier"))
            {
                return;
            }

            AzirSoldier.Add(s);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            PurgeSoldiers();
            TurretLogic();
            UsePassive();
            Harass();
            Combo();
            Flee();
        }

        #endregion

        #region Utilities

        private static void PurgeSoldiers()
        {
            if (AzirSoldier == null || AzirSoldier.Count < 1)
            {
                return;
            }

            AzirSoldier.RemoveAll(h => !h.IsValid || h.IsDead);
        }

/*        public static List<List<Vector3>> GetQPredictions(Vector3 position)
        {
            foreach (var soldier in AzirSoldier.Where(h => h.IsValid && !h.IsDead))
            {
                var d = Player.Distance(soldier);
                d = d > Q.Range ? soldier.Distance(position) : Q.Range;

                var startPos = soldier.ServerPosition;
                var endPos = soldier.ServerPosition.Extend(position, d);
                var line = new Geometry.Line(startPos.To2D(), endPos.To2D(), startPos.Distance(endPos));
                //   line.ToPolygon().Points
            }
        }
        */

        public static double GetWDamage(Obj_AI_Turret turret)
        {
            return Player.CalcDamage(
                turret, Damage.DamageType.Magical, 80 + 25 * Player.Level + .6 * Player.FlatMagicDamageMod);
        }

        private static IEnumerable<Obj_AI_Minion> GetETargets(Vector3 position)
        {
            return position == Vector3.Zero
                ? null
                : AzirSoldier.Where(obj => obj.IsValid).OrderByDescending(obj => obj.Distance(position)).ToList();
        }

        public static void UsePassive()
        {
            if (Menu.Item("UsePassive").GetValue<bool>())
            {
                var turret =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(
                            h =>
                                h.IsValid && h.Name == "TowerClicker" && h.Health < 1 &&
                                Player.Distance(h.ServerPosition) < 800)
                        .OrderByDescending(h => Player.Distance(h.ServerPosition))
                        .FirstOrDefault();

                if (turret == null || !turret.IsValid)
                {
                    return;
                }

                var p = new PKT_InteractReq { NetworkId = Player.NetworkId, TargetNetworkId = turret.NetworkId };
                p.Encode().SendAsPacket();
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