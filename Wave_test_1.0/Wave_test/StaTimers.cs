using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Wave_test
{
    public partial class MainWindow : Window
    {
        #region global
        private DispatcherTimer clockTimer = new DispatcherTimer();// 用于更新时间的定时器
        #endregion

        #region StatusBar Timer
        // 定时器初始化
        private void InitClockTimer()
        {
            clockTimer.Interval = new TimeSpan(0, 0, 1);
            clockTimer.IsEnabled = true;
            clockTimer.Tick += ClockTimer_Tick;
            clockTimer.Start();
        }

        //Event handler for timers
        private void ClockTimer_Tick(object sender, EventArgs e)
        {
            UpdateTimeDate();

            if (statusInfoTextBlock.Text.Equals("创建成功！"))//在时间更新的同时，判断server是否有连接
            {
              if(netWork_connectionNum > 0){
                  statusInfoTextBlock.Text = "创建成功！共有" + netWork_connectionNum.ToString() + "个连接";
              }
            }            

        }
        #endregion
    }
}
