using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.XInput;
using Color = System.Drawing.Color;

namespace ControlSharp
{
    internal class Program
    {
        public static Controller Control;
        public static int[] ControllerArray = { 0, 1, 2, 3, 4 };
        public static Menu Menu;

        public static Orbwalking.Orbwalker OrbWalker;
        public static Orbwalking.OrbwalkingMode CurrentMode = Orbwalking.OrbwalkingMode.None;
        public static Gamepad CurrentPad;
        public static Render.Circle CurrentPosition;

        private static void Main(string[] args)
        {
            //Console.Clear();
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            foreach (var c in
                ControllerArray.Select(controlId => new Controller((UserIndex) controlId)).Where(c => c.IsConnected))
            {
                Control = c;
            }

            if (Control == null || !Control.IsConnected)
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
            }

            Menu.Item("Draw").ValueChanged += OnValueChanged;
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        private static void OnValueChanged(object sender, OnValueChangeEventArgs onValueChangeEventArgs)
        {
            if (onValueChangeEventArgs.GetNewValue<bool>())
            {
                CurrentPosition = new Render.Circle(ObjectManager.Player.Position, 100, Color.Red, 2);
                CurrentPosition.Add();
            }
            else
            {
                CurrentPosition.Remove();
            }
        }

        private static void UpdateStates()
        {
            CurrentPad = Control.GetState().Gamepad;
            var b = CurrentPad.Buttons.ToString();

            if (b == "None")
            {
                return;
            }

            if (b.Contains("DPadUp"))
            {
                CurrentMode = Orbwalking.OrbwalkingMode.Combo;
            }
            else if (b.Contains("DPadLeft"))
            {
                CurrentMode = Orbwalking.OrbwalkingMode.LaneClear;
            }
            else if (b.Contains("DPadRight"))
            {
                CurrentMode = Orbwalking.OrbwalkingMode.Mixed;
            }
            else if (b.Contains("DPadDown"))
            {
                CurrentMode = Orbwalking.OrbwalkingMode.LastHit;
            }
            else if (!b.Contains("LeftThumb")) //Push any button to cancel mode
            {
                CurrentMode = Orbwalking.OrbwalkingMode.None;
            }

            OrbWalker.ActiveMode = CurrentMode;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Control == null || !Control.IsConnected)
            {
                Game.PrintChat("Controller disconnected!");
                Game.OnGameUpdate -= Game_OnGameUpdate;
                return;
            }

            UpdateStates();
            var pos = ObjectManager.Player.ServerPosition;
            pos.X += CurrentPad.LeftThumbX / 100f;
            pos.Y += CurrentPad.LeftThumbY / 100f;

            if (ObjectManager.Player.Distance(pos) < 75)
            {
                return;
            }

            CurrentPosition.Position = pos;
            OrbWalker.SetOrbwalkingPoint(pos);
        }
    }
}