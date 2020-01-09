using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TsiSp003.ApplicationLayerData;
using TsiSp003.Master;

namespace VMSMaintWpf
{
    public partial class MainWindow : Window
    {
        private void btnMsgGetFromCtrller_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte id;
                if (!byte.TryParse(tbMsgId.Text, out id) || id == 0)
                {
                    throw new Exception("Illegal Message ID");
                }
                RemoteControllerLink ctrl = remoteConctrollerLinks[parameters.ControllerID];
                ControllerReply rpl;
                rpl = ctrl.SignRequestStoredMsg(id);
                if (rpl.status != ControllerReply.Status.SUCCESS)
                {
                    MessageBox.Show(rpl.status.ToString());
                    return;
                }
                SignSetMessage msg = ctrl.Messages[id];
                tbMsgRev.Text = msg.msgRev.ToString();
                tbMsgTransTime.Text = msg.transitionTime.ToString();
                for (int i = 0; i < 6; i++)
                {
                    if (i < msg.msgFrame.Count)
                    {
                        tabMsgFrmId[i].Text = msg.msgFrame[i].frameId.ToString();
                        tabMsgFrmOnTime[i].Text = msg.msgFrame[i].frameTime.ToString();
                    }
                    else
                    {
                        tabMsgFrmId[i].Text = null;
                        tabMsgFrmOnTime[i].Text = null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void btnMsgFrmSetToCtrller_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte id = 1;
                if (!byte.TryParse(tbMsgId.Text, out id) || id == 0)
                {
                    throw new Exception("Illegal Message ID");
                }
                byte rev;
                if (!byte.TryParse(tbMsgRev.Text, out rev))
                {
                    throw new Exception("Illegal Message Rev");
                }
                byte tt;
                if (!byte.TryParse(tbMsgTransTime.Text, out tt))
                {
                    throw new Exception("Illegal Transition Time");
                }
                SignSetMessage msg = new SignSetMessage();
                msg.msgId = id;
                msg.msgRev = rev;
                msg.transitionTime = tt;
                msg.msgFrame = new List<MsgFrame>();
                for (int i = 0; i < 6; i++)
                {
                    byte fid, time;
                    if (tabMsgFrmId[i].Text.Length > 0 && tabMsgFrmOnTime[i].Text.Length > 0)
                    {
                        fid = byte.Parse(tabMsgFrmId[i].Text);
                        time = byte.Parse(tabMsgFrmOnTime[i].Text);
                        if (fid == 0 || time == 0)
                        {
                            throw new Exception("Message entry error");
                        }
                        msg.msgFrame.Add(new MsgFrame() { frameId = fid, frameTime = time });
                    }
                    else if (tabMsgFrmId[i].Text.Length == 0 && tabMsgFrmOnTime[i].Text.Length == 0)
                    {
                        if (i == 0) throw new Exception("Message entry error");
                        break;
                    }
                    else
                    {
                        throw new Exception("Message entry error");
                    }
                }
                RemoteControllerLink ctrl = remoteConctrollerLinks[parameters.ControllerID];
                ControllerReply rpl;
                lock (ctrl)
                {
                    rpl = ctrl.SignSetMessage(msg);
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

        private void btnMsgDisplay_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte id;
                if (!byte.TryParse(tbMsgId.Text, out id) || id == 0)
                {
                    throw new Exception("Illegal Message ID");
                }
                RemoteControllerLink ctrl = remoteConctrollerLinks[parameters.ControllerID];
                ControllerReply rpl;
                lock (ctrl)
                {
                    rpl = ctrl.SignDisplayMessage(1, id);
                }
                if (rpl.status != ControllerReply.Status.SUCCESS)
                {
                    throw new Exception(rpl.status.ToString());
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
