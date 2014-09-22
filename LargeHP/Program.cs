#region

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

#endregion

namespace LargeHP
{
    internal class Program
    {
        private static readonly Dictionary<string, Render.Text> TextDictionary = new Dictionary<string, Render.Text>();
        private static readonly string[] Names = { "Baron", "Dragon" };

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            foreach (var name in Names)
            {
                var text = TextDictionary[name];
                var unit = FindUnit(name);

                if (unit == null)
                {
                    continue;
                }

                if (text == null || text.Unit == null)
                {
                    Utility.DebugMessage("FOUND: " + name);
                    var rText = new Render.Text(
                        unit.Health.ToString(), unit, new Vector2(5, -10), 45, SharpDX.Color.Blue, "Helvetica");
                    rText.Add();
                    TextDictionary.Add(name, rText);
                    continue;
                }

                if (text.Unit.IsDead)
                {
                    Utility.DebugMessage("REMOVED: " + name);
                    text.Dispose();
                    TextDictionary.Remove(name);
                    continue;
                }

                text.text = unit.Health.ToString();
            }
        }


        private static Obj_AI_Base FindUnit(string name)
        {
            return
                ObjectManager.Get<Obj_AI_Base>()
                    .First(unit => unit.IsValid && unit.IsVisible && !unit.IsDead && (unit.Name.ToLower().Contains(name.ToLower())));
        }
    }
}