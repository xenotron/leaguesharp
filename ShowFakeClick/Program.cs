#region

using System;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

#endregion

namespace ShowFakeClick
{
    internal class Program
    {
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
            ShowFakeClick(Game.CursorPos);
        }

        private static void ShowFakeClick(Vector3 position)
        {
            var tile = NavMesh.WorldToGrid(position.X, position.Y);
            var z = NavMesh.GetHeightForPosition(position.X, position.Y);

            var p = new GamePacket(0x87);
            p.WriteHexString(
                "00 00 00 00 02 69 DC 57 4D C9 4F 15 0A 20 00 00 00 00 00 00 00 00 00 01 00 00 00 00 B2 01 00 40");
            p.WriteInteger(ObjectManager.Player.NetworkId);
            p.WriteByte(0, 8);
            p.WriteShort((short) tile.X);
            p.WriteFloat(z);
            p.WriteShort((short) tile.Y);
            p.WriteHexString("6F F2 00 00 00 00 18 F3");
            p.WriteShort((short) tile.X);
            p.WriteFloat(z);
            p.WriteShort((short) tile.Y);
            p.WriteHexString(
                "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 80 3F 69 DC 57 4D 79 0D DA 08 20 00 00 00 00 00 00 00 00 00 01 00 00 00 00 B1 01 00 40");
            p.WriteInteger(ObjectManager.Player.NetworkId);
            p.WriteByte(0, 8);
            p.WriteShort((short) tile.X);
            p.WriteFloat(z);
            p.WriteShort((short) tile.Y);
            p.WriteHexString("6F F2 00 00 00 00 18 F3");
            p.WriteShort((short) tile.X);
            p.WriteFloat(z);
            p.WriteShort((short) tile.Y);
            p.WriteHexString("00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 80 3F 00");
            p.Process();
        }
    }
}