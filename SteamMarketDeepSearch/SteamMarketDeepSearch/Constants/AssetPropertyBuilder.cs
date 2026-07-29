using System;
using System.Collections.Generic;

namespace SteamMarketDeepSearch.Constants
{
    public static class AssetPropertyBuilder
    {
        private static readonly IReadOnlyList<string> PaintSeedFilters =
        [
            "CAEg1wUo1wU=" // paintseed == 727
        ];

        private static readonly IReadOnlyList<string> FloatFilters =
        [
            "CAIVrBw6Px01Xjo/",     // 0.727 > float < 0.728
            "CAIVveOUPR0rGJU9",     // 0.0727 > float < 0.0728
            "CAIVRdgwPh188jA+",     // 0.1727 > float < 0.1728
            "CAIVVp+LPh1xrIs+",     // 0.2727 > float < 0.2728
            "CAIVidK+Ph2k374+",     // 0.3727 > float < 0.3728
            "CAIVvAXyPh3XEvI+",     // 0.4727 > float < 0.4728
            "CAIVeJwSPx0FoxI/",     // 0.5727 > float < 0.5728
            "CAIVETYsPx2fPCw/",     // 0.6727 > float < 0.6728
            "CAIVq89FPx051kU/",     // 0.7727 > float < 0.7728
            "CAIVRGlfPx3Sb18/",     // 0.8727 > float < 0.8728
            "CAIV3gJ5Px1sCXk/"      // 0.9727 > float < 0.9728
        ];

        public static IEnumerable<string> BuildAssetProperties()
        {
            foreach (string paint in PaintSeedFilters)
            {
                foreach (string wear in FloatFilters)
                {
                    yield return
                        $"assetproperty={Uri.EscapeDataString(paint)}&" +
                        $"assetproperty={Uri.EscapeDataString(wear)}";
                }
            }
        }
    }
}