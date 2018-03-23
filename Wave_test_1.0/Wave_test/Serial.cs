using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Wave_test
{
    public partial class MainWindow : Window
    {
        #region Global
        private SerialPort Usart = new SerialPort();//申明串口
        #endregion

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
                    Information("串口已经打开！", Usart.IsOpen);
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
                Information("准备就绪！", Usart.IsOpen);
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
                    lock (_viewModel.Q_data.SyncRoot)
                    {
                        for (int i = 0; i < n; i++)
                        {
                            _viewModel.Q_data.Enqueue(buf[i]);//队列在多线程时偶尔会出现“源数组长度不足，请检查Srcindex和长度以及数组的下限”错误，这里没有处理
                        }
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
    }
}
