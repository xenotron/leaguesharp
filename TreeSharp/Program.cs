using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using LeagueSharp;
using LeagueSharp.Common;
using TreeSharp.Properties;

namespace TreeSharp
{
    internal class Program
    {
        public static List<String> WardSpells = new List<string>
        {
            "SightWard",
            "VisionWard",
            "TrinketTotemLvl1",
            "ItemGhostWard"
        };

        public static List<Obj_AI_Base> WardList = new List<Obj_AI_Base>();
        public static Render.Text WardText;

        public static SoundObject OnWardSound;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            WardText = new Render.Text("Ward Count: 0", 200, 200, 24, SharpDX.Color.Red);
            WardText.Add();

            OnWardSound = new SoundObject(Resources.OnWard);
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Game.OnGameUpdate += Game_OnGameUpdate;
            //Game.OnGameNotifyEvent += Game_OnGameNotifyEvent;
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            var wardCount = WardList.Count(w => w.IsValid && !w.IsDead && w.Health > 0 && w.IsVisible);
            WardText.text = "Ward Count: " + wardCount;
        }

        private static void Game_OnGameNotifyEvent(GameNotifyEventArgs args)
        {
            if (args.NetworkId != ObjectManager.Player.NetworkId)
            {
                return;
            }

            if (args.EventId == GameEventId.OnChampionPentaKill)
            {
                return;
            }

            if (args.EventId == GameEventId.OnDie)
            {
                return;
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender == null || !sender.IsValid || !sender.IsMe)
            {
                return;
            }

            if (WardSpells.Contains(args.SData.Name))
            {
                OnWardSound.Play();
            }

            if (args.SData.Name.Equals("TrinketTotemLvl1"))
            {
                WardList.Add(sender);
            }
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