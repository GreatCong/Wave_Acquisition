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

namespace Wave_test
{
    //定义委托
    public delegate void ChangeSettingHandler(window_settings setting);

    /// <summary>
    /// PlotSetting_window.xaml 的交互逻辑
    /// </summary>
    public partial class PlotSetting_window : Window
    {
        #region  global
       
        public event ChangeSettingHandler ChangeSettingHandlerEvent; //定义事件

        public window_settings settings = new window_settings();//定义数据交互类
        #endregion

        public PlotSetting_window()
        {
            InitializeComponent();
            this.Title = "Plot Setting";
        }

        #region init
        /// <summary>
        /// 初始化父窗口的数据
        /// </summary>
        private void PreInitCores()
        {
            textBox_namesY.Text = settings.namesY;
            textBox_valuePlusY.Text = settings.value_plusY.ToString();
            textBox_valueDivY.Text = settings.value_divY.ToString(); ;

            radioButton_isZoom.IsChecked = true;
        }

        #endregion

        #region 界面其他消息

        /// <summary>
        /// 确认消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_OK_Click(object sender, RoutedEventArgs e)
        {

            //引发事件
            if (ChangeSettingHandlerEvent != null)
            {
                //window_settings settings = new window_settings();
                
                try
                {
                    settings.namesY = textBox_namesY.Text;
                    settings.value_plusY = Convert.ToInt32(textBox_valuePlusY.Text.ToString());
                    settings.value_divY = Convert.ToInt32(textBox_valueDivY.Text.ToString());
                }
                catch (SystemException ep){
                    MessageBox.Show(ep.Message, "setting error!");
                    return;
                }


                ChangeSettingHandlerEvent(settings);
            }

            this.Close();//关闭窗口
        }

        #endregion

        #region 窗口消息

        //父窗口向子窗口传递消息，最好是在Window_Loaded事件中
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PreInitCores();
        }

        #endregion
    }


}
