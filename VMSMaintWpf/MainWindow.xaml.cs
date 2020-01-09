using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TsiSp003;
using TsiSp003.Master;
using ByteStream;
using System.ComponentModel;
using System.Threading;

namespace VMSMaintWpf
{
    /// <summary>
    /// Interaction logic for VmsMaintDesktop.xaml
    /// Max pixels = 288 * 64
    /// </summary>

    public partial class MainWindow : Window
    {
        ConnectionParameters parameters = new ConnectionParameters();
        ProtocolWindow protocolWindow;
        SignWindow signWindow;
        DataLink dataLink;
        private RemoteControllerLink[] remoteConctrollerLinks = new RemoteControllerLink[256];
        TextBox[] tabMsgFrmId;
        TextBox[] tabMsgFrmOnTime;

        ComboBox[] tabPlanF_M_Mode;
        TextBox[] tabPlanF_M_Id;
        TextBox[] tabPlanStart;
        TextBox[] tabPlanStop;
        CheckBox[] tabPlanWeekdays;


        public MainWindow()
        {
            InitializeComponent();
            ConnectionMode connectionMode = new ConnectionMode(parameters);
            connectionMode.ShowDialog();
            protocolWindow = ProtocolWindow.GetProtocolWindow();
            dataLink = new DataLink(parameters.ByteStream);
            dataLink.DataRcvd += DataRxd;
            dataLink.DataSent += DataTxd;
            lblRootName.Content = parameters.RootName;
            /*
            if (parameters.BroadcastIds.Count > 0)
            {
                foreach (var s in parameters.BroadcastIds)
                {
                    lstController.Items.Add("Broadcast:" + s);
                }
            }
            */
            lstController.Items.Add("Controller:" + parameters.ControllerID);
            remoteConctrollerLinks[parameters.ControllerID] =
                new RemoteControllerLink(parameters, dataLink);

            tabMsgFrmId = new TextBox[6] { tbMsgFrmId1, tbMsgFrmId2, tbMsgFrmId3, tbMsgFrmId4, tbMsgFrmId5, tbMsgFrmId6 };
            tabMsgFrmOnTime = new TextBox[6] { tbMsgFrmOnTime1, tbMsgFrmOnTime2, tbMsgFrmOnTime3, tbMsgFrmOnTime4, tbMsgFrmOnTime5, tbMsgFrmOnTime6 };
            tabPlanF_M_Mode = new ComboBox[6] { cbPlanFrmMsg1, cbPlanFrmMsg2, cbPlanFrmMsg3, cbPlanFrmMsg4, cbPlanFrmMsg5, cbPlanFrmMsg6};
            tabPlanF_M_Id = new TextBox[6] { tbPlanFMId1, tbPlanFMId2, tbPlanFMId3, tbPlanFMId4, tbPlanFMId5, tbPlanFMId6};
            tabPlanStart = new TextBox[6] { tbPlanStartTime1, tbPlanStartTime2, tbPlanStartTime3, tbPlanStartTime4, tbPlanStartTime5, tbPlanStartTime6};
            tabPlanStop = new TextBox[6] { tbPlanStopTime1, tbPlanStopTime2, tbPlanStopTime3, tbPlanStopTime4, tbPlanStopTime5, tbPlanStopTime6};
            tabPlanWeekdays = new CheckBox[7] { cbSun, cbMon, cbTue, cbWed, cbThu, cbFri, cbSat };
            for(int i=0;i<6;i++)
            {
                tabPlanF_M_Mode[i].Items.Add("Frame");
                tabPlanF_M_Mode[i].Items.Add("Message");
            }

            cbFrmFont.ItemsSource = Enum.GetNames(typeof(ConstCode.FrameFont));
            cbFrmFont.SelectedIndex = 0;
            cbFrmConspicuity.ItemsSource = Enum.GetNames(typeof(ConstCode.ConspicuityDevices));
            cbFrmConspicuity.SelectedIndex = 0;

            lblFrmFont.Visibility = Visibility.Hidden;
            cbFrmFont.Visibility = Visibility.Hidden;
            btnFrmLoadImg.Visibility = Visibility.Hidden;
            spPalette.Visibility = Visibility.Hidden;

            btnRefreshStatus_Click(null, null);
        }

        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private string onlineStatus;
        public string OnlineStatus
        {
            get { return onlineStatus; }
            set
            {
                onlineStatus = value;
                OnPropertyChanged("OnlineStatus");
            }
        }



        #region Protocol Monitor
        string StreamDataTrans(byte[] data, string dir)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var s in data)
            {
                if (s < 0x20)
                {
                    if (s == (byte)TsiSp003.Packets.CtrlChar.SOH ||
                        s == (byte)TsiSp003.Packets.CtrlChar.ACK ||
                        s == (byte)TsiSp003.Packets.CtrlChar.NAK)
                    {
                        sb.AppendLine();
                        sb.Append(DateTime.Now.ToString("hh:mm:ss.fff") + dir);
                    }
                    sb.Append("<" + s.ToString("X2") + ">");
                }
                else
                {
                    sb.Append(((char)s).ToString());
                }
            }
            return sb.ToString();
        }



        void DataRxd(byte[] rx)
        {
            protocolWindow.AppendText(StreamDataTrans(rx, "<- "));
        }

        void DataTxd(byte[] tx)
        {
            protocolWindow.AppendText(StreamDataTrans(tx, "-> "));
        }

        private void ProtocolMonitor_Click(object sender, RoutedEventArgs e)
        {
            if(!protocolWindow.IsVisible) protocolWindow.Show();
        }
        #endregion

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (dataLink != null)
            {
                dataLink.Close();
            }
            protocolWindow.SetClose(true);
            protocolWindow.Close();
            signWindow?.Close();
        }

        private void ShowSign_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                signWindow = new SignWindow(remoteConctrollerLinks[parameters.ControllerID], btnShowSign);
                signWindow.Show();
            }
            catch
            {
                MessageBox.Show("There is no valid sign");
            }
        }

        string Byte2Hex(byte b)
        {
            return string.Format("0x{0:X2}", b);
        }
        string Ushort2Hex(ushort b)
        {
            return string.Format("0x{0:X4}", b);
        }

    }
}
