#region

using System;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Humanizer
{
    public class Program
    {
        public static Menu Config;
        public static float LastMove;
        public static Obj_AI_Base Player = ObjectManager.Player;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Config = new Menu("Humanizer", "Humanizer", true);
            Config.AddItem(new MenuItem("MovementDelay", "Movement Delay")).SetValue(new Slider(0, 200, 0));
            Config.AddItem(new MenuItem("Enabled", "Spell Humanizer Enabled").SetValue(true));
            Config.Item("Enabled").ValueChanged += Program_ValueChanged;
            Config.AddToMainMenu();

            Obj_AI_Base.OnIssueOrder += Obj_AI_Base_OnIssueOrder;
        }

        private static void Obj_AI_Base_OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            if (sender == null || !sender.IsValid || !sender.IsMe || args.Order != GameObjectOrder.MoveTo)
            {
                return;
            }

            if (Environment.TickCount - LastMove < GetDelay())
            {
                args.Process = false;
                return;
            }

            LastMove = Environment.TickCount;
        }

        private static void Program_ValueChanged(object sender, OnValueChangeEventArgs e)
        {
            SpellHumanizer.Enabled = e.GetNewValue<bool>();
        }

        private static float GetDelay()
        {
            return Config.Item("MovementDelay").GetValue<Slider>().Value;
        }
    }
}