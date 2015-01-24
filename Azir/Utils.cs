#region

using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Azir
{
    internal static class Utils
    {
        public static bool IsGoodCastTarget(this Obj_AI_Hero hero, float range)
        {
            return hero != null && hero.IsValidTarget(range);
        }
    }
}