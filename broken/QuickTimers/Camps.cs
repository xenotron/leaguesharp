#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

#endregion

namespace QuickTimers
{
    public class Camp
    {
        public enum CampState
        {
            Unknown,
            Dead,
            Alive
        };


        /*
            private static Dictionary<int, int> BuffHashArray;
            BuffHashArray.Add(1, 0x15FB410C);
            BuffHashArray.Add(4, 0x340EC40A);
            BuffHashArray.Add(6, 0x5E7EB806);
            BuffHashArray.Add(7, 0x352D5006);
            BuffHashArray.Add(10, 0x54116508);
            BuffHashArray.Add(12, 0x5E896800);
        */

        private static Vector2 _scale = new Vector2(0, 0);
        private static Vector2 _position = new Vector2(0, 0);

        private static readonly Vector2 MajorCampTextOffset = new Vector2(10, 16);
        private static readonly Vector2 MinorCampTextOffset = new Vector2(1, 12);
        private static readonly int[] MajorCampSpriteDimensions = { 85, 61 };
        private static readonly int[] MinorCampSpriteDimensions = { 64, 46 };
        private readonly Obj_AI_Minion CampObject;

        private readonly bool _isMajor;
        private readonly int _respawnDuration;

        public int BuffHash;
        private int LastPrint;
        public string Name;
        public Render.Text RenderText;
        public Render.Sprite Sprite;
        public CampState State = CampState.Unknown;

        public Camp(int campId, string name, Vector2 position, int respawnTime)
        {
            _respawnDuration = respawnTime * 60 * 1000;
            _isMajor = IsMajorCamp(name);
            Name = name;
            Sprite = GetMapSprite(Name, position);
            RenderText = GetRenderText(position, _isMajor);
            RespawnTime = 0;

            foreach (Obj_AI_Minion obj in ObjectManager.Get<Obj_AI_Minion>().Where(obj => obj.CampNumber == campId))
            {
                CampObject = obj;
            }

            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        public int RespawnTime { get; set; }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (State != CampState.Dead)
            {
                return;
            }

            RenderText.text = Utils.FormatTime(Math.Abs(Environment.TickCount - RespawnTime) / 1000f);
            PrintFloat();
        }

        public void Kill(float respawn)
        {
            if (respawn == 0.0f) //Camp found dead.
            {
                Sprite.GrayScale();
                Sprite.Show();
            }
            else
            {
                Sprite.Hide();
            }

            State = respawn == 0.0f ? CampState.Unknown : CampState.Dead;
            RespawnTime = Environment.TickCount + _respawnDuration + 1000 + Game.Ping;
        }

        public void Spawn()
        {
            Sprite.Reset();
            Sprite.Show();

            RenderText.text = "";

            State = CampState.Alive;
            RespawnTime = 0;
        }

        public void Draw()
        {
            Sprite.Show();
            Sprite.Add(1);
        }

        public static Dictionary<int, Camp> GetCamps(Vector2 pos, Vector2 scale)
        {
            _scale = scale;
            return new Dictionary<int, Camp>
            {
                { 1, new Camp(1, "Blue", GetScaledVector(-250), 5) },
                { 4, new Camp(4, "Red", GetScaledVector(-175), 5) },
                { 6, new Camp(6, "Dragon", GetScaledVector(5), 6) },
                { 7, new Camp(7, "Blue", GetScaledVector(187), 5) },
                { 10, new Camp(10, "Red", GetScaledVector(112), 5) },
                { 12, new Camp(12, "Baron", GetScaledVector(-89), 7) }
            };
        }

        private void PrintFloat()
        {
            if (Environment.TickCount - LastPrint <= 1000 || Render.OnScreen(Drawing.WorldToScreen(CampObject.Position)))
            {
                return;
            }
            CampObject.PrintFloatText(RenderText.text, Packet.FloatTextPacket.Invulnerable);
            LastPrint = Environment.TickCount;
        }

        private static Vector2 GetScaledVector(int x)
        {
            return new Vector2(Drawing.Width / 2f + (_scale.X * +x), 25);
        }

        private static Render.Text GetRenderText(Vector2 pos, bool isMajor)
        {
            Vector2 offset = isMajor ? MajorCampTextOffset : MinorCampTextOffset;
            int size = isMajor ? 48 : 42;
            var text = new Render.Text("", (int) (pos.X + offset.X), (int) (pos.Y + offset.Y), size, Color.White);
            text.Add(1);
            return text;
        }

        private static Stream GetBitmapStream(string name)
        {
            string str = "QuickTimers.Resources." + name + ".png";
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(str);
        }

        private static Render.Sprite GetMapSprite(string name, Vector2 pos)
        {
            Stream stream = GetBitmapStream(name);
            var sprite = new Render.Sprite(stream, pos) { Scale = _scale };
            sprite.OnReset += Sprite_OnReset;
            Crop(sprite, IsMajorCamp(name));
            return sprite;
        }


        private static void Sprite_OnReset(Render.Sprite sprite)
        {
            int w = sprite.Width;
            sprite.Scale = _scale;
            Crop(sprite, w == 85);
        }

        private static void Crop(Render.Sprite sprite, bool isMajor = false)
        {
            int[] dimensions = isMajor ? MajorCampSpriteDimensions : MinorCampSpriteDimensions;
            sprite.Crop(0, 0, dimensions[0], dimensions[1], true);
        }

        private static bool IsMajorCamp(string name)
        {
            return (name == "Dragon" || name == "Baron");
        }
    }
}