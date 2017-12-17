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
        private ObservableDataSource<Point> dataSource2 = new ObservableDataSource<Point>();
        private LineGraph graphSin1 = new LineGraph(); //线的注释类
        private LineGraph graphSin2 = new LineGraph();
        private DispatcherTimer timer = new DispatcherTimer();//timer
        private int i = 0;
        Header chartHeader = new Header();//列出标题
        TextBlock headerContents = new TextBlock();//标题内容类

        //声明串口
        private SerialPort Usart = new SerialPort();

        public MainWindow()
        {
            InitializeComponent();
            headerContents.Text = "This is a test";
            headerContents.HorizontalAlignment = HorizontalAlignment.Center;//居中显示
            chartHeader.Content = headerContents;
            plotter.Children.Add(chartHeader);//将标题内容写入
        }

        private void button_Start_Click(object sender, RoutedEventArgs e)
        {
            //plotter.AddLineGraph(dataSource, Colors.Green, 2); 
            timer.Interval = TimeSpan.FromSeconds(0.1);
            timer.Tick += new EventHandler(AnimatedPlot);//AnimatedPlot相当于是timer的回调
            timer.IsEnabled = true;//开启timer

            if (i == 0)
            {
                graphSin1 = plotter.AddLineGraph(dataSource1, Colors.Red, 2, "Sin1");
                graphSin2 = plotter.AddLineGraph(dataSource2, Colors.Black, 2, "Sin2");
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
            i = 0;
            timer.IsEnabled = false; 
            plotter.Children.Remove(graphSin1);
            plotter.Children.Remove(graphSin2);
            //需要注意的就是清除示波器数据时，除了要用plotter.Children.Remove（）指令，将此通道曲线移除，还要将数据源里数据移除。
            dataSource1 = new ObservableDataSource<Point>();
            dataSource2 = new ObservableDataSource<Point>();
        }
        int xaxis = 0;
        int group = 1024;
        //timer回调
        private void AnimatedPlot(object sender, EventArgs e)
        {
            double x = i;
            //double y1 = Math.Sin(i * 0.2);
            double y2 = 2 * Math.Sin(i * 0.6);

            plotter.Children.Remove(graphSin1);
            plotter.Children.Remove(graphSin2);
            //需要注意的就是清除示波器数据时，除了要用plotter.Children.Remove（）指令，将此通道曲线移除，还要将数据源里数据移除。
            dataSource1 = new ObservableDataSource<Point>();
            dataSource2 = new ObservableDataSource<Point>();
            graphSin1 = plotter.AddLineGraph(dataSource1, Colors.Red, 2, "Sin1");
            graphSin2 = plotter.AddLineGraph(dataSource2, Colors.Black, 2, "Sin2");

            for (int ii = 0; ii < 1024; ii++) {
                Point point1 = new Point(i, Math.Sin(i * 0.2));
                Point point2 = new Point(x+ii, y2);
                // 追加至Plot
                dataSource1.AppendAsync(base.Dispatcher, point1);
                dataSource2.AppendAsync(base.Dispatcher, point2);
                i++;
            }

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
                int n = Usart.BytesToRead;

                if (n == 0) return;

                //声明一个临时数组来存储当前来的串口数据
                byte[] buf = new byte[n];



                //读取缓冲数据
                Usart.Read(buf, 0, n);

                //丢弃接受缓冲区数据
                Usart.DiscardInBuffer();

                string tempStr = " ";

                    //将字节全部转换为字符串！
                    tempStr = (Encoding.ASCII.GetString(buf));

                    TextBox_SerialReceive.Text += tempStr;

            }
            catch (Exception ex)
            {
                MessageBox.Show(Usart.BytesToRead.ToString() + "接收串口信息错误：" + ex.Message);
            }
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
                Usart.ReadBufferSize = 4096;

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

                Button_SerialOpen.Content = "打开串口";
            }
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
    }
}
