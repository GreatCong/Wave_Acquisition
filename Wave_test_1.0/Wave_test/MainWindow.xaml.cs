using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Wave_test
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Global
        private PlotViewModel _viewModel;

        //数据绑定
        private Data_textDisplay textbox_receive_dis;
        private Data_textDisplay textbox_netWorkDataNum_dis;
        private Data_textDisplay textBox_netWorkPkgNum_dis;
        private Data_textDisplay textBox_netWorkTbPeriod_dis;

        //CheckBox
        private bool isSerialDataDispaly = false;

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            this.Title = this.Title + " --Mutiwave -- by Liucongjun";//列出标题

            LoadModelData();//初始化
            InitCore();

        }

        #region Init
        void LoadModelData()
        {
            _viewModel = new PlotViewModel();//初始化model
            //画直线
            this.DataContext = _viewModel;
            _viewModel.SimplePlotModel.Title = "MutiWave Display";//plot标题           

        }
        private void InitCore()
        {
            //绑定数据
            this.TextBoxPlot_dataX.SetBinding(TextBox.TextProperty, new Binding("Data_value") { Source = _viewModel.data_x = new Data_XYZ() });
            this.TextBoxPlot_dataY.SetBinding(TextBox.TextProperty, new Binding("Data_value") { Source = _viewModel.data_y = new Data_XYZ() });
            this.TextBoxPlot_dataZ.SetBinding(TextBox.TextProperty, new Binding("Data_value") { Source = _viewModel.data_z = new Data_XYZ() });

            this.TextBox_SerialReceive.SetBinding(TextBox.TextProperty, new Binding("Value_textDisplay") { Source = this.textbox_receive_dis = new Data_textDisplay() });
            this.TextBox_netWorkDataNum.SetBinding(TextBox.TextProperty, new Binding("Value_textDisplay") { Source = this.textbox_netWorkDataNum_dis = new Data_textDisplay() });
            this.TextBox_netWorkPkgNum.SetBinding(TextBox.TextProperty, new Binding("Value_textDisplay") { Source = this.textBox_netWorkPkgNum_dis = new Data_textDisplay() });
            this.TextBox_netWorkTbPeriod.SetBinding(TextBox.TextProperty, new Binding("Value_textDisplay") { Source = this.textBox_netWorkTbPeriod_dis = new Data_textDisplay() });

            ComboBox_PlotChannel_choose.Items.Add("1 Channnel");
            ComboBox_PlotChannel_choose.Items.Add("2 Channnels");
            ComboBox_PlotChannel_choose.Items.Add("3 Channnels");
            //默认显示的是1个通道
            ComboBox_PlotChannel_choose.SelectedIndex = 0;// -1表示未选中 

            Labe_PlotThreadState.Background = Brushes.Red;//设置初始化为红色

            //初始化chal(1--3) checked
            CheckBox_dataCH1_isDisplay.IsChecked = true;
            CheckBox_dataCH2_isDisplay.IsChecked = true;
            CheckBox_dataCH3_isDisplay.IsChecked = true;

            textBox_CH1_bias.Text = _viewModel.getDisplay_bias(0).ToString();
            textBox_CH2_bias.Text = _viewModel.getDisplay_bias(1).ToString();
            textBox_CH3_bias.Text = _viewModel.getDisplay_bias(2).ToString();

            netWork_init();
            InitClockTimer();
            LoadConfig();//没有配置信息就直接返回
        }
        #endregion

        #region Plot消息
        //Start plot
        private void Button_PlotStart_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.isStop = false;
            _viewModel.Q_data.Clear();
            Labe_PlotThreadState.Background = Brushes.Green;//设置颜色为绿色

            //改变显示的Channel数
            //if (Usart.IsOpen == false)//串口不关闭，改变会有问题
            //{
            _viewModel.setChannel_num(ComboBox_PlotChannel_choose.SelectedIndex + 1);
            //}

            ComboBox_PlotChannel_choose.IsEnabled = false;
        }
        //Stop Plot
        private void Button_PlotStop_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.isStop = true;
            ComboBox_PlotChannel_choose.IsEnabled = true;

            Labe_PlotThreadState.Background = Brushes.Red;//设置颜色为红色
        }
        #endregion

        #region 界面其他消息

        //采集按钮的消息
        private void Start_acq_Click(object sender, RoutedEventArgs e)
        {
            //if (Usart.IsOpen == false)
            //{
            //    MessageBox.Show("请开启串口！", "错误");
            //    return;
            //}

            //Usart.Write("Y");
            if (Start_acq.Content.Equals("开始采集"))
            {
                //if (SendCMD(AD_Ctrl, 1) == false) return;//start
                if (SendByte((byte)'Y') == false) return;//start
                //cbRX.Checked = false;
                //btnOpen.Enabled = false;
                //btnSaveToFile.Enabled = false;
                //btnDrawFromFile.Enabled = false;
                Start_acq.Content = "停止采集";
            }
            else
            {
                //if (SendCMD(AD_Ctrl, 0) == false) return;//stop
                if (SendByte((byte)'N') == false) return;//
                //cbRX.Checked = true;
                //btnOpen.Enabled = true;
                //btnSaveToFile.Enabled = true;
                //btnDrawFromFile.Enabled = true;
                Start_acq.Content = "开始采集";
            }
        }

        //private void Stop_acq_Click(object sender, RoutedEventArgs e)
        //{
        //    if (Usart.IsOpen == false)
        //    {
        //        MessageBox.Show("请开启串口！", "错误");
        //        return;
        //    }

        //    Usart.Write("N");
        //}

        //选择发送的数据是否显示在databox中
        private void CheckBox_IsSerialDataDis_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBox_IsSerialDataDis.IsChecked == true)
            {
                isSerialDataDispaly = true;
            }
            else if (CheckBox_IsSerialDataDis.IsChecked == false)
            {
                isSerialDataDispaly = false;
            }
        }

        //tabcontrol 选择是USB还是network
        private void TabControl_TransportChoose_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TabControl_TransportChoose.SelectedItem == USB_tabItem)
            {
                _viewModel.Int_TabControl_TransportChoose_select = 0;
            }
            else if (TabControl_TransportChoose.SelectedItem == WiFi_tabItem)
            {
                _viewModel.Int_TabControl_TransportChoose_select = 1;
            }
        }

        //发送数据
        private void Button_SmartSend_Click(object sender, RoutedEventArgs e)
        {
            //if (Usart.IsOpen == false)
            //{
            //    MessageBox.Show("请开启串口！", "错误");
            //    return;
            //}

            //Usart.Write(TextBox_SerialSend.Text);
            byte[] byteArray = System.Text.Encoding.Default.GetBytes(TextBox_SerialSend.Text);
            SmartSend(byteArray);
            //延时100ms
        }

        //清除数据
        private void Button_SmartSend_clear_Click(object sender, RoutedEventArgs e)
        {
            TextBox_SerialReceive.Text = "";//清除文本框的内容
        }

        //通道1显示
        private void CheckBox_dataCH1_isDisplay_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBox_dataCH1_isDisplay.IsChecked == true)
            {
                _viewModel.is_dataCH1_Display = true;
            }
            else if (CheckBox_dataCH1_isDisplay.IsChecked == false)
            {
                _viewModel.is_dataCH1_Display = false;
            }
        }

        //通道2显示
        private void CheckBox_dataCH2_isDisplay_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBox_dataCH2_isDisplay.IsChecked == true)
            {
                _viewModel.is_dataCH2_Display = true;
            }
            else if (CheckBox_dataCH2_isDisplay.IsChecked == false)
            {
                _viewModel.is_dataCH2_Display = false;
            }
        }

        //通道3显示
        private void CheckBox_dataCH3_isDisplay_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBox_dataCH3_isDisplay.IsChecked == true)
            {
                _viewModel.is_dataCH3_Display = true;
            }
            else if (CheckBox_dataCH3_isDisplay.IsChecked == false)
            {
                _viewModel.is_dataCH3_Display = false;
            }
        }

        //偏置显示Ch1
        private void textBox_CH1_bias_KeyDown(object sender, KeyEventArgs e)
        {
            _viewModel.setDisplay_bias(0, Convert.ToInt32(textBox_CH1_bias.Text.ToString()));
        }

        //偏置显示Ch2
        private void textBox_CH2_bias_KeyDown(object sender, KeyEventArgs e)
        {
            _viewModel.setDisplay_bias(1, Convert.ToInt32(textBox_CH2_bias.Text.ToString()));
        }

        //偏置显示Ch3
        private void textBox_CH3_bias_KeyDown(object sender, KeyEventArgs e)
        {
            _viewModel.setDisplay_bias(2, Convert.ToInt32(textBox_CH3_bias.Text.ToString()));
        }

        #endregion

        #region windows 窗体消息

        //窗口关闭消息
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Usart.IsOpen)
            {
                Usart.Close();
            }

            MessageBoxResult msg_res = MessageBox.Show("是否在退出前保存软件配置？", "小贴士", MessageBoxButton.YesNoCancel, MessageBoxImage.Information);

            // 提示是否需要保存配置到文件中
            if (msg_res == MessageBoxResult.Yes)
            {
                SaveConfig();
            }
            else if(msg_res == MessageBoxResult.Cancel)
            {
                e.Cancel = true;//不关闭窗口
            }
           
        }

        #endregion

        

    }

}
