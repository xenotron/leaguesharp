#region

using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.XInput;
using Color = System.Drawing.Color;

#endregion

namespace ControlSharp
{
    internal class Program
    {
        public static int[] ControllerArray = { 0, 1, 2, 3, 4 };
        public static Menu Menu;

        public static Orbwalking.Orbwalker OrbWalker;
        public static Orbwalking.OrbwalkingMode CurrentMode = Orbwalking.OrbwalkingMode.None;

        public static GamepadState Controller;
        public static Render.Circle CurrentPosition;
        public static Render.Text Text;
        public static float MaxD = 0;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            foreach (var c in
                ControllerArray.Select(controlId => new Controller((UserIndex) controlId)).Where(c => c.IsConnected))
            {
                Controller = new GamepadState(c.UserIndex);
            }

            if (Controller == null || !Controller.Connected)
            {
                Game.PrintChat("No controller detected!");
                return;
            }

            Menu = new Menu("ControllerTest", "ControllerTest", true);
            OrbWalker = new Orbwalking.Orbwalker(Menu);
            Menu.AddItem(new MenuItem("Draw", "Draw Circle").SetValue(true));
            Menu.AddToMainMenu();

            if (Menu.Item("Draw").GetValue<bool>())
            {
                CurrentPosition = new Render.Circle(ObjectManager.Player.Position, 100, Color.Red, 2);
                CurrentPosition.Add();
                Text = new Render.Text(new Vector2(50, 50), "MODE: " + CurrentMode, 30, new ColorBGRA(255, 0, 0, 255));
                Text.OutLined = true;
                Text.Add();
            }

            Game.PrintChat(
                "<b><font color =\"#FFFFFF\">ControlSharp by </font><font color=\"#5C00A3\">Trees</font><font color =\"#FFFFFF\"> loaded!</font></b>");

            Menu.Item("Draw").ValueChanged += OnValueChanged;
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        private static void OnValueChanged(object sender, OnValueChangeEventArgs onValueChangeEventArgs)
        {
            if (onValueChangeEventArgs.GetNewValue<bool>())
            {
                CurrentPosition = new Render.Circle(ObjectManager.Player.Position, 100, Color.Red, 2);
                CurrentPosition.Add();
                Text = new Render.Text(new Vector2(50, 50), "MODE: " + CurrentMode, 30, new ColorBGRA(255, 0, 0, 255));
                Text.OutLined = true;
                Text.Add();
            }
            else
            {
                CurrentPosition.Remove();
                Text.Remove();
            }
        }

        private static void UpdateStates()
        {
            if (Controller.DPad.Count == 1) // Change mode command
            {
                if (Controller.DPad.Up)
                {
                    CurrentMode = Orbwalking.OrbwalkingMode.Combo;
                }
                else if (Controller.DPad.Left)
                {
                    CurrentMode = Orbwalking.OrbwalkingMode.LaneClear;
                }
                else if (Controller.DPad.Right)
                {
                    CurrentMode = Orbwalking.OrbwalkingMode.Mixed;
                }
                else if (Controller.DPad.Down)
                {
                    CurrentMode = Orbwalking.OrbwalkingMode.LastHit;
                }
            }

            //Push any button to cancel mode
            if (Controller.A || Controller.B || Controller.X || Controller.Y || Controller.LeftShoulder ||
                Controller.RightShoulder || Controller.Back || Controller.Start || Controller.RightStick.Clicked)
            {
                CurrentMode = Orbwalking.OrbwalkingMode.None;
            }

            var S1 = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Summoner1);
            var S2 = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Summoner2);

            if (Controller.LeftTrigger > 0 && S1.State == SpellState.Ready)
            {
                SummonerCastLogic(S1);
            }

            if (Controller.RightTrigger > 0 && S2.State == SpellState.Ready)
            {
                SummonerCastLogic(S2);
            }

            Text.text = "MODE: " + CurrentMode;
            OrbWalker.ActiveMode = CurrentMode;
        }

        private static void SummonerCastLogic(SpellDataInst spell)
        {
            switch (spell.Name.ToLower().Replace("summoner", ""))
            {
                case "barrier":
                    ObjectManager.Player.Spellbook.CastSpell(spell.Slot);
                    break;
                case "boost":
                    ObjectManager.Player.Spellbook.CastSpell(spell.Slot);
                    break;
                case "dot":
                    foreach (
                        var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValidTarget(550) && h.Health < 600)
                        )
                    {
                        ObjectManager.Player.Spellbook.CastSpell(spell.Slot, enemy);
                        break;
                    }
                    break;
                case "flash": //LOL
                    Controller.Update();
                    var pos = ObjectManager.Player.ServerPosition.To2D() + (Controller.LeftStick.Position / 75);
                    pos.Extend(ObjectManager.Player.ServerPosition.To2D(), 550);
                    ObjectManager.Player.Spellbook.CastSpell(spell.Slot, pos.To3D());
                    break;
                case "haste":
                    ObjectManager.Player.Spellbook.CastSpell(spell.Slot);
                    break;
                case "heal":
                    ObjectManager.Player.Spellbook.CastSpell(spell.Slot);
                    break;
                case "mana":
                    ObjectManager.Player.Spellbook.CastSpell(spell.Slot);
                    break;
                case "revive":
                    ObjectManager.Player.Spellbook.CastSpell(spell.Slot);
                    break;
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            var wp = ObjectManager.Player.GetWaypoints();

            //in case you manually click to move
            if (wp.Count > 0 && ObjectManager.Player.Distance(wp[wp.Count - 1]) > 540)
            {
                CurrentMode = Orbwalking.OrbwalkingMode.None;
                OrbWalker.ActiveMode = CurrentMode;
                return;
            }

            if (Controller == null || !Controller.Connected)
            {
                Game.PrintChat("Controller disconnected!");
                Game.OnGameUpdate -= Game_OnGameUpdate;
                return;
            }

            Controller.Update();
            UpdateStates();

            var p = ObjectManager.Player.ServerPosition.To2D() + (Controller.LeftStick.Position / 75);
            var pos = new Vector3(p.X, p.Y, ObjectManager.Player.Position.Z);

            if (ObjectManager.Player.Distance(pos) < 75)
            {
                return;
            }


            CurrentPosition.Position = pos;
            OrbWalker.SetOrbwalkingPoint(pos);
        }
    }
}