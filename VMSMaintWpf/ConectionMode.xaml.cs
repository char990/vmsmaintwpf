using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
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

using ByteStream;
using IniParser;
using IniParser.Model;
using IniParser.Parser;
using TsiSp003.Master;

namespace VMSMaintWpf
{
    /// <summary>
    /// Interaction logic for ConnectionMode.xaml
    /// </summary>
    public partial class ConnectionMode : Window
    {
        readonly int[] BAUD_RATE = { 9600, 19200, 38400, 57600, 115200 };

        ConnectionParameters parameters;
        public ConnectionMode(ConnectionParameters parameters)
        {
            InitializeComponent();
            wpCom.Visibility = Visibility.Hidden;
            wpTcpClient.Visibility = Visibility.Hidden;
            wpTcpServer.Visibility = Visibility.Hidden;
            cbBaudrate.ItemsSource = BAUD_RATE;
            this.parameters = parameters;
            parameters.RootName = null;
            ReadZone();
        }

        private void RadioButton_Checked_ConnectionMode(object sender, RoutedEventArgs e)
        {
            if(rbCom.IsChecked==true)
            {
                wpCom.Visibility = Visibility.Visible;
                wpTcpClient.Visibility = Visibility.Hidden;
                wpTcpServer.Visibility = Visibility.Hidden;
                string[] portStr = SerialPort.GetPortNames();
                if (portStr.Length == 0)
                {
                    MessageBox.Show("No COM port found!", "Error");
                    return;
                }

                foreach (string s in SerialPort.GetPortNames())
                {
                    cbComport.Items.Add(s);
                }
                cbComport.SelectedIndex = 0;
            }
            else if (rbTcpClient.IsChecked == true)
            {
                wpCom.Visibility = Visibility.Hidden;
                wpTcpClient.Visibility = Visibility.Visible;
                wpTcpServer.Visibility = Visibility.Hidden;
            }
            else if (rbTcpServer.IsChecked == true)
            {
                wpCom.Visibility = Visibility.Hidden;
                wpTcpClient.Visibility = Visibility.Hidden;
                wpTcpServer.Visibility = Visibility.Visible;
            }
        }

        private void tbConnect_Click(object sender, RoutedEventArgs e)
        {
            byte ctrlID;
            if(! byte.TryParse(tbControllerID.Text, out ctrlID))
            {
                MessageBox.Show("Contriller ID:" + ctrlID + "is not valid");
                return;
            }
            if (parameters.BroadcastIds.Contains(ctrlID))
            {
                MessageBox.Show("Contriller ID:" + ctrlID + "is used as Broadcast ID");
                return;
            }
            parameters.ControllerID = ctrlID;
            // check values
            if (rbCom.IsChecked == true)
            {
                try
                {
                    SerialPort serial_port = new SerialPort(
                        cbComport.SelectedValue.ToString(),
                        Int32.Parse(cbBaudrate.SelectedValue.ToString()),
                        Parity.None, 8, StopBits.One);
                    ComPort comPort = new ComPort(serial_port);
                    parameters.ByteStream = comPort;
                    parameters.RootName = String.Format("{0}:{1}:{2}bps 8-N-1",
                        cbZone.SelectedValue, serial_port.PortName, serial_port.BaudRate);
                }
                catch
                {
                    MessageBox.Show("Can't Open " + cbComport.SelectedValue.ToString());
                    return;
                }
            }
            else if (rbTcpClient.IsChecked == true)
            {
                try
                {
                    string ip = IPAddress.Parse(tbClientIp.Text).ToString();
                    int port = Int32.Parse(tbClientPort.Text);
                    TcpipClient tcpipClient = new TcpipClient(ip, port);
                    parameters.ByteStream = tcpipClient;
                    parameters.RootName = cbZone.SelectedValue+":"+ip+":"+port;
                }
                catch
                {
                    MessageBox.Show("Can't Open " + tbClientIp.Text + ":" + tbClientPort.Text);
                    return;
                }
            }
            else if (rbTcpServer.IsChecked == true)
            {

            }
            if(parameters.ByteStream == null)
            {
                MessageBox.Show("Connection parameters error!");
            }
            else
            {
                this.DialogResult = true;
                Close();    // close ConnectionMode : Window
            }
        }

