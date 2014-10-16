#region

using System;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace SpellHumanizer
{
    internal class Program
    {
        public static Obj_AI_Hero Player;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            Game.OnGameSendPacket += Game_OnGameSendPacket;
        }

        private static void Game_OnGameSendPacket(GamePacketEventArgs args)
        {
            if (args.PacketData[0] != Packet.C2S.Cast.Header)
            {
                return;
            }

            var slot = (SpellSlot) args.PacketData[6];
            var state = IsSummonerSpell(args.PacketData[5])
                ? Player.SummonerSpellbook.CanUseSpell(slot)
                : Player.Spellbook.CanUseSpell(slot);

            if (Player.IsDead || state != SpellState.Ready)
            {
                args.Process = false;
            }
        }

        private static bool IsSummonerSpell(byte spellByte)
        {
            return spellByte == 0xE9 || spellByte == 0xEF || spellByte == 0x8B || spellByte == 0xED || spellByte == 0x63;
        }
    }
}