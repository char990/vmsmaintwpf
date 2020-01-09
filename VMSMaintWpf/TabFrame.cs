using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TsiSp003.ApplicationLayerData;
using TsiSp003.Master;
using static TsiSp003.ConstCode;
using System.Windows.Media;
using static TsiSp003.RtaFont;
using TsiSp003;
using System.Windows.Input;

namespace VMSMaintWpf
{
    public partial class MainWindow : Window
    {
        int textRows;
        int pixelWidth;
        int pixelHeight;

        TextBox[] textLines;
        Color[,] bmpPixels;
        Color[,] treatedPixels;
        Canvas canvas = new Canvas() { HorizontalAlignment = HorizontalAlignment.Left };

        private void cbFrmFrameType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int items;
            switch (cbFrmFrameType.SelectedIndex)
            {
                case 0:
                    btnFrmLoadImg.Visibility = Visibility.Hidden;
                    lblFrmFont.Visibility = Visibility.Visible;
                    cbFrmFont.Visibility = Visibility.Visible;
                    items = 10;
                    break;
                case 1:
                    btnFrmLoadImg.Visibility = Visibility.Visible;
                    lblFrmFont.Visibility = Visibility.Hidden;
                    cbFrmFont.Visibility = Visibility.Hidden;
                    items = 11;
                    break;
                case 2:
                    btnFrmLoadImg.Visibility = Visibility.Visible;
                    lblFrmFont.Visibility = Visibility.Hidden;
                    cbFrmFont.Visibility = Visibility.Hidden;
                    items = 12;
                    break;
                default:
                    return;
            }
            string[] c = new string[items];
            Array.Copy(COLOURS, c, items);
            cbFrmColour.ItemsSource = c;
            cbFrmColour.SelectedIndex = -1;
            gridFrmFrameContent.Children.Clear();
        }

