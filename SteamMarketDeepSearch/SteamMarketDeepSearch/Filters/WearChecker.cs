namespace SteamMarketDeepSearch.Filters
{
    public static class WearChecker
    {
        public static bool Matches(
            string wearText)
        {
            int dotIndex =
                wearText.IndexOf('.');

            if (dotIndex == -1)
            {
                return false;
            }

            string decimals =
                wearText[(dotIndex + 1)..];


            return
                MatchesAtPosition(decimals, 0) ||
                MatchesAtPosition(decimals, 1) ||
                MatchesAtPosition(decimals, 2) ||
                MatchesAtPosition(decimals, 3);
        }


        private static bool MatchesAtPosition(
            string decimals,
            int position)
        {
            if (decimals.Length < position + 3)
            {
                return false;
            }

            return
                decimals.Substring(
                    position,
                    3) == "727";
        }
    }
}