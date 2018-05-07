using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Wave_test
{
    public partial class MainWindow : Window
    {
        #region global

        window_settings settings_plot = new window_settings();//父窗口与子窗口数据交互类

        #endregion

        #region 子窗口交互消息
        //采用委托的方式进行数据交互

        //menu setting click消息
        private void MenuItem_PlotSeting_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.isStop)
            {
                PlotSetting_window setting_frm = new PlotSetting_window();
                //订阅事件
                setting_frm.ChangeSettingHandlerEvent += new ChangeSettingHandler(setting_frm_ChangeSettingEvent);

                setting_frm.settings = settings_plot;//父窗口向子窗口传递
                setting_frm.ShowDialog();
            }

        }

        //自定义委托消息
        void setting_frm_ChangeSettingEvent(window_settings setting)
        {
            //this.TextBox_SerialReceive.Text = setting.namesY + setting.value_plusY.ToString()+setting.value_divY.ToString();

            _viewModel.setDisplay_plusY(setting.value_plusY);
            _viewModel.setDisplay_divY(setting.value_divY);
            _viewModel.setDisplay_Ytitle(setting.namesY);

            _viewModel.setDisplay_plotTime(setting.plot_sleepTime);//plot线程休眠时间

            this.settings_plot = setting;//保存当前的设置
        }


        #endregion
    }

    #region 自定义子窗口数据交互类
    /// <summary>
    /// 自定义类,用于子窗口的数据交互
    /// </summary>
    public class window_settings
    {
        public int value_plusY = 1;
        public int value_divY = 1;
        public string namesY = "mV";
        public int plot_sleepTime = 50;//50ms

        public window_settings()
        {

        }
    }
    #endregion
}
