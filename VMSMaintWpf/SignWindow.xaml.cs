using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TsiSp003.ApplicationLayerData;
using TsiSp003.Master;

namespace VMSMaintWpf
{
    /// <summary>
    /// Interaction logic for SignWindow.xaml
    /// </summary>
    public partial class SignWindow : Window
    {
        int Y;
        int X;
        int squareSize;
        RemoteControllerLink ctrl;
        BackgroundWorker bgWorker;
        Button s;
        VirtualSign virtualSign;

        public SignWindow(RemoteControllerLink ctrl, Button s)
        {
            InitializeComponent();
            this.ctrl = ctrl;
            this.s = s;
            if (ctrl.Scr != null)
            {
                X = ctrl.Scr.groupConfigs[0].signConfigs[0].width;
                Y = ctrl.Scr.groupConfigs[0].signConfigs[0].height;
            }
            else if (ctrl.Sesr != null)
            {
                X = ctrl.Sesr.signExtendedStatusEntry[0].columns;
                Y = ctrl.Sesr.signExtendedStatusEntry[0].rows;
            }
            else
            {
                throw new Exception("Can't get sign width & height");
            }
            virtualSign = new VirtualSign(SignArea, X, Y);

            bgWorker = new BackgroundWorker();
            bgWorker.WorkerReportsProgress = true;
            bgWorker.WorkerSupportsCancellation = true;
            bgWorker.DoWork += DoWork_Handler;
            bgWorker.ProgressChanged += ProgressChanged_Handler;
            //bgWorker.RunWorkerCompleted += RunWorkerCompleted_Handler;
            bgWorker.RunWorkerAsync(ctrl);
            s.IsEnabled = false;
        }

        private void DoWork_Handler(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            ControllerReply rpl;
            RemoteControllerLink _ctrl = (RemoteControllerLink)e.Argument;
            while (true)
            {
                lock (_ctrl)
                {
                    rpl = _ctrl.HeartbeatPoll();
                    if (rpl.status == ControllerReply.Status.SUCCESS)
                    {
                        byte id = _ctrl.Ssr.signStatusEntry[0].frameId;
                        if (id != 0 && _ctrl.Frames[id] == null)
                        {
                            rpl = _ctrl.SignRequestStoredFrm(id);
                        }
                        if (rpl.status == ControllerReply.Status.SUCCESS)
                        {
                            worker.ReportProgress(id);
                        }
                    }
                }
                for (int i = 0; i < 10; i++)
                {
                    if (worker.CancellationPending)
                    {
                        return;
                    }
                    Thread.Sleep(100);
                }
            }
        }

        private void ProgressChanged_Handler(object sender, ProgressChangedEventArgs e)
        {
            int frameId = e.ProgressPercentage;
            if (frameId != 0)
            {
                this.Title = String.Format("Plan ID:[{0}], Msg ID:[{1}], Frame ID:[{2}], RTC={3}",
                    ctrl.Ssr.signStatusEntry[0].planId,
                    ctrl.Ssr.signStatusEntry[0].msgId,
                    ctrl.Ssr.signStatusEntry[0].frameId,
                    ctrl.Ssr.datetime);
                if (ctrl.Frames[frameId] is SignSetTextFrame)
                {
                    virtualSign.AllOff();
                }
                else if (ctrl.Frames[frameId] is SignSetGraphicsFrame)
                {
                    virtualSign.Display((ctrl.Frames[frameId] as SignSetGraphicsFrame).pixels);
                }
                else if (ctrl.Frames[frameId] is SignSetHighResolutionGraphicsFrame)
                {
                    virtualSign.Display((ctrl.Frames[frameId] as SignSetHighResolutionGraphicsFrame).pixels);
                }
                else
                {
                    throw new Exception("Frame is unknown");
                }
            }
        }

        private void RunWorkerCompleted_Handler(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("Sign work done");
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            bgWorker.CancelAsync();
            s.IsEnabled = true;
        }
    }
}
