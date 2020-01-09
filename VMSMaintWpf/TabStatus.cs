using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TsiSp003;
using TsiSp003.ApplicationLayerData;
using TsiSp003.Master;

namespace VMSMaintWpf
{
    public partial class MainWindow : Window
    {

        private void btnRefreshStatus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RemoteControllerLink ctrl = remoteConctrollerLinks[parameters.ControllerID];
                ControllerReply rpl;
                lock (ctrl)
                {
                    /*
                    rpl = ctrl.SignConfigurationRequest();
                    if (rpl.status == ControllerReply.Status.SUCCESS)
                    {
                        pixelWidth = ctrl.Scr.groupConfigs[0].signConfigs[0].width;
                        pixelHeight = ctrl.Scr.groupConfigs[0].signConfigs[0].height;
                    }*/
                    rpl = ctrl.SignExtendedStatusRequest();
                    if (rpl.status == ControllerReply.Status.SUCCESS)
                    {
                        foreach (var s in cbFrmFrameType.Items)
                        {
                            (s as Control).IsEnabled = false;
                        }
                        switch (ctrl.Sesr.signExtendedStatusEntry[0].signType)
                        {
                            case ConstCode.SesrSignType.TEXT:
                                pixelWidth = 0;
                                pixelHeight = 0;
                                (cbFrmFrameType.Items[0] as Control).IsEnabled = true;
                                break;
                            case ConstCode.SesrSignType.GFX:
                                pixelWidth = ctrl.Sesr.signExtendedStatusEntry[0].columns;
                                pixelHeight = ctrl.Sesr.signExtendedStatusEntry[0].rows;
                                (cbFrmFrameType.Items[0] as Control).IsEnabled = true;
                                (cbFrmFrameType.Items[1] as Control).IsEnabled = true;
                                (cbFrmFrameType.Items[2] as Control).IsEnabled = true;
                                break;
                            case ConstCode.SesrSignType.ADVGFX:
                                pixelWidth = ctrl.Sesr.signExtendedStatusEntry[0].status[0]*0x100+
                                             ctrl.Sesr.signExtendedStatusEntry[0].status[1];
                                pixelHeight = ctrl.Sesr.signExtendedStatusEntry[0].status[2] * 0x100 +
                                             ctrl.Sesr.signExtendedStatusEntry[0].status[3];
                                (cbFrmFrameType.Items[0] as Control).IsEnabled = true;
                                if(pixelWidth<256 || pixelHeight<256)
                                {
                                    (cbFrmFrameType.Items[1] as Control).IsEnabled = true;
                                }
                                (cbFrmFrameType.Items[2] as Control).IsEnabled = true;
                                break;
                            default:
                                throw new Exception("Sign Extended Status Request Got illegal sign type");
                        }

                    }
                    else
                    {
                        throw new Exception("Sign Extended Status Request:" + rpl.status.ToString());
                    }
                    /*
                    RtaFont.SignFont font = RtaFont.GetFont(RtaFont.ITS_FONT_SIZE.FONT_DEFAULT);

                    int nlines = (txtRows + font.line_space) / font.rows_per_cell;
                    textLines = new TextBox[nlines];
                    for (int i = 0; i < nlines; i++)
                    {
                        textLines[i] = new TextBox();
                        spTxtFrmLines.Children.Add(textLines[i]);
                    }*/
                    rpl = ctrl.HeartbeatPoll();
                    if (rpl.status != ControllerReply.Status.SUCCESS)
                    {
                        throw new Exception("Heartbeat Poll:" + rpl.status.ToString());
                    }
                }
                bmpPixels = new System.Windows.Media.Color[pixelWidth, pixelHeight];
                treatedPixels = new System.Windows.Media.Color[pixelWidth, pixelHeight];
                canvas.Width = pixelWidth;
                canvas.Height = pixelHeight;
                StRefreshSsr(ctrl.Ssr);
                tbStMfcCode.Text = Encoding.ASCII.GetString(ctrl.Sesr.manufacturer);
                SignExtendedStatusEntry sese = ctrl.Sesr.signExtendedStatusEntry[0];
                tbStSignType.Text = sese.signType.ToString();
                tbSignDimension.Text = pixelWidth + "*" + pixelHeight;
                tbStDimmingMode.Text = sese.dimmingMode == 0 ? "Auto" : "Manual";
                tbStDimmingLvl.Text = sese.dimmingLevel.ToString();
                tbStLedStatus.Text = BitConverter.ToString(sese.status.ToArray());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void StRefreshSsr(SignStatusReply ssr)
        {
            tbStOnline.Text = ssr.isOnline ? "On-line" : "Off-line";
            tbStAppError.Text = Byte2Hex(ssr.appErrCode);
            tbStCtrllerRtc.Text = ssr.datetime.ToString();
            tbStHdwChk.Text = Ushort2Hex(ssr.ctrlerHdwChksum);
            tbStCtrlerError.Text = Byte2Hex(ssr.ctrlerErrCode);
            tbStSigns.Text = ssr.numberOfSigns.ToString();
            SignStatusEntry sse = ssr.signStatusEntry[0];
            tbStSignErrCode.Text = sse.signErrCode.ToString();
            tbStSignEnDis.Text = sse.isSignEnabled?"Enabled":"Disabled";
            tbStFrmId.Text = sse.frameId.ToString();
            tbStFrmRev.Text = sse.frameRev.ToString();
            tbStMsgId.Text = sse.msgId.ToString();
            tbStMsgRev.Text = sse.msgRev.ToString();
            tbStPlanId.Text = sse.planId.ToString();
            tbStPlanRev.Text = sse.planRev.ToString();
        }


        private void cbStSigns_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbStSigns.SelectedIndex >= 0)
            {
            }
        }

    }
}
