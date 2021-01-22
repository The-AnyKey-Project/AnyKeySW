using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keymono.GraphicsLibraries
{
    class Effects
    {

        static float[][] gray_matrix = new float[][] {
                    new float[] { 0.299f, 0.299f, 0.299f, 0, 0 },
                    new float[] { 0.587f, 0.587f, 0.587f, 0, 0 },
                    new float[] { 0.114f, 0.114f, 0.114f, 0, 0 },
                    new float[] { 0,      0,      0,      1, 0 },
                    new float[] { 0,      0,      0,      0, 1 }
                };

        static float[][] bw_matrix = new float[][] {
                    new float[] {1.5f, 1.5f, 1.5f, 0, 0},
                    new float[] {1.5f, 1.5f, 1.5f, 0, 0},
                    new float[] {1.5f, 1.5f, 1.5f, 0, 0},
                    new float[] {0f, 0f, 0f, 1f, 0f},
                    new float[] {-1f, -1f, -1f, 0f, 1f}
                };

        public static Image ConvertToGrayScale(Image SourceImage, float Threshold)
        {
            return ConvertColorMatrix(SourceImage, gray_matrix, Threshold);
        }

        public static Image ConvertToBlackAndWhite(Image SourceImage, float Threshold)
        {
                return ConvertColorMatrix(ReplaceTransparency(SourceImage, Color.Black), bw_matrix, Threshold);
            
        }

        private static Image ConvertColorMatrix(Image SourceImage, float[][] ColorMatrix, float Threshold)
        {
            Bitmap work = new Bitmap(SourceImage);

            using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(work))
            {
                var ia = new System.Drawing.Imaging.ImageAttributes();
                ia.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix(bw_matrix));
                ia.SetThreshold(Threshold); // Change this threshold as needed
                var rc = new Rectangle(0, 0, SourceImage.Width, SourceImage.Height);
                gr.DrawImage(SourceImage, rc, 0, 0, SourceImage.Width, SourceImage.Height, GraphicsUnit.Pixel, ia);
            }
            return work;
        }

        public static System.Drawing.Bitmap ReplaceTransparency(System.Drawing.Image image, System.Drawing.Color background)
        {
            return ReplaceTransparency((System.Drawing.Bitmap)image, background);
        }

        public static System.Drawing.Bitmap ReplaceTransparency(System.Drawing.Bitmap bitmap, System.Drawing.Color background)
        {
            /* Important: you have to set the PixelFormat to remove the alpha channel.
             * Otherwise you'll still have a transparent image - just without transparent areas */
            var result = new System.Drawing.Bitmap(bitmap.Size.Width, bitmap.Size.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            var g = System.Drawing.Graphics.FromImage(result);

            g.Clear(background);
            g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
            g.DrawImage(bitmap, 0, 0);

            return result;
        }
        public static Bitmap ResizeImage(Bitmap bmp, int newWidth, int newHeight)
        {
            Bitmap newImage = new Bitmap(newWidth, newHeight);
            using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(newImage))
            {
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                gr.DrawImage(bmp, new Rectangle(0, 0, newWidth, newHeight));
            }
            return newImage;
        }
    }
}
