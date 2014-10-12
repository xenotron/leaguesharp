#region

using System;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace SpellHumanizer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Game.OnGameSendPacket += Game_OnGameSendPacket;
        }

        private static void Game_OnGameSendPacket(GamePacketEventArgs args)
        {
            if (args.PacketData[0] != Packet.C2S.Cast.Header)
            {
                return;
            }

            var spellState = ObjectManager.Player.Spellbook.CanUseSpell((SpellSlot) args.PacketData[6]);

            if (ObjectManager.Player.IsDead || spellState == SpellState.Cooldown || spellState == SpellState.NoMana ||
                spellState == SpellState.NotLearned || spellState == SpellState.Surpressed ||
                spellState == SpellState.Unknown)
            {
                args.Process = false;
            }
        }
    }
}