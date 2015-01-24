#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using TreeSharp.Properties;

#endregion

namespace TreeSharp
{
    internal class Program
    {
        public static List<Obj_AI_Base> WardList = new List<Obj_AI_Base>();
        public static Render.Text WardText;
        public static SoundObject OnWardSound;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            WardText = new Render.Text(
                "Ward Count: 0/3", Drawing.Width / 2 + 500, Drawing.Height - 50, 22, Color.Yellow);
            WardText.Add();

            OnWardSound = new SoundObject(Resources.OnWard);
            Game.OnGameUpdate += Game_OnGameUpdate;
            GameObject.OnCreate += GameObject_OnCreate;
            //Game.OnGameNotifyEvent += Game_OnGameNotifyEvent;
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            var unit = sender as Obj_AI_Minion;

            if (unit == null || !unit.IsValid)
            {
                return;
            }

            if (unit.Name == "VisionWard" && unit.BaseSkinName == "sightward")
            {
                WardList.Add(unit);
            }

            if (unit.Name.ToLower().Contains("ward") && sender.IsAlly &&
                ObjectManager.Player.Distance(sender.Position) < 500)
            {
                OnWardSound.Play();
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            var wardCount = WardList.Count(w => w.IsValid && !w.IsDead && w.Health > 0 && w.IsVisible);
            WardText.text = "Ward Count: " + wardCount + "/3";
            WardText.Color = wardCount == 3 ? Color.Red : Color.Yellow;
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