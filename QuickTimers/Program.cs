#region

using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

#endregion

namespace QuickTimers
{
    internal class Program
    {
        private static readonly Vector2 _scale = new Vector2(1.25f, 1.25f);
        private static Vector2 _pos = new Vector2(Drawing.Width / 2f - 286.5f, 15);
        private static Dictionary<int, Camp> Camps;
        private static Render.Sprite HUD;


        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_Load;
        }

        private static void Game_Load(EventArgs args)
        {
            HUD = LoadHUD();

            Camps = Camp.GetCamps(_pos, _scale);

            foreach (var camp in Camps)
            {
                camp.Value.Draw();
                camp.Value.Kill(0);
            }

            Game.OnGameProcessPacket += Game_OnGameProcessPacket;
        }

        private static void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            var packet = new GamePacket(args.PacketData);
            Camp camp;

            switch (packet.Header)
            {
                case 0xC3:
                    packet.Position = 5;
                    var UnitNetworkId = packet.ReadInteger();
                    var CampId = packet.ReadInteger();
                    var EmptyType = packet.ReadByte();
                    var BuffHash = packet.ReadInteger();
                    var respawnTime = packet.ReadFloat();

                    camp = Camps[CampId];
                    if (camp != null)
                        camp.Kill(respawnTime);

                    break;

                case 0xE9:
                    packet.Position = 21;
                    var campId = packet.ReadByte();

                    camp = Camps[campId];
                    if (camp != null)
                        camp.Spawn();

                    break;
            }
        }

        private static Render.Sprite LoadHUD()
        {
            _pos = GetScaledVector(_pos);

            var loadHud = new Render.Sprite(Properties.Resources.HUD, _pos)
            {
                Scale = _scale,
                Color = new ColorBGRA(255f, 255f, 255f, 20f)
            };
            loadHud.Position = GetPosition(loadHud.Width);
            loadHud.Show();
            loadHud.Add(0);

            return loadHud;
        }

        private static Vector2 GetPosition(int width)
        {
            return new Vector2(Drawing.Width / 2f - width / 2f, 15);
        }

        private static Vector2 GetScaledVector(Vector2 vector)
        {
            return Vector2.Modulate(_scale, vector);
        }
    }
}