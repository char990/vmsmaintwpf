using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace VMSMaintWpf
{
    public class VirtualSign
    {
        Canvas canvas;
        int squareSize;
        int X, Y;
        Color OFF;
        public VirtualSign(Canvas canvas, int columns, int rows)
        {
            this.canvas = canvas;
            squareSize = 512 / columns;
            Y = rows;
            X = columns;
            canvas.Width = squareSize * columns;
            canvas.Height = squareSize * rows;
            canvas.Background = Brushes.Black;
            OFF = Color.FromRgb(0x40,0x40,0x40);
        }


        public void Display(Color[,] pxl)
        {
            if (pxl.Length != Y * X)
            {
                throw new Exception("Display data length error");
            }
            canvas.IsEnabled = false;
            canvas.Children.Clear();
            for (int y = 0; y < Y; y++)
            {
                for (int x = 0; x < X; x++)
                {
                    Ellipse led = new Ellipse
                    {
                        Width = squareSize,
                        Height = squareSize,
                    };
                    if (pxl[x, y] == Colors.Black)
                    {
                        led.Fill = new SolidColorBrush(OFF);
                    }
                    else
                    {
                        led.Fill = new SolidColorBrush(pxl[x, y]);
                    }
                    Canvas.SetTop(led, y * squareSize);
                    Canvas.SetLeft(led, x * squareSize);
                    canvas.Children.Add(led);
                }
            }
            canvas.IsEnabled = true;
        }

        
        public void Display(int[,] pxl)
        {
            if (pxl.Length != Y * X)
            {
                throw new Exception("Display data length error");
            }
            Color[,] cpxl = new Color[X, Y];
            for (int y = 0; y < Y; y++)
            {
                for (int x = 0; x < X; x++)
                {
                    cpxl[x, y] = Color.FromRgb((byte)(pxl[x, y] >> 16), (byte)(pxl[x, y] >> 8), (byte)(pxl[x, y]));
                }
            }
            Display(cpxl);
        }

        public void AllOff()
        {
            Color[,] off = new Color[X, Y];
            Array.Clear(off, 0, off.Length);
            Display(off);
        }

    }
}