        private void btnFrmGetFromCtrller_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte frmId = 1;
                if (!byte.TryParse(tbFrmFrameId.Text, out frmId) || frmId == 0)
                {
                    throw new Exception("Illegal Frame ID");
                }
                RemoteControllerLink ctrl = remoteConctrollerLinks[parameters.ControllerID];
                ControllerReply rpl;
                rpl = ctrl.SignRequestStoredFrm(frmId);
                if (rpl.status != ControllerReply.Status.SUCCESS)
                {
                    throw new Exception(rpl.status.ToString());
                }
                if ((ctrl.Frames[frmId] is SignSetTextFrame))
                {
                    LoadFrame((ctrl.Frames[frmId] as SignSetTextFrame));
                    return;
                }
                else if ((ctrl.Frames[frmId] is SignSetGraphicsFrame))
                {
                    LoadFrame((ctrl.Frames[frmId] as SignSetGraphicsFrame));
                }
                else if ((ctrl.Frames[frmId] is SignSetHighResolutionGraphicsFrame))
                {
                    LoadFrame((ctrl.Frames[frmId] as SignSetHighResolutionGraphicsFrame));
                    return;
                }
                else
                {
                    throw new Exception("Illegal frame type from controller");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void LoadFrame(SignSetTextFrame f)
        {
            gridFrmFrameContent.Children.Clear();
            //            gridFrmFrameContent.Children.Add(canvas);

            cbFrmFrameType.SelectedIndex = 0;
            tbFrmFrameRev.Text = f.frameRev.ToString();
            cbFrmFont.SelectedIndex = (int)f.font;
            cbFrmColour.SelectedValue = f.colour.ToString();
            cbFrmConspicuity.SelectedValue = ((ConspicuityDevices)f.conspicuity).ToString();
            switch (f.font)
            {
                case TsiSp003.RtaFont.ITS_FONT_SIZE.FONT_DEFAULT:
                    break;
                default:
                    break;
            }
        }

        private void LoadFrame(SignSetGraphicsFrame f)
        {
            cbFrmFrameType.SelectedIndex = 1;
            tbFrmFrameRev.Text = f.frameRev.ToString();
            cbFrmColour.SelectedValue = f.colour.ToString();
            cbFrmConspicuity.SelectedValue = ((ConspicuityDevices)f.conspicuity).ToString();
            LoadGfx(f.pixels);
        }

        private void LoadFrame(SignSetHighResolutionGraphicsFrame f)
        {
            cbFrmFrameType.SelectedIndex = 2;
            tbFrmFrameRev.Text = f.frameRev.ToString();
            cbFrmColour.SelectedValue = f.colour.ToString();
            cbFrmConspicuity.SelectedValue = ((ConspicuityDevices)f.conspicuity).ToString();
            LoadGfx(f.pixels);
        }

        private void LoadGfx(int[,] gfxFrame)
        {
            for (int x = 0; x < pixelWidth; x++)
            {
                for (int y = 0; y < pixelHeight; y++)
                {
                    bmpPixels[x, y] = new System.Windows.Media.Color
                    {
                        A = 255,
                        R = (byte)(gfxFrame[x, y] >> 16),
                        G = (byte)(gfxFrame[x, y] >> 8),
                        B = (byte)(gfxFrame[x, y])
                    };
                }
            }
            gridFrmFrameContent.Children.Clear();
            gridFrmFrameContent.Children.Add(canvas);
            VirtualSign virtualSign = new VirtualSign(canvas, pixelWidth, pixelHeight);
            virtualSign.Display(bmpPixels);
        }

        private void btnFrmSetToCtrller_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte frmId = 1;
                if (!byte.TryParse(tbFrmFrameId.Text, out frmId) || frmId == 0)
                {
                    throw new Exception("Illegal Frame ID");
                }
                byte frmRev;
                if (!byte.TryParse(tbFrmFrameRev.Text, out frmRev))
                {
                    throw new Exception("Illegal Frame Rev");
                }
                switch (cbFrmFrameType.SelectedIndex)
                {
                    case 0:
                        SetFrameToCtrller(new SignSetTextFrame(), frmId, frmRev);
                        break;
                    case 1:
                        SetFrameToCtrller(new SignSetGraphicsFrame(), frmId, frmRev);
                        break;
                    case 2:
                        SetFrameToCtrller(new SignSetHighResolutionGraphicsFrame(), frmId, frmRev);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            /*
            SignSetGraphicsFrame frame = new SignSetGraphicsFrame();
            frame.colour = (GfxColour)Enum.Parse(typeof(GfxColour), cbGfxFrmColour.SelectedValue.ToString());
            if (frame.colour == GfxColour.MultiColours)
            {
                MessageBox.Show("MultiColours is not allowed");
                return;
            }
            frame.cd = (ConspicuityDevices)Enum.Parse(typeof(ConspicuityDevices), cbGfxFrmConspicuity.SelectedValue.ToString());
            frame.frameId = frmId;
            frame.frameRev = frmRev;
            frame.columns = (byte)gfxColumns;
            frame.rows = (byte)gfxRows;
            frame.pixels = new int[gfxColumns, gfxRows];
            for (int x = 0; x < gfxColumns; x++)
            {
                for (int y = 0; y < gfxRows; y++)
                {
                    frame.pixels[x, y] = colorPixels[x, y].R * 0x10000 + colorPixels[x, y].G * 0x100 + colorPixels[x, y].B;
                }
            }
            // todo : font width
            RemoteControllerLink ctrl = remoteConctrollerLinks[parameters.ControllerID];
            ControllerReply rpl;
            lock (ctrl)
            {
                rpl = ctrl.SignSetGraphicsFrame(frame);
            }
            if (rpl.status != ControllerReply.Status.SUCCESS)
            {
                throw new Exception(rpl.status.ToString());
            }*/
        }

        private void SetFrameToCtrller(SignSetTextFrame f, byte frmId, byte frmRev)
        {
            f.frameId = frmId;
            f.frameRev = frmRev;
            if (cbFrmColour.SelectedIndex < 0 || cbFrmColour.SelectedIndex > 9)
            {
                throw new Exception("Illegal colour");
            }
            f.colour = (FrameColour)Enum.Parse(typeof(FrameColour), cbFrmColour.SelectedValue.ToString());
            f.cd = (ConspicuityDevices)Enum.Parse(typeof(ConspicuityDevices), cbFrmConspicuity.SelectedValue.ToString());
            f.font = (ITS_FONT_SIZE)Enum.Parse(typeof(ITS_FONT_SIZE), cbFrmFont.SelectedValue.ToString());



            // todo : font width
            RemoteControllerLink ctrl = remoteConctrollerLinks[parameters.ControllerID];
            ControllerReply rpl;
            lock (ctrl)
            {
                rpl = ctrl.SignSetTextFrame(f);
            }
            if (rpl.status != ControllerReply.Status.SUCCESS)
            {
                MessageBox.Show(rpl.status.ToString());
            }
        }

        private void SetFrameToCtrller(SignSetGraphicsFrame f, byte frmId, byte frmRev)
        {
            try
            {
                f.frameId = frmId;
                f.frameRev = frmRev;
                if (cbFrmColour.SelectedIndex < 0 || cbFrmColour.SelectedIndex > 10)
                {
                    throw new Exception("Illegal colour");
                }
                if (pixelHeight > 255 || pixelWidth > 255)
                {
                    throw new Exception("Can't fit in Graphics Frame");
                }
                f.colour = (FrameColour)Enum.Parse(typeof(FrameColour), cbFrmColour.SelectedValue.ToString());
                f.cd = (ConspicuityDevices)Enum.Parse(typeof(ConspicuityDevices), cbFrmConspicuity.SelectedValue.ToString());
                f.columns = (byte)pixelWidth;
                f.rows = (byte)pixelHeight;
                f.pixels = new int[pixelWidth, pixelHeight];
                for (int x = 0; x < pixelWidth; x++)
                {
                    for (int y = 0; y < pixelHeight; y++)
                    {
                        f.pixels[x, y] = treatedPixels[x, y].R * 0x10000 +
                                        treatedPixels[x, y].G * 0x100 +
                                        treatedPixels[x, y].B;
                    }
                }
                RemoteControllerLink ctrl = remoteConctrollerLinks[parameters.ControllerID];
                ControllerReply rpl;
                lock (ctrl)
                {
                    rpl = ctrl.SignSetGraphicsFrame(f);
                }
                if (rpl.status != ControllerReply.Status.SUCCESS)
                {
                    throw new Exception(rpl.status.ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void SetFrameToCtrller(SignSetHighResolutionGraphicsFrame f, byte frmId, byte frmRev)
        {
            try
            {
                f.frameId = frmId;
                f.frameRev = frmRev;
                if (cbFrmColour.SelectedIndex < 0 || cbFrmColour.SelectedIndex > 11)
                {
                    throw new Exception("Illegal colour");
                }
                if (pixelHeight > 65535 || pixelWidth > 65535)
                {
                    throw new Exception("Can't fit in Graphics Frame");
                }
                f.colour = (FrameColour)Enum.Parse(typeof(FrameColour), cbFrmColour.SelectedValue.ToString());
                f.cd = (ConspicuityDevices)Enum.Parse(typeof(ConspicuityDevices), cbFrmConspicuity.SelectedValue.ToString());
                f.columns = (ushort)pixelWidth;
                f.rows = (ushort)pixelHeight;
                f.pixels = new int[pixelWidth, pixelHeight];
                for (int x = 0; x < pixelWidth; x++)
                {
                    for (int y = 0; y < pixelHeight; y++)
                    {
                        f.pixels[x, y] = treatedPixels[x, y].R * 0x10000 +
                                        treatedPixels[x, y].G * 0x100 +
                                        treatedPixels[x, y].B;
                    }
                }
                RemoteControllerLink ctrl = remoteConctrollerLinks[parameters.ControllerID];
                ControllerReply rpl;
                lock (ctrl)
                {
                    rpl = ctrl.SignSetHighResolutionGraphicsFrame(f);
                }
                if (rpl.status != ControllerReply.Status.SUCCESS)
                {
                    throw new Exception(rpl.status.ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        private void btnFrmDisplayFrame_Click(object sender, RoutedEventArgs e)
        {
            byte frmId = 1;
            if (!byte.TryParse(tbFrmFrameId.Text, out frmId) || frmId == 0)
            {
                MessageBox.Show("Illegal Frame ID");
                return;
            }
            RemoteControllerLink ctrl = remoteConctrollerLinks[parameters.ControllerID];
            ControllerReply rpl;
            rpl = ctrl.SignDisplayFrame(1, frmId);
            if (rpl.status != ControllerReply.Status.SUCCESS)
            {
                MessageBox.Show(rpl.status.ToString());
            }
        }

        private void btnFrmLoadImg_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Open BMP Image";
            openFileDialog.Filter = "Image File|*.bmp;";
            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }
            bmpPixels = BmpPixels.LoadFile(openFileDialog.FileName, pixelWidth, pixelHeight);
            Array.Copy(bmpPixels, treatedPixels, bmpPixels.Length);
            RedrawCanvas(bmpPixels);
        }

        private void RedrawCanvas(Color[,] mc)
        {
            gridFrmFrameContent.Children.Clear();
            gridFrmFrameContent.Children.Add(canvas);
            VirtualSign virtualSign = new VirtualSign(canvas, pixelWidth, pixelHeight);
            virtualSign.Display(mc);
        }

        private void cbFrmFont_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SignFont font = GetFont((ITS_FONT_SIZE)cbFrmFont.SelectedIndex);
            textRows = (pixelHeight + font.line_space) / font.rows_per_cell;
            if (textRows == 0) return;
            for (int i = 0; i < textLines.Length; i++)
            {
                textLines[i].Visibility = (i < textRows) ? Visibility.Visible : Visibility.Hidden;
            }
        }

        private void cbFrmColour_DropDownClosed(object sender, EventArgs e)
        {
            try
            {
                if (cbFrmFrameType.SelectedIndex < 0 || cbFrmColour.SelectedIndex < 0 ||
                    (cbFrmFrameType.SelectedIndex == 0 && cbFrmColour.SelectedIndex > 9) ||
                    (cbFrmFrameType.SelectedIndex == 1 && cbFrmColour.SelectedIndex > 10) ||
                    (cbFrmFrameType.SelectedIndex == 2 && cbFrmColour.SelectedIndex > 11))
                {
                    return;
                }
                if (cbFrmColour.SelectedIndex <= 9)
                {// mono
                    TreatMonoColour();
                    spPalette.Visibility = Visibility.Hidden;
                }
                else if (cbFrmColour.SelectedIndex == 10)
                {
                    TreatMuliColours();
                    spPalette.Visibility = Visibility.Visible;
                }
                else
                {
                    Array.Copy(bmpPixels, treatedPixels, bmpPixels.Length);
                    RedrawCanvas(treatedPixels);
                    spPalette.Visibility = Visibility.Hidden;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        class PaletteColour: StackPanel
        {
            public Color originalColour;
            public Button btn;
            public CbBox comboBox;
            public int index;
            string[] cstr;
            public PaletteColour(Color c, int index)
            {
                this.originalColour = c;
                this.index = index;
                this.Height = 26;
                this.Width = 100;
                this.Orientation = Orientation.Horizontal;
                btn = new Button
                {
                    Height = 16,
                    Width = 16,
                    VerticalAlignment = VerticalAlignment.Center,
                    Background = new SolidColorBrush(c)
                };
                cstr = new string[10];
                Array.Copy(COLOURS, 1, cstr, 1, 9);
                cstr[0] = "OFF";
                comboBox = new CbBox
                {
                    index = index,
                    ItemsSource = cstr,
                    Width = 80
                };
                Children.Add(btn);
                Children.Add(comboBox);
            }

        }

        public class CbBox : ComboBox
        {
            public int index;
        }

        private void TreatMuliColours()
        {
            List<Color> lst = new List<Color>();
            foreach (var v in bmpPixels)
            {
                if (!lst.Contains(v))
                {
                    lst.Add(v);
                    if (lst.Count > 16)
                    {
                        throw new Exception("Only Mono or 16-colour allowed");
                    }
                }
            }
            palette.Children.Clear();
            for (int i=0;i< lst.Count;i++)
            {
                PaletteColour p = new PaletteColour(lst[i],i);
                palette.Children.Add(p);
                p.comboBox.SelectionChanged += Colour4Changed;
            }
        }

        private void Colour4Changed(object sender, SelectionChangedEventArgs e)
        {
            CbBox cb = (sender as CbBox);
            PaletteColour p = (palette.Children[cb.index] as PaletteColour);
            for (int x = 0; x < pixelWidth; x++)
            {
                for (int y = 0; y < pixelHeight; y++)
                {
                    if (bmpPixels[x, y] == p.originalColour)
                    {
                        int c = MonoColourRGB[cb.SelectedIndex];
                        treatedPixels[x, y] = Color.FromRgb((byte)(c >> 16), (byte)(c >> 8), (byte)(c));
                    }
                }
            }
            RedrawCanvas(treatedPixels);
        }

        private void TreatMonoColour()
        {
            int c = MonoColourRGB[cbFrmColour.SelectedIndex];
            Color color = Color.FromRgb((byte)(c >> 16), (byte)(c >> 8), (byte)(c));
            for (int x = 0; x < pixelWidth; x++)
            {
                for (int y = 0; y < pixelHeight; y++)
                {
                    if (bmpPixels[x, y] == Colors.Black)
                    {
                        treatedPixels[x, y] = Colors.Black;
                    }
                    else
                    {
                        treatedPixels[x, y] = color;
                    }
                }
            }
            RedrawCanvas(treatedPixels);
        }

        private void btnView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            RedrawCanvas(bmpPixels);
        }

        private void btnView_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            RedrawCanvas(treatedPixels);
        }

    }
}
