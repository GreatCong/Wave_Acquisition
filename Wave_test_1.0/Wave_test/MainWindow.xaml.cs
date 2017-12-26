using System;
using System.Collections.Generic;
using System.IO.Ports;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Wave_test
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private PlotViewModel _viewModel;
        private SerialPort Usart = new SerialPort();//申明串口

        public MainWindow()
        {
            InitializeComponent();
            this.Title = this.Title + " --Mutiwave -- by Liucongjun";//列出标题
          
            LoadModelData();//初始化
         
        }

        #region 串口消息
        //打开串口
        private void Button_SerialOpen_Click(object sender, RoutedEventArgs e)
        {
            if (Button_SerialOpen.Content.Equals("Open"))
            {
                if (ComboBox_SerialName.Text == "")
                {
                    MessageBox.Show("请选择串口！", "错误");
                    return;
                }

                //串口参数初始化
                Usart.BaudRate = 115200;
                Usart.DataBits = 8;
                //停止位设置
                Usart.StopBits = System.IO.Ports.StopBits.One;
                //奇偶校验
                Usart.Parity = System.IO.Ports.Parity.None;

                //串口数据接收区大小
                Usart.ReadBufferSize = 4096 * 20;
                //准备就绪
                Usart.DtrEnable = true;//启用控制终端就续信号
                Usart.RtsEnable = true; //启用请求发送信号
                //设置数据读取超时为1秒
                Usart.ReadTimeout = 1000;
                Usart.ReceivedBytesThreshold = 1024;
                Usart.DataReceived += new SerialDataReceivedEventHandler(Usart_DataReceived);//DataReceived事件委托

                Usart.PortName = ComboBox_SerialName.SelectionBoxItem.ToString();
                if (Usart.IsOpen == false)
                {
                    Usart.Open();
                }
                //关闭串口选择功能
                ComboBox_SerialName.IsEnabled = false;

                Button_SerialOpen.Content = "Close";
            }
            else if (Button_SerialOpen.Content.Equals("Close"))
            {
                Usart.Close();

                //关闭串口选择功能

                ComboBox_SerialName.IsEnabled = true;

                Button_SerialOpen.Content = "Open";
            }
        }

        //下拉串口选择框
        private void ComboBox_SerialName_DropDownOpened(object sender, EventArgs e)
        {
            string[] names = SerialPort.GetPortNames();
            ComboBox_SerialName.Items.Clear();
            for (int i = 0; i < names.Length; i++)
            {
                ComboBox_SerialName.Items.Add(names[i]);
            }
        }

        //发送数据
        private void Button_SerialSend_Click(object sender, RoutedEventArgs e)
        {
            if (Usart.IsOpen == false)
            {
                MessageBox.Show("请开启串口！", "错误");
                return;
            }

            Usart.Write(TextBox_SerialSend.Text);
            //延时100ms
        }

        #endregion

        #region 串口事件处理
        private void Usart_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (Usart.IsOpen == false) return;
            try
            {
                //记录缓存数量
                int n = Usart.BytesToRead;

                if (n == 0)
                {
                    Usart.DiscardInBuffer();
                    return;
                }

                Console.WriteLine("n={0}", n);

                if (n > 10)
                {
                    //声明一个临时数组来存储当前来的串口数据
                    byte[] buf = new byte[n];

                        //读取缓冲数据
                        Usart.Read(buf, 0, n);

                        //丢弃接受缓冲区数据
                        // Usart.DiscardInBuffer();
                        for (int i = 0; i < n; i++)
                        {
                            _viewModel.Q_data.Enqueue(buf[i]);//队列在多线程时偶尔会出现“源数组长度不足，请检查Srcindex和长度以及数组的下限”错误，这里没有处理
                        }

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Usart.BytesToRead.ToString() + "接收串口信息错误：" + ex.Message);
                }
            //throw new NotImplementedException();
        }
        #endregion

        #region Init
        void LoadModelData()
        {            
            _viewModel = new PlotViewModel();//初始化model
            //画直线
            this.DataContext = _viewModel;
            _viewModel.SimplePlotModel.Title = "This is a test";//plot标题

            //绑定数据
            this.TextBoxPlot_dataX.SetBinding(TextBox.TextProperty, new Binding("Data_value") { Source = _viewModel.data_x = new Data_XYZ() });
            this.TextBoxPlot_dataY.SetBinding(TextBox.TextProperty, new Binding("Data_value") { Source = _viewModel.data_y = new Data_XYZ() });
            this.TextBoxPlot_dataZ.SetBinding(TextBox.TextProperty, new Binding("Data_value") { Source = _viewModel.data_z = new Data_XYZ() });

            ComboBox_PlotChannel_choose.Items.Add("1 Channnel");
            ComboBox_PlotChannel_choose.Items.Add("2 Channnels");
            ComboBox_PlotChannel_choose.Items.Add("3 Channnels");
            //默认显示的是1个通道
            ComboBox_PlotChannel_choose.SelectedIndex = 0;// -1表示未选中 
            
        }
        #endregion

        #region Plot消息
        //Start plot
        private void Button_PlotStart_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.isStop = false;
            _viewModel.Q_data.Clear();

            //改变显示的Channel数
            if (Usart.IsOpen == false)//串口不关闭，改变会有问题
            {
                _viewModel.setChannel_num(ComboBox_PlotChannel_choose.SelectedIndex + 1);
            }

            ComboBox_PlotChannel_choose.IsEnabled = false;
        }
        //Stop Plot
        private void Button_PlotStop_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.isStop = true;
            ComboBox_PlotChannel_choose.IsEnabled = true;
        }
        #endregion

    }

}
