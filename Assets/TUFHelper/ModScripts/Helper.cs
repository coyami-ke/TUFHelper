using System;

namespace TUFHelper
{
    public static class Helper
    {
        public static double pguDiffToSortNumber(string pgu)
        {
            if (pgu.Equals("-22"))
            {
                return 1000;
            }
            else if (pgu.Equals("727"))
            {
                return 727;
            }
            else if (pgu.Equals("64"))
            {
                return 64;
            }
            else if (pgu.Equals("0.9"))
            {
                return 50;
            }
            else if (pgu.Equals("-21"))
            {
                return 1100;
            }
            else if (pgu.StartsWith("P"))
            {
                return double.Parse(pgu.Substring(1));
            }
            else if (pgu.StartsWith("G"))
            {
                return double.Parse(pgu.Substring(1)) + 20;
            }
            else if (pgu.StartsWith("U"))
            {
                return double.Parse(pgu.Substring(1)) + 40;
            }
            return 800;
        }
    }


}