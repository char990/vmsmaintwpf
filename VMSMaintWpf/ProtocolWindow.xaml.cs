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

namespace VMSMaintWpf
{
    /// <summary>
    /// Interaction logic for ProtocolWindow.xaml
    /// </summary>
    public partial class ProtocolWindow : Window
    {
        static private ProtocolWindow protocolWindow;

        public delegate void ProtocolAppendText(string s);
        ProtocolAppendText _app_txt;

        private ProtocolWindow()
        {
            InitializeComponent();
            _app_txt = _AppendText;
        }

        static public ProtocolWindow GetProtocolWindow()
        {
            if(protocolWindow==null)
            {
                protocolWindow = new ProtocolWindow();
            }
            return protocolWindow;
        }

        private void btnPtClearData_Click(object sender, RoutedEventArgs e)
        {
            tbProtocol.Clear();
        }

        private void _AppendText(string s)
        {
            if (tbProtocol.Text.Length > 1024 * 1024)
            {
                tbProtocol.Text = "";
            }
            tbProtocol.AppendText(s);
            if (chkPtToEnd.IsChecked == true)
            {
                tbProtocol.ScrollToEnd();
            }
        }

        public void AppendText(string s)
        {
            this.Dispatcher.Invoke(_app_txt,s);
        }

        private bool close=false;
        public void SetClose(bool c)
        {
            close = c;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!close)
            {
                e.Cancel = true;
                this.Hide();
            }
        }
    }
}
