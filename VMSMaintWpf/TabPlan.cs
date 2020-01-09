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
        private void btnPlanGetFromCtrller_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte id;
                if (!byte.TryParse(tbPlanId.Text, out id) || id == 0)
                {
                    throw new Exception("Illegal Plan ID");
                }
                RemoteControllerLink ctrl = remoteConctrollerLinks[parameters.ControllerID];
                ControllerReply rpl;
                rpl = ctrl.SignRequestStoredPlan(id);
                if (rpl.status != ControllerReply.Status.SUCCESS)
                {
                    throw new Exception(rpl.status.ToString());
                }
                SignSetPlan plan = ctrl.Plans[id];
                tbPlanRev.Text = plan.planRev.ToString();
                for (int d = 0; d < 7; d++)
                {
                    tabPlanWeekdays[d].IsChecked = (plan.dayOfWeek & (1 << d)) != 0;
                }
                for (int i = 0; i < 6; i++)
                {
                    if (i < plan.planSubsq.Count)
                    {
                        tabPlanF_M_Mode[i].SelectedIndex = plan.planSubsq[i].peType - 1;
                        tabPlanF_M_Id[i].Text = plan.planSubsq[i].frmmsgId.ToString();
                        tabPlanStart[i].Text = plan.planSubsq[i].startHour + ":" + plan.planSubsq[i].startMinute.ToString("D2");
                        tabPlanStop[i].Text = plan.planSubsq[i].stopHour + ":" + plan.planSubsq[i].stopMinute.ToString("D2");
                    }
                    else
                    {
                        tabPlanF_M_Mode[i].SelectedIndex = -1;
                        tabPlanF_M_Id[i].Text = null;
                        tabPlanStart[i].Text = null;
                        tabPlanStop[i].Text = null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        private void btnPlanFrmSetToCtrller_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte id = 1;
                if (!byte.TryParse(tbPlanId.Text, out id) || id == 0)
                {
                    throw new Exception("Illegal Plan ID");
                }
                byte rev;
                if (!byte.TryParse(tbPlanRev.Text, out rev))
                {
                    throw new Exception("Illegal Plan Rev");
                }
                byte weekDays = 0;
                for (int i = 0; i < 7; i++)
                {
                    if (tabPlanWeekdays[i].IsChecked == true)
                    {
                        weekDays |= (byte)(1 << i);
                    }
                }
                if (weekDays == 0)
                {
                    throw new Exception("Week days error");
                }
                SignSetPlan plan = new SignSetPlan();
                plan.dayOfWeek = weekDays;
                plan.planId = id;
                plan.planRev = rev;
                plan.planSubsq = new List<PlanEntry>();
                for (int i = 0; i < 6; i++)
                {
                    if ((tabPlanF_M_Mode[i].SelectedIndex == 0 || tabPlanF_M_Mode[i].SelectedIndex == 1) &&
                        tabPlanF_M_Id[i].Text.Length > 0 && tabPlanStart[i].Text.Length > 0 && tabPlanStop[i].Text.Length > 0)
                    {
                        PlanEntry entry = new PlanEntry();
                        hh_mm(tabPlanStart[i].Text, out entry.startHour, out entry.startMinute);
                        hh_mm(tabPlanStop[i].Text, out entry.stopHour, out entry.stopMinute);
                        entry.peType = (byte)(tabPlanF_M_Mode[i].SelectedIndex + 1);
                        entry.frmmsgId = byte.Parse(tabPlanF_M_Id[i].Text);
                        plan.planSubsq.Add(entry);
                    }
                    else if (tabPlanF_M_Mode[i].SelectedIndex == -1 && tabPlanF_M_Id[i].Text.Length == 0
                         && tabPlanStart[i].Text.Length == 0 && tabPlanStop[i].Text.Length == 0)
                    {
                        if (i == 0) throw new Exception("Plan entry error");
                        break;
                    }
                    else
                    {
                        throw new Exception("Plan entry error");
                    }
                }
                RemoteControllerLink ctrl = remoteConctrollerLinks[parameters.ControllerID];
                ControllerReply rpl;
                lock (ctrl)
                {
                    rpl = ctrl.SignSetPlan(plan);
                }
                if (rpl.status != ControllerReply.Status.SUCCESS)
                {
                    MessageBox.Show(rpl.status.ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        void hh_mm(string str, out byte h, out byte m)
        {
            char[] s = new char[] { ',', '.', ':' };
            Exception ex = new Exception("Invalid Start or Stop Time");
            string[] ss = str.Split(s);
            if (ss.Length != 2) throw ex;
            h = byte.Parse(ss[0]);
            m = byte.Parse(ss[1]);
            if (h > 23 || m > 59) throw ex;
        }

        private void btnEnDisPlan(object sender, RoutedEventArgs e)
        {
            try
            {
                byte id;
                if (!byte.TryParse(tbPlanId.Text, out id) || id == 0)
                {
                    throw new Exception("Illegal Plan ID");
                }
                RemoteControllerLink ctrl = remoteConctrollerLinks[parameters.ControllerID];
                ControllerReply rpl;
                lock (ctrl)
                {
                    if (sender == btnPlanDisable)
                    {
                        rpl = ctrl.DisablePlan(1, id);
                    }
                    else if (sender == btnPlanEnable)
                    {
                        rpl = ctrl.EnablePlan(1, id);
                    }
                    else
                    {
                        throw new Exception("EnDisPlan ???");
                    }
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

        private void btnReqEnabledPlans_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RemoteControllerLink ctrl = remoteConctrollerLinks[parameters.ControllerID];
                ControllerReply rpl;
                lock (ctrl)
                {
                    rpl = ctrl.RequestEnabledPlans();
                }
                if (rpl.status != ControllerReply.Status.SUCCESS)
                {
                    throw new Exception(rpl.status.ToString());
                }
                if (ctrl.EnabledPlans.Count == 0)
                {
                    tbEnabledPlans.Text = "No enabled plan";
                }
                else
                {
                    StringBuilder s = new StringBuilder();
                    foreach (var v in ctrl.EnabledPlans)
                    {
                        s.AppendLine(String.Format("Group ID = {0}, Plan ID = {1}", v.groupId, v.planId));
                    }
                    tbEnabledPlans.Text = s.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

    }
}
