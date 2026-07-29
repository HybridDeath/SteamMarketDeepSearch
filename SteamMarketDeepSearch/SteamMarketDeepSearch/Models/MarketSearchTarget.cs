using SteamMarketDeepSearch.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamMarketDeepSearch.Models
{
    public class MarketSearchTarget
    {
        public string DisplayName { get; set; } = string.Empty;
        public ItemCategory Category { get; set; }
        public WeaponType? WeaponType { get; set; }
        public string MarketQuery { get; set; } = string.Empty;
    }
}
