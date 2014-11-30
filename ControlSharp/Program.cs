using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.XInput;

namespace ControlSharp
{
    internal class Program
    {
        public static Controller Control;
        public static int[] ControllerArray = { 0, 1, 2, 3, 4 };
        public static Menu Menu;

        public static Orbwalking.Orbwalker OrbWalker;

        private static void Main(string[] args)
        {
            Console.Clear();
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
            Menu.AddToMainMenu();

            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Control == null || !Control.IsConnected)
            {
                Game.PrintChat("Controller disconnected!");
                Game.OnGameUpdate -= Game_OnGameUpdate;
                return;
            }

            var state = Control.GetState().Gamepad;
            var currentMode = Orbwalking.OrbwalkingMode.None;

            if (state.LeftTrigger > 0)
            {
                currentMode = Orbwalking.OrbwalkingMode.Mixed;
            }
            if (state.RightTrigger > 0)
            {
                currentMode = Orbwalking.OrbwalkingMode.Combo;
            }

            OrbWalker.ActiveMode = currentMode;
            var pos = ObjectManager.Player.ServerPosition;
            pos.X += state.LeftThumbX / 100f;
            pos.Y += state.LeftThumbY / 100f;
            
            if (ObjectManager.Player.Distance(pos) < 50)
            {
                return;
            }
            
            OrbWalker.SetOrbwalkingPoint(pos);
        }
    }
}