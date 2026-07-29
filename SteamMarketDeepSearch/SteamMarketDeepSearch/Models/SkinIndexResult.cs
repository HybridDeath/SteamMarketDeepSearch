using System.Collections.Generic;
using System.Linq;

namespace SteamMarketDeepSearch.Models
{
    public class SkinIndexResult
    {
        public List<SkinDefinition> Skins { get; set; } = [];

        public int TotalSkins =>
            Skins.Count;

        public SkinDefinition? LowestSupply =>
            Skins
                .Where(x => x.SellOrderCount > 0)
                .OrderBy(x => x.SellOrderCount)
                .FirstOrDefault();


        public SkinDefinition? HighestSupply =>
            Skins
                .Where(x => x.SellOrderCount > 0)
                .MaxBy(x => x.SellOrderCount);
    }
}
