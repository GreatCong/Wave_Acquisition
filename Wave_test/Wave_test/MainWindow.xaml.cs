using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System;
using System.Collections;
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
using System.Windows.Threading;

namespace Wave_test
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableDataSource<Point> dataSource1 = new ObservableDataSource<Point>(); //数据源
        private LineGraph graphWave1 = new LineGraph(); //线的注释类

        private DispatcherTimer timer = new DispatcherTimer();//timer
        //private int i = 0;
        Header chartHeader = new Header();//列出标题
        TextBlock headerContents = new TextBlock();//标题内容类
        bool reset_state = true;
        delegate void HandleInterfaceUpdateDelagate(string text);//委托；此为重点
        HandleInterfaceUpdateDelagate interfaceUpdateHandle;
        HorizontalAxisTitle chartHorizontalAxisTitle = new HorizontalAxisTitle();//x轴内容
        VerticalAxisTitle chartVerticalAxisTitle = new VerticalAxisTitle();//y轴内容
        TextBlock chartXContents= new TextBlock();
        TextBlock chartYContents = new TextBlock();
        bool IsSerialDataDisplay = false;
        Queue<byte> Q_data = new Queue<byte>();

        //声明串口
        private SerialPort Usart = new SerialPort();

        public MainWindow()
        {
            InitializeComponent();
            this.Title = this.Title + " --Mutiwave -- by Liucongjun";//列出标题

            headerContents.Text = "This is a test";
            headerContents.HorizontalAlignment = HorizontalAlignment.Center;//居中显示
            chartHeader.Content = headerContents;
            plotter.Children.Add(chartHeader);//将标题内容写入

            chartXContents.Text = "x轴";
            chartXContents.HorizontalAlignment = HorizontalAlignment.Center;//居中显示
            chartHorizontalAxisTitle.Content = chartXContents;
            plotter.Children.Add(chartHorizontalAxisTitle);//将X轴内容写入

            chartYContents.Text = "y轴";
            chartYContents.HorizontalAlignment = HorizontalAlignment.Center;//居中显示
            chartVerticalAxisTitle.Content = chartYContents;
            plotter.Children.Add(chartVerticalAxisTitle);//将Y轴内容写入
        }

        private void button_Start_Click(object sender, RoutedEventArgs e)
        {
            //plotter.AddLineGraph(dataSource, Colors.Green, 2); 
            timer.Interval = TimeSpan.FromSeconds(0.01);
            timer.Tick += new EventHandler(AnimatedPlot);//AnimatedPlot相当于是timer的回调
            timer.IsEnabled = true;//开启timer

            Q_data.Clear();//清除队列信息

            if (reset_state)
            {
                graphWave1 = plotter.AddLineGraph(dataSource1, Colors.Red, 2, "X轴");
                reset_state = false;
            }
            // Force evertyhing plotted to be visible
            plotter.Viewport.FitToView();
        }

        private void button_stop_Click(object sender, RoutedEventArgs e)
        {
            timer.IsEnabled = false;
        }

        private void button_Clear_Click(object sender, RoutedEventArgs e)
        {
            //i = 0;
            reset_state = true;
            timer.IsEnabled = false; 
            plotter.Children.Remove(graphWave1);
            //需要注意的就是清除示波器数据时，除了要用plotter.Children.Remove（）指令，将此通道曲线移除，还要将数据源里数据移除。
            dataSource1 = new ObservableDataSource<Point>();
        }
        //int xaxis = 0;
        //int group = 1024;
        //timer回调
        private void AnimatedPlot(object sender, EventArgs e)
        {
            //double x = i;
            //double y1 = Math.Sin(i * 0.2);

            //dataSource1 = new ObservableDataSource<Point>();
            //graphWave1.DataSource = dataSource1;

            //for (int ii = 0; ii < 500; ii++)
            //{
            //    Random rd = new Random();
            //    int RandKey = rd.Next(1, 999);//产生随机数
            //    Point point1 = new Point(ii, Math.Sin(RandKey * 0.02));
            //    // 追加至Plot
            //    dataSource1.AppendAsync(base.Dispatcher, point1);
            //    //i++;
            //}

            //Point point1 = new Point(x, y1);
            //Point point2 = new Point(x, y2);
            //// 追加至Plot
            //dataSource1.AppendAsync(base.Dispatcher, point1);
            //dataSource2.AppendAsync(base.Dispatcher, point2);
            //i++;
            //下面的代码是保持x轴不缩放
            //if (x - group > 0)
            //    xaxis = (int)x - group;
            //else
            //    xaxis = 0;

            //plotter.Viewport.Visible = new System.Windows.Rect(xaxis, -2, group, 4);//主要注意这里一行

            if (Usart.IsOpen == false) return;
            try
            {
                //记录缓存数量
                //int n = Usart.BytesToRead;

                int n_thresh = 2048;
                int n = Q_data.Count;
               //Console.WriteLine("n={0}", n);
                if (n == 0) return;

                if (n >= n_thresh)
                {
                    //声明一个临时数组来存储当前来的串口数据
                    byte[] buf_temp = new byte[n_thresh];

                    //读取缓冲数据
                   // Usart.Read(buf, 0, n_thresh);
                    //Console.WriteLine("n={0}", n_thresh);

                    //丢弃接受缓冲区数据
                    //Usart.DiscardInBuffer();


                    dataSource1 = new ObservableDataSource<Point>();
                    graphWave1.DataSource = dataSource1;

                    for (int ii = 0; ii < n_thresh / 2; ii++)
                    {
                        //ushort ushort_temp = BitConverter.ToUInt16(buf, 2 * ii);
                        byte temp1 = Q_data.Dequeue();
                        byte temp2 = Q_data.Dequeue();
                        //int ushort_temp = (ushort)(buf_temp[2 * ii] << 8) | buf_temp[2 * ii + 1];//位操作最好加上强制转换，否则数据不对
                        int ushort_temp = (ushort)(temp1 << 8) | temp2;//位操作最好加上强制转换，否则数据不对
                        Point point1 = new Point(ii, ushort_temp);
                        // 追加至Plot
                        dataSource1.AppendAsync(base.Dispatcher, point1);
                        //i++;
                        //方便显示在text中
                        buf_temp[2 * ii] = temp1;
                        buf_temp[2 * ii + 1] = temp2;
                    }//for

                    if (IsSerialDataDisplay==true)
                    {
                        string tempStr = " ";
                        interfaceUpdateHandle = new HandleInterfaceUpdateDelagate(UpdateTextBox);//实例化委托对象                 
                        Dispatcher.Invoke(interfaceUpdateHandle, new string[] { Encoding.ASCII.GetString(buf_temp) });
                    }

                }//if
                else { 
                   //未接受完的数据进行处理
                    Console.WriteLine("something needs to be handled");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("定时处理错误：" + ex.Message);
            }
        }

        private void UpdateTextBox(string text) 
        {
            TextBox_SerialReceive.Text += text;
        }

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
                Usart.ReadBufferSize = 4096*2;
                //准备就绪
                Usart.DtrEnable = true;//启用控制终端就续信号
                Usart.RtsEnable = true; //启用请求发送信号
                //设置数据读取超时为1秒
              // Usart.ReadTimeout = 1000;
               Usart.ReceivedBytesThreshold = 10;
               Usart.DataReceived += new SerialDataReceivedEventHandler(Usart_DataReceived);//DataReceived事件委托

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

        //串口事件处理函数
        private void Usart_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (Usart.IsOpen == false) return;
            try
            {
                //记录缓存数量
                int n = Usart.BytesToRead;
               // Console.WriteLine("n={0}", n);

                if (n == 0) return;

                //if (n > 500) 
                //{
                //声明一个临时数组来存储当前来的串口数据
                byte[] buf = new byte[n];

                //读取缓冲数据
                Usart.Read(buf, 0, n);

                //丢弃接受缓冲区数据
               // Usart.DiscardInBuffer();
                for (int i = 0; i < n; i++) {
                    Q_data.Enqueue(buf[i]);//队列在多线程时偶尔会出现“源数组长度不足，请检查Srcindex和长度以及数组的下限”错误，这里没有处理
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(Usart.BytesToRead.ToString() + "接收串口信息错误：" + ex.Message);
            }
            //throw new NotImplementedException();
        }

        //串口号选择下拉框
        private void ComboBox_SerialName_DropDownOpened(object sender, EventArgs e)
        {
            string[] names = SerialPort.GetPortNames();
            ComboBox_SerialName.Items.Clear();
            for (int i = 0; i < names.Length; i++)
            {
                ComboBox_SerialName.Items.Add(names[i]);
            }
        }

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

        private void TextBox_SerialReceive_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            TextBox_SerialReceive.SelectionStart = TextBox_SerialReceive.Text.Length;
            TextBox_SerialReceive.ScrollToEnd();
            //TextBox_SerialReceive.ScrollToCaret();
        }

        private void Button_SerialClear_Click(object sender, RoutedEventArgs e)
        {
            TextBox_SerialReceive.Clear();
        }

        private void CheckBox_IsSerialDataDis_Checked(object sender, RoutedEventArgs e)
        {
            if (CheckBox_IsSerialDataDis.IsChecked == true)
            {
                IsSerialDataDisplay = true;
            }
            else if(CheckBox_IsSerialDataDis.IsChecked == false){
                IsSerialDataDisplay = false;
            }
        }

        //public ushort ReverseUInt16(ushort value)
        //{
        //    return (ushort)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);
        //}
    }
}
