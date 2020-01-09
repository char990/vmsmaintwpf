using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;


namespace VMSMaintWpf
{
    static public class BmpPixels
    {
        static public Color[,] LoadFile(string file, int pixelWidth, int pixelHeight)
        {
            Color[,] bmpPixels = new Color[pixelWidth, pixelHeight];
            System.Drawing.Color color;
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(file);
            for (int x = 0; x < pixelWidth; x++)
            {
                for (int y = 0; y < pixelHeight; y++)
                {
                    if (x < bmp.Width && y < bmp.Height)
                    {
                        color = bmp.GetPixel(x, y);
                        bmpPixels[x, y] = Color.FromRgb(color.R, color.G, color.B);
                    }
                    else
                    {
                        bmpPixels[x, y] = Colors.Black;
                    }
                }
            }
            return bmpPixels;
        }
    }
}
