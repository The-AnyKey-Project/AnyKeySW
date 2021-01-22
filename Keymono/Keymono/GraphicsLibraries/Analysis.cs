
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Keymono.GraphicsLibraries
{
    public static class Analysis
    {
        public static List<Color> TenMostUsedColors { get; private set; }
        public static List<int> TenMostUsedColorIncidences { get; private set; }

        public static Color MostUsedColor { get; private set; }
        public static int MostUsedColorIncidence { get; private set; }

        public static Color MostUsedVisibleColor { 
            get
            {
                foreach (Color c in TenMostUsedColors)
                {
                    if (c.Name != "0" && c.A > 128)
                        return c;
                }
                return Color.Transparent;
            } 
        }

        private static int pixelColor;

        private static Dictionary<int, int> dctColorIncidence;

        public static Color GetReadableForeColor(Color c)
        {
            return (((c.R + c.B + c.G) / 3) > 128) ? Color.Black : Color.White;
        }

        

        public static void Analyse(Bitmap theBitMap)
        {
            TenMostUsedColors = new List<Color>();
            TenMostUsedColorIncidences = new List<int>();

            MostUsedColor = Color.Empty;
            MostUsedColorIncidence = 0;

            // does using Dictionary<int,int> here
            // really pay-off compared to using
            // Dictionary<Color, int> ?

            // would using a SortedDictionary be much slower, or ?

            dctColorIncidence = new Dictionary<int, int>();

            // this is what you want to speed up with unmanaged code
            for (int row = 0; row < theBitMap.Size.Width; row++)
            {
                for (int col = 0; col < theBitMap.Size.Height; col++)
                {
                    pixelColor = theBitMap.GetPixel(row, col).ToArgb();

                    if (dctColorIncidence.Keys.Contains(pixelColor))
                    {
                        dctColorIncidence[pixelColor]++;
                    }
                    else
                    {
                        dctColorIncidence.Add(pixelColor, 1);
                    }
                }
            }

            // note that there are those who argue that a
            // .NET Generic Dictionary is never guaranteed
            // to be sorted by methods like this
            var dctSortedByValueHighToLow = dctColorIncidence.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

            // this should be replaced with some elegant Linq ?
            foreach (KeyValuePair<int, int> kvp in dctSortedByValueHighToLow.Take(10))
            {
                TenMostUsedColors.Add(Color.FromArgb(kvp.Key));
                TenMostUsedColorIncidences.Add(kvp.Value);
            }

            MostUsedColor = Color.FromArgb(dctSortedByValueHighToLow.First().Key);
            MostUsedColorIncidence = dctSortedByValueHighToLow.First().Value;
        }


    }
}
