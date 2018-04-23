using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Wave_test
{
    public partial class MainWindow : Window {

        #region SmartSendData

        private void SendData(Socket sock, byte[] bytebuf)//TCP senddata
        {
            if ((sock == null) || (!sock.Connected))
            {
                MessageBox.Show("请先建立连接!");
                return;
            }
            try
            {
                sock.Send(bytebuf);
            }
            catch { }
        }

        private void SendData(Socket sock, EndPoint DstEndPoint, byte[] bytebuf) //UDP senddata
        {
            //if (!sock.Connected)
            //{
            //    MessageBox.Show("请先建立连接!");
            //    return;
            //}
            try
            {
                sock.SendTo(bytebuf, DstEndPoint);
            }
            catch (SocketException se)
            {
                MessageBox.Show("Socket Error!" + se.Message);
            }
            catch (ObjectDisposedException oe)
            {
                MessageBox.Show("Object Error!" + oe.Message);
            }
            catch { }
            //catch (SocketException se)
            //{
            //    MessageBox.Show("创建失败！" + se.Message);
            //    return;
            //}
        }

        private void SendData(SerialPort sp, byte[] bytebuf)//SerialPort senddata
        {
            if (!Usart.IsOpen) //如果没打开
            {
                MessageBox.Show("请先打开串口！", "Error");
                return;
            }
            Usart.DiscardOutBuffer();
            try
            {
                Usart.Write(bytebuf, 0, bytebuf.Length);
            }
            catch { }
        }

        private void SmartSend(byte[] bytebuf)
        {
            if (TabControl_TransportChoose.SelectedItem == USB_tabItem)//串口选项卡
            {
                SendData(Usart, bytebuf);
            }
            else if (TabControl_TransportChoose.SelectedItem == WiFi_tabItem)//网口选项卡
            {
                EndPoint remoteEP = new IPEndPoint(0, 0);

                switch (wifi_protocalType)
                {
                    case (int)NetworkProtocalType.TCP_Server:
                        if (netWork_connectionNum > 0) SendData(dictSock[0], bytebuf);
                        break;
                    case (int)NetworkProtocalType.TCP_Client:
                        SendData(socket_TCP_Client, bytebuf);
                        break;
                    case (int)NetworkProtocalType.UDP_Server:
                        remoteEP = EndpointParse(TextBox_wifi_endpoint.Text);
                        SendData(socket_UDP_Server, remoteEP, bytebuf);
                        break;
                    case (int)NetworkProtocalType.UDP_Client:
                        remoteEP = EndpointParse(TextBox_wifi_endpoint.Text);
                        SendData(socket_UDP_Client, remoteEP, bytebuf);
                        //udpClient.Send(bytebuf, bytebuf.Length, (IPEndPoint)remoteEP);
                        break;
                    default: break;
                }
            }
        }

        private bool SendByte(byte cmd)//调用SmartSend
        {
            byte[] data = new byte[1];
            data[0] = cmd;
            SmartSend(data);
            return true;
        }
        #endregion

        #region 配置信息
        // 
        // 目前保存的配置信息如下：
        // 1. 波特率
        // 2. 奇偶校验位
        // 3. 数据位
        // 4. 停止位
        // 5. 字节编码
        // 6. 发送区文本内容
        // 7. 自动发送时间间隔
        // 8. 窗口状态：最大化|高度+宽度
        // 9. 面板显示状态
        // 10. 接收数据模式
        // 11. 是否显示接收数据
        // 12. 发送数据模式
        // 13. 发送追加内容
        //

        /// <summary>
        /// 保存配置信息
        /// </summary>
        private void SaveConfig()
        {
            // 配置对象实例
            Configuration config = new Configuration();

            //// 保存波特率
            //AddBaudRate(config);

            //// 保存奇偶校验位
            //config.Add("parity", parityComboBox.SelectedIndex);

            //// 保存数据位
            //config.Add("dataBits", dataBitsComboBox.SelectedIndex);

            //// 保存停止位
            //config.Add("stopBits", stopBitsComboBox.SelectedIndex);

            //// 字节编码
            //config.Add("encoding", encodingComboBox.SelectedIndex);

            //// 保存发送区文本内容
            //config.Add("sendDataTextBoxText", sendDataTextBox.Text);

            //// 自动发送时间间隔
            //config.Add("autoSendDataInterval", autoSendIntervalTextBox.Text);
            //config.Add("timeUnit", timeUnitComboBox.SelectedIndex);

            // 窗口状态信息
            config.Add("maxmized", this.WindowState == WindowState.Maximized);
            config.Add("windowWidth", this.Width);
            config.Add("windowHeight", this.Height);
            config.Add("windowLeft", this.Left);
            config.Add("windowTop", this.Top);

            //// 面板显示状态
            //config.Add("serialPortConfigPanelVisible", serialPortConfigPanel.Visibility == Visibility.Visible);
            //config.Add("autoSendConfigPanelVisible", autoSendConfigPanel.Visibility == Visibility.Visible);
            //config.Add("serialCommunicationConfigPanelVisible", serialCommunicationConfigPanel.Visibility == Visibility.Visible);

            //// 保存接收模式
            //config.Add("receiveMode", receiveMode);
            //config.Add("showReceiveData", showReceiveData);

            //// 保存发送模式
            //config.Add("sendMode", sendMode);

            //// 保存发送追加
            //config.Add("appendContent", appendContent);

            //保存要显示的通道数
            config.Add("dataChannel_display", _viewModel.getChannel_num());
            //保存Network相关信息
            config.Add("netWork_endpointp_client", netWork_endpointp_client);//client的地址
            config.Add("netWork_port_server", netWork_port_server);//server的端口号

            // 保存配置信息到磁盘中
            Configuration.Save(config, @"Config\default.conf");
        }

        ///// <summary>
        ///// 将波特率列表添加进去
        ///// </summary>
        ///// <param name="conf"></param>
        //private void AddBaudRate(Configuration conf)
        //{
        //    conf.Add("baudRate", baudRateComboBox.Text);
        //}

        /// <summary>
        /// 加载配置信息
        /// </summary>
        private bool LoadConfig()
        {
            Configuration config = Configuration.Read(@"Config\default.conf");

            if (config == null)//如果没有配置信息，就直接返回
            {
                return false;
            }

            //// 获取波特率
            //string baudRateStr = config.GetString("baudRate");
            //baudRateComboBox.Text = baudRateStr;

            //// 获取奇偶校验位
            //int parityIndex = config.GetInt("parity");
            //parityComboBox.SelectedIndex = parityIndex;

            //// 获取数据位
            //int dataBitsIndex = config.GetInt("dataBits");
            //dataBitsComboBox.SelectedIndex = dataBitsIndex;

            //// 获取停止位
            //int stopBitsIndex = config.GetInt("stopBits");
            //stopBitsComboBox.SelectedIndex = stopBitsIndex;

            //// 获取编码
            //int encodingIndex = config.GetInt("encoding");
            //encodingComboBox.SelectedIndex = encodingIndex;

            //// 获取发送区内容
            //string sendDataText = config.GetString("sendDataTextBoxText");
            //sendDataTextBox.Text = sendDataText;

            // 获取自动发送数据时间间隔
            //string interval = config.GetString("autoSendDataInterval");
            //int timeUnitIndex = config.GetInt("timeUnit");
            //autoSendIntervalTextBox.Text = interval;
            //timeUnitComboBox.SelectedIndex = timeUnitIndex;

            // 窗口状态
            if (config.GetBool("maxmized"))
            {
                this.WindowState = WindowState.Maximized;
            }
            double width = config.GetDouble("windowWidth");
            double height = config.GetDouble("windowHeight");
            double top = config.GetDouble("windowTop");
            double left = config.GetDouble("windowLeft");
            this.Width = width;
            this.Height = height;
            this.Top = top;
            this.Left = left;

            //// 面板显示状态
            //if (config.GetBool("serialPortConfigPanelVisible"))
            //{
            //    serialSettingViewMenuItem.IsChecked = true;
            //    serialPortConfigPanel.Visibility = Visibility.Visible;
            //}
            //else
            //{
            //    serialSettingViewMenuItem.IsChecked = false;
            //    serialPortConfigPanel.Visibility = Visibility.Collapsed;
            //}

            //if (config.GetBool("autoSendConfigPanelVisible"))
            //{
            //    autoSendDataSettingViewMenuItem.IsChecked = true;
            //    autoSendConfigPanel.Visibility = Visibility.Visible;
            //}
            //else
            //{
            //    autoSendDataSettingViewMenuItem.IsChecked = false;
            //    autoSendConfigPanel.Visibility = Visibility.Collapsed;
            //}

            //if (config.GetBool("serialCommunicationConfigPanelVisible"))
            //{
            //    serialCommunicationSettingViewMenuItem.IsChecked = true;
            //    serialCommunicationConfigPanel.Visibility = Visibility.Visible;
            //}
            //else
            //{
            //    serialCommunicationSettingViewMenuItem.IsChecked = false;
            //    serialCommunicationConfigPanel.Visibility = Visibility.Collapsed;
            //}

            //// 加载接收模式
            //receiveMode = (ReceiveMode)config.GetInt("receiveMode");

            //switch (receiveMode)
            //{
            //    case ReceiveMode.Character:
            //        recvCharacterRadioButton.IsChecked = true;
            //        break;
            //    case ReceiveMode.Hex:
            //        recvHexRadioButton.IsChecked = true;
            //        break;
            //    case ReceiveMode.Decimal:
            //        recvDecRadioButton.IsChecked = true;
            //        break;
            //    case ReceiveMode.Octal:
            //        recvOctRadioButton.IsChecked = true;
            //        break;
            //    case ReceiveMode.Binary:
            //        recvBinRadioButton.IsChecked = true;
            //        break;
            //    default:
            //        break;
            //}

            //showReceiveData = config.GetBool("showReceiveData");
            //showRecvDataCheckBox.IsChecked = showReceiveData;

            //// 加载发送模式
            //sendMode = (SendMode)config.GetInt("sendMode");

            //switch (sendMode)
            //{
            //    case SendMode.Character:
            //        sendCharacterRadioButton.IsChecked = true;
            //        break;
            //    case SendMode.Hex:
            //        sendHexRadioButton.IsChecked = true;
            //        break;
            //    default:
            //        break;
            //}

            ////加载追加内容
            //appendContent = config.GetString("appendContent");

            //switch (appendContent)
            //{
            //    case "":
            //        appendNoneRadioButton.IsChecked = true;
            //        break;
            //    case "\r":
            //        appendReturnRadioButton.IsChecked = true;
            //        break;
            //    case "\n":
            //        appednNewLineRadioButton.IsChecked = true;
            //        break;
            //    case "\r\n":
            //        appendReturnNewLineRadioButton.IsChecked = true;
            //        break;
            //    default:
            //        break;
            //}

            //加载显示的通道数
            int dataChannel_display = config.GetInt("dataChannel_display");
            _viewModel.setChannel_num(dataChannel_display);
            ComboBox_PlotChannel_choose.SelectedIndex = dataChannel_display - 1;
            //加载Network相关信息
            netWork_endpointp_client = config.GetString("netWork_endpointp_client");
            netWork_port_server = config.GetString("netWork_port_server");
            TextBox_wifi_endpoint.Text = netWork_endpointp_client;

            return true;
        }
        #endregion

        #region 状态栏
        /// <summary>
        /// 更新时间信息
        /// </summary>
        private void UpdateTimeDate()
        {
            string timeDateString = "";
            DateTime now = DateTime.Now;
            timeDateString = string.Format("{0}年{1}月{2}日 {3}:{4}:{5}",
                now.Year,
                now.Month.ToString("00"),
                now.Day.ToString("00"),
                now.Hour.ToString("00"),
                now.Minute.ToString("00"),
                now.Second.ToString("00"));

            timeDateTextBlock.Text = timeDateString;
        }

        /// <summary>
        /// 警告信息提示（一直提示）
        /// </summary>
        /// <param name="message">提示信息</param>
        private void Alert(string message)
        {
            // #FF68217A
            StatusBar_state.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x21, 0x2A));
            statusInfoTextBlock.Text = message;
        }

        /// <summary>
        /// 普通状态信息提示
        /// </summary>
        /// <param name="message">提示信息</param>
        private void Information(string message, bool isOpen)//isOpen=true color=orange
        {
            //if (Usart.IsOpen)
            if (isOpen)
            {
                // #FFCA5100
                StatusBar_state.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xCA, 0x51, 0x00));
            }
            else
            {
                // #FF007ACC
                StatusBar_state.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x7A, 0xCC));
            }
            statusInfoTextBlock.Text = message;
        }

        //只显示文本的变化
        private void Information(string message)
        {           
            statusInfoTextBlock.Text = message;
        }
        #endregion
    }
}