        private void ReadZone()
        {
            try
            {
                var files = Directory.GetFiles("Zones", "*.ini");
                if (files.Length == 0)
                {
                    throw new Exception();
                }
                else
                {
                    string[] f = new string[files.Length];
                    for (int i=0;i< files.Length;i++)
                    {
                        f[i] = System.IO.Path.GetFileNameWithoutExtension(files[i]);
                    }
                    cbZone.ItemsSource = f;
                }
            }
            catch
            {
                throw new Exception("Can't get Zone ini file");
            }
        }

        private void cbZone_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var dataParser = new IniDataParser();
                dataParser.Configuration.CommentString = "//";
                var parser = new FileIniDataParser(dataParser);
                IniData data = parser.ReadFile("Zones\\" + cbZone.SelectedValue + ".ini");
                string clientIp = data["NETWORK"]["IP"];
                if(clientIp!=null)
                {
                    tbClientIp.Text = clientIp;
                }
                string clientPort = data["NETWORK"]["PORT"];
                if (clientPort != null)
                {
                    tbClientPort.Text = clientPort;
                }
                string baud = data["DIRECT"]["BAUD"];
                if(baud!=null)
                {
                    int baudrate = Int32.Parse(baud);
                    for(int i=0;i<BAUD_RATE.Length;i++)
                    {
                        if(baudrate == BAUD_RATE[i])
                        {
                            cbBaudrate.SelectedIndex = i;
                            break;
                        }
                    }
                }

                parameters.PwdOffset = ushort.Parse(data["OFFSETS"]["PASSWORD"], System.Globalization.NumberStyles.HexNumber);
                parameters.SeedOffset = byte.Parse(data["OFFSETS"]["SEED"],System.Globalization.NumberStyles.HexNumber);
                parameters.TimeoutMs = int.Parse(data["TIMER_INTERVAL"]["TIMEOUT"]);
                parameters.PollMs = int.Parse(data["TIMER_INTERVAL"]["POLL"]);
                parameters.BroadcastIds = new List<byte>();
                string broadcast = data["BROADCAST"]["ID"];
                if(broadcast==null)
                {
                    broadcast = "0";
                }
                string[] b_ids = broadcast.Split(',');
                foreach(var id in b_ids)
                {
                    byte k = byte.Parse(id);
                    if(parameters.BroadcastIds.Contains(k))
                    {
                        throw new Exception();
                    }
                    parameters.BroadcastIds.Add(k);
                }
                rbCom.IsChecked = true;
                tbConnect.IsEnabled = true;
            }
            catch
            {
                tbConnect.IsEnabled = false;
                MessageBox.Show("Parameters in Zone ini file error");
            }
        }
    }
}

/* ini file must be in Directory .\Zones
 * The following is the RTA_NSW.ini which is from VMS Maint49
 * But there is a mistake at [OFFSETS]
 * According to Tsi-Sp-003 : Chapter3.4, the seed offset is 8-bit and password offset is 16-bit
 * So here we follow the Tsi-Sp-003
 * [OFFSETS]
 * PASSWORD=7D1A     <---(1)		
 * SEED=83           <---(2)
[BROADCAST]
ID=0,255

[DIRECT]
COM=5
BAUD=38400

[MODEM]
PHONE=11111111

[NETWORK]
IP=127.0.0.1
PORT=5001

[DAILY LOG]
CREATE = 0
LOG="c:\VMS Maint 49\Log\VMSMaint.log"

[OFFSETS]
PASSWORD=83     <---(1)
SEED=7D1A       <---(2)

[SECURITY]
//KEY=70617373776f7264

//Poll_request = 1 than request the config
//Poll_request = 0 than doesn't request the config
[GETCONFIG]
POLL_REQUEST=0
GETTRAILORSTATUS=0

[TIMER_INTERVAL]
POLL = 1000
TIMEOUT= 1000

[DEVICEID ROLLOVER]
ENABLE=1
 */
