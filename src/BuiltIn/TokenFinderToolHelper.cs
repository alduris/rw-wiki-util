using MoreSlugcats;

namespace WikiUtil.BuiltIn
{
    internal static class TokenFinderToolHelper
    {
        public static bool IsAllowedPearl(DataPearl.AbstractDataPearl.DataPearlType type)
        {
            return type != DataPearl.AbstractDataPearl.DataPearlType.Misc
                && type != DataPearl.AbstractDataPearl.DataPearlType.Misc2
                && type != DataPearl.AbstractDataPearl.DataPearlType.PebblesPearl
                && (!ModManager.MSC || type != MoreSlugcatsEnums.DataPearlType.BroadcastMisc);
        }
    }
}
