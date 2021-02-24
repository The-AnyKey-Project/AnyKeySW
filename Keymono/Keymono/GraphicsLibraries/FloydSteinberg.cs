using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Dither
{
    class Dither
    {
        private static int[,] ditherArray;

        public static Bitmap FloydSteinberg(Bitmap orgImage)
        {
            int height = orgImage.Height;
            int width = orgImage.Width;
            ditherArray = new int[height, width];
            int column = 0, line = 0, temp = 0;
            Bitmap neu = new Bitmap(width, height);
            int r = 0, g = 0, b = 0, gray = 0;

            for (column = 0; column < width; column++)
            {
                for (line = 0; line < height; line++)
                {
                    Color pixel = orgImage.GetPixel(column, line);
                    r = pixel.R;
                    g = pixel.G;
                    b = pixel.B;
                    gray = (r + g + b) / 3;
                    ditherArray[line, column] = gray;
                }
            }

            for (line = 1; line < height - 1; line++)
            {
                for (column = 1; column < width - 1; column++)
                {
                    FloydSteinbergDither(line, column);
                }
            }

            for (column = 0; column < width; column++)
            {
                for (line = 0; line < height; line++)
                {
                    Color pixel = orgImage.GetPixel(column, line);
                    temp = ditherArray[line, column];
                    if (temp == 0)
                        temp = 0;
                    else
                        temp = 255;
                    pixel = Color.FromArgb(temp, temp, temp);
                    neu.SetPixel(column, line, pixel);
                }
            }
            return neu;
        }

        private static void FloydSteinbergDither(int column, int line)
        {
            int t = 0;
            if (ditherArray[column, line] < 128)
            {
                t = ditherArray[column, line] / 16;
                ditherArray[column, line] = 0;
            }
            else
            {
                t = (ditherArray[column, line] - 255) / 16;
                ditherArray[column, line] = 1;
            }
            ditherArray[column + 1, line - 1] += (t * 3);
            ditherArray[column + 1, line] += (t * 5);
            ditherArray[column + 1, line + 1] += t;
            ditherArray[column, line + 1] += (t * 7);
        }

    }
}
