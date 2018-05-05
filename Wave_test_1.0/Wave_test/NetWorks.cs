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
using System.Windows.Input;

namespace Wave_test
{
    public partial class MainWindow : Window
    {
        #region Global
        //private string wifi_protocalType;//UDP Server | UDP Client | TCP Client | TCP Server
        private enum NetworkProtocalType
        {
            TCP_Client,
            TCP_Server,
            UDP_Client,
            UDP_Server
        };
        private int wifi_protocalType;
        private Socket socket_TCP_Server = null;
        private Socket socket_TCP_Client = null;
        private Socket socket_UDP_Server = null;
        private Socket socket_UDP_Client = null;
        private int netWork_length = -1;
        public int netWork_s1 = 0, netWork_s2 = 0;
        int netWork_pktNum = 0;
        long netWork_dataNum = 0;
        public List<byte> netWork_receivedBytes = new List<byte>();
        Thread thread_TCP_Listen = null;
        Thread threadServerRec = null;
        Thread thread_TCP_Client = null;
        Thread thread_UDP_Server = null;
        Thread thread_UDP_Client = null;
        private UInt16 netWork_connectionNum = 0;
        Dictionary<int, Socket> dictSock = new Dictionary<int, Socket>();
        Dictionary<int, EndPoint> dictEndPoint = new Dictionary<int, EndPoint>();

        string netWork_endpointp_client_Normal = "";//client的地址
        string netWork_port_server = "";//对于server而言，只需要端口号就可以了
        string netWork_endpointp_client_Remote = "";//远程的client地址
        #endregion

        #region NetWork Function
        private void netWork_init()//网络初始化
        {
            ComboxBox_Wifi_protocal.Items.Add("TCP Client");
            ComboxBox_Wifi_protocal.Items.Add("TCP Server");
            ComboxBox_Wifi_protocal.Items.Add("UDP Client");
            ComboxBox_Wifi_protocal.Items.Add("UDP Server");
            //默认显示的是TCP Client
            ComboxBox_Wifi_protocal.SelectedIndex = 0;// -1表示未选中
            //wifi_protocalType = "TCP Client";
            wifi_protocalType = (int)NetworkProtocalType.TCP_Client;

            netWork_endpointp_client_Remote = "114.215.189.49:6001";
            netWork_endpointp_client_Normal = "192.168.45.1:6001";
            netWork_port_server = "6001";
            TextBox_wifi_endpoint.Text = netWork_endpointp_client_Normal;//Profile.G_EndPoint;
        }

        private string GetInternalIP()//获取IP地址
        {
            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }
        private string GetInternalIP(string type)// "WLAN" "LAN"
        {
            string strIP = "";
            foreach (NetworkInterface netInt in NetworkInterface.GetAllNetworkInterfaces())
            {
                string netname = netInt.Name;
                string keywd1 = "VMware";
                string keywd2 = "Loopback";
                if ((netInt.OperationalStatus == OperationalStatus.Up) && (netname.IndexOf(keywd1) == (-1)) && (netname.IndexOf(keywd2) == (-1)))
                {
                    NetworkInterfaceType nettype = new NetworkInterfaceType();
                    if (type == "WLAN") nettype = NetworkInterfaceType.Wireless80211;
                    else if (type == "LAN") nettype = NetworkInterfaceType.Ethernet;
                    else return null;

                    if (netInt.NetworkInterfaceType == nettype)
                    {
                        foreach (UnicastIPAddressInformation ipIntProp in netInt.GetIPProperties().UnicastAddresses.ToArray<UnicastIPAddressInformation>())
                        {
                            if (ipIntProp.Address.AddressFamily == AddressFamily.InterNetwork)
                                return ipIntProp.Address.ToString();// ip
                        }
                    }
                }
            }
            return strIP;
        }
        //处理tcp接收的数据。

        IPEndPoint EndpointParse(string text)//验证输入的IP地址是否符合规范
        {
            IPEndPoint endPoint = new IPEndPoint(0, 0);
            string[] addr = text.Split(':');
            try
            {
                endPoint = new IPEndPoint(IPAddress.Parse(addr[0]), Convert.ToInt32(addr[1]));
            }
            catch
            {
                MessageBox.Show("输入的IP地址格式不对,示例 192.168.12.1:40000");
                return null;
            }
            return endPoint;
        }

        void NetworkConnect()//网络连接 此函数中开启监听回调线程
        {
            EndPoint serverEP;
            switch (wifi_protocalType)
            {
                case (int)NetworkProtocalType.TCP_Client:
                    socket_TCP_Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    serverEP = EndpointParse(TextBox_wifi_endpoint.Text);//解析端点地址

                    try
                    {
                        //statusTextBlock.Text = ("与服务器连接中...");
                        socket_TCP_Client.Connect(serverEP);
                        Information("连接成功！", true);
                        netWork_endpointp_client_Normal = TextBox_wifi_endpoint.Text;//保存当前的设置信息
                        //LocalNetworkInfo(socketClient, true);
                    }
                    catch (SocketException se)
                    {
                        MessageBox.Show("与服务器连接失败！" + se.Message);
                        return;
                    }

                    thread_TCP_Client = new Thread(NetWork_RecMsg);
                    thread_TCP_Client.IsBackground = true;//设置为后台线程，当主线程终止后，后台线程也终止了
                    thread_TCP_Client.Start(socket_TCP_Client);

                    Button_Wifi_connect.Content = "断开";
                    TextBox_wifi_endpoint.IsEnabled = false;
                    ComboxBox_Wifi_protocal.IsEnabled = false;
                    break;
                case (int)NetworkProtocalType.TCP_Server:
                    // 创建负责监听的套接字，注意其中的参数；
                    socket_TCP_Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    serverEP = EndpointParse(TextBox_wifi_endpoint.Text);
                    if (serverEP == null) return;
                    try
                    {
                        // 将负责监听的套接字绑定到唯一的ip和端口上；
                        socket_TCP_Server.Bind(serverEP);
                        Information("创建成功！", true);
                        netWork_port_server = TextBox_wifi_endpoint.Text;//保存当前的设置信息
                        string[] str = netWork_port_server.Split(new char[] { ':' });
                        netWork_port_server = str[1];
                    }
                    catch (SocketException se)
                    {
                        MessageBox.Show("创建服务器失败！" + se.Message);
                        return;
                    }
                    // 设置监听队列的长度；
                    socket_TCP_Server.Listen(10);
                    // 创建负责监听的线程；
                    thread_TCP_Listen = new Thread(TCPWatchConnecting);
                    thread_TCP_Listen.IsBackground = true;
                    thread_TCP_Listen.Start();

                    Button_Wifi_connect.Content = "断开";
                    TextBox_wifi_endpoint.IsEnabled = false;
                    ComboxBox_Wifi_protocal.IsEnabled = false;
                    break;
                case (int)NetworkProtocalType.UDP_Server:
                    socket_UDP_Server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    EndPoint localEP = EndpointParse(TextBox_wifi_endpoint.Text);
                    try
                    {
                        socket_UDP_Server.Bind(localEP);
                        Information("创建成功！", true);
                        netWork_port_server = TextBox_wifi_endpoint.Text;//保存当前的设置信息
                        string[] str = netWork_port_server.Split(new char[] { ':' });//拆解IP地址，只保留端口号
                        netWork_port_server = str[1];
                    }
                    catch (SocketException se)
                    {
                        MessageBox.Show("创建失败！" + se.Message);
                        return;
                    }

                    thread_UDP_Server = new Thread(NetWork_RecMsg);
                    thread_UDP_Server.IsBackground = true;
                    thread_UDP_Server.Start(null);

                    Button_Wifi_connect.Content = "断开";
                    TextBox_wifi_endpoint.IsEnabled = false;
                    ComboxBox_Wifi_protocal.IsEnabled = false;
                    break;
                case (int)NetworkProtocalType.UDP_Client:
                    socket_UDP_Client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    EndPoint rep = EndpointParse(TextBox_wifi_endpoint.Text);
                    try
                    {
                        socket_UDP_Client.SendTo(new byte[] { 0xaa }, rep);
                        Information("连接成功！", true);
                        netWork_endpointp_client_Normal = TextBox_wifi_endpoint.Text;//保存当前的设置信息
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
                    byte[] buf = new byte[50];

                    //while(socket_UDP_Client.ReceiveFrom(buf, ref rep)==0);
                    //txtEndPoint.Text = System.Text.Encoding.Default.GetString(buf);
                    //try
                    //{
                    //    socket_UDP_Client.Connect(EndpointParse(txtEndPoint.Text));
                    //}
                    //catch (SocketException se)
                    //{
                    //    MessageBox.Show("与服务器连接失败！" + se.Message);
                    //    return;
                    //}
                    thread_UDP_Client = new Thread(NetWork_RecMsg);
                    thread_UDP_Client.IsBackground = true;
                    thread_UDP_Client.Start(null);

                    Button_Wifi_connect.Content = "断开";
                    // TextBox_wifi_endpoint.IsEnabled = false;
                    ComboxBox_Wifi_protocal.IsEnabled = false;
                    break;
            }//switch

            //thread_DataProcessing = new Thread(DataProcessing);
            //thread_DataProcessing.Abort();
        }

        void NetworkDisconnect()//网络断开
        {
            switch (wifi_protocalType)
            {
                case (int)NetworkProtocalType.TCP_Server:
                    try
                    {
                        //if (thread_TCP_Listen != null) thread_TCP_Listen.Abort();
                        //if (threadServerRec != null) threadServerRec.Abort();
                        //socket_TCP_Server.Shutdown(SocketShutdown.Both);//禁用socket收发
                        socket_TCP_Server.Close();
                    }
                    catch (System.Exception ex)
                    {
                        //没有连上时，通过异常终止accept
                        MessageBox.Show(ex.Message, "TCP_Server.close Error");
                    }

                    if (thread_TCP_Listen != null) thread_TCP_Listen.Abort();
                    if (threadServerRec != null) threadServerRec.Abort();

                    netWork_connectionNum = 0;
                    Button_Wifi_connect.Content = "创建";
                    TextBox_wifi_endpoint.IsEnabled = true;
                    ComboxBox_Wifi_protocal.IsEnabled = true;
                    try
                    {
                        for (UInt16 i = 0; i < netWork_connectionNum; i++)
                        {
                            dictSock[i].Disconnect(true);
                            dictSock[i].Close();
                        }
                        dictSock.Clear();//清除所有的socket 发送函数默认的采用dictSock[0]


                        //socketWatch.Close();
                        //threadWatch.Abort();

                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show(ex.Message, "TCP_Server.close socket Error");
                    }
                    //dictConnectInfo.Clear();
                    //connectObjectComboBox.Items.Refresh();
                    //connectObjectComboBox.SelectedIndex = 0;
                    ////显示提示文字
                    //connectButton.Content = "监听";
                    break;
                case (int)NetworkProtocalType.TCP_Client:
                    try
                    {
                        thread_TCP_Client.Abort();
                        socket_TCP_Client.Shutdown(SocketShutdown.Both);
                        socket_TCP_Client.Close();
                    }
                    catch
                    {
                    }
                    Button_Wifi_connect.Content = "连接";
                    TextBox_wifi_endpoint.IsEnabled = true;
                    ComboxBox_Wifi_protocal.IsEnabled = true;
                    break;
                case (int)NetworkProtocalType.UDP_Server:
                    try
                    {
                        thread_UDP_Server.Abort();
                        socket_UDP_Server.Shutdown(SocketShutdown.Both);
                        socket_UDP_Server.Close();
                    }
                    catch
                    {
                    }
                    Button_Wifi_connect.Content = "创建";
                    TextBox_wifi_endpoint.IsEnabled = true;
                    ComboxBox_Wifi_protocal.IsEnabled = true;
                    break;
                case (int)NetworkProtocalType.UDP_Client:
                    try
                    {
                        thread_UDP_Client.Abort();
                        socket_UDP_Client.Shutdown(SocketShutdown.Both);
                        socket_UDP_Client.Close();
                    }
                    catch
                    { }
                    Button_Wifi_connect.Content = "创建";
                    TextBox_wifi_endpoint.IsEnabled = true;
                    ComboxBox_Wifi_protocal.IsEnabled = true;
                    break;
                default:
                    break;

            }//switch

            //thread_DataProcessing.Abort();
        }

        public void IP_Parse()//IP验证 没有用到
        {
            Regex regExp = new Regex(@"^(?<ip>(?:25[0-5]|2[0-4]\d|1\d{0,2}|[1-9]\d?)\.(?:(?:25[0-5]|2[0-4]\d|1\d{0,2}|\d{1,2})\.){2}(?:25[0-5]|2[0-4]\d|1\d{0,2}|\d{1,2}))(:(?<port>\d{0,4}))?$");
            string[] test = { "192.168.0.256", "192.168.0.255", "192.168.0.255:8080" };
            StringBuilder textBuilder = new StringBuilder();
            foreach (string t in test)
            {
                Match m = regExp.Match(t);
                if (!m.Success)
                {
                    MessageBox.Show("Test text is" + Environment.NewLine + t + Environment.NewLine + "Invalid for ip rule.");
                    continue;
                }
                textBuilder.Remove(0, textBuilder.Length);
                if (m.Groups["ip"].Success)
                {
                    textBuilder.AppendLine("ip:" + m.Groups["ip"].Value);
                }
                else
                {
                    textBuilder.AppendLine("ip not found!");
                }
                if (m.Groups["port"].Success)
                {
                    textBuilder.Append("port:" + m.Groups["port"].Value);
                }
                else
                {
                    textBuilder.AppendLine("port not found!");
                }
                MessageBox.Show(textBuilder.ToString());
            }
        }


        //[DllImport("kernel32")]//这个动态连接库里面包含了很多WindowsAPI函数
        //static extern uint GetTickCount();
        #endregion

        #region NetWork thread handler
        void NetWork_RecMsg(object sockConnectionparn)//接收处理线程
        {
            Socket sockConnection = sockConnectionparn as Socket;//as关键词就是类型转换，不能转换就返回NULL

            byte[] buffMsgRec = new byte[6000];//设置缓冲数组的大小为6000
            int tp = 0;
            netWork_s1 = System.Environment.TickCount;

            //if (cbRX.Checked == false)
            while (true)
            {
                try
                {
                    EndPoint REP;//UDP传输会用
                    switch (wifi_protocalType)
                    {
                        case (int)NetworkProtocalType.UDP_Server:
                            REP = EndpointParse(TextBox_wifi_endpoint.Text);
                            netWork_length = socket_UDP_Server.ReceiveFrom(buffMsgRec, ref REP);//ref关键字表示按参数引用传递
                            break;
                        case (int)NetworkProtocalType.UDP_Client:
                            REP = new IPEndPoint(IPAddress.Any, 0);
                            netWork_length = socket_UDP_Client.ReceiveFrom(buffMsgRec, ref REP);
                            TextBox_SerialReceive.Text += REP.ToString() + "\r\n";//显示到文本框中
                            break;
                        case (int)NetworkProtocalType.TCP_Server:
                            netWork_length = sockConnection.Receive(buffMsgRec);

                            netWork_s2 = System.Environment.TickCount;
                            tp = netWork_s2 - netWork_s1;
                            //下面的应该是显示一些统计信息
                            textBox_netWorkTbPeriod_dis.Value_textDisplay = tp.ToString();

                            netWork_pktNum = netWork_length;
                            textBox_netWorkPkgNum_dis.Value_textDisplay = netWork_pktNum.ToString();


                            netWork_dataNum = netWork_dataNum + netWork_length;
                            textbox_netWorkDataNum_dis.Value_textDisplay = netWork_dataNum.ToString();

                            if (isSerialDataDispaly)//checked will be dispalyed
                            {
                                string str = Encoding.ASCII.GetString(buffMsgRec, 0, netWork_length);
                                textbox_receive_dis.Value_textDisplay += str;
                            }

                            break;
                        case (int)NetworkProtocalType.TCP_Client:
                            netWork_length = sockConnection.Receive(buffMsgRec);
                            //Console.WriteLine(netWork_length);

                            //netWork_s2 = GetTickCount();
                            netWork_s2 = System.Environment.TickCount;
                            tp = netWork_s2 - netWork_s1;
                            //下面的应该是显示一些统计信息
                            textBox_netWorkTbPeriod_dis.Value_textDisplay = tp.ToString();

                            netWork_pktNum = netWork_length;
                            textBox_netWorkPkgNum_dis.Value_textDisplay = netWork_pktNum.ToString();


                            netWork_dataNum = netWork_dataNum + netWork_length;
                            textbox_netWorkDataNum_dis.Value_textDisplay = netWork_dataNum.ToString();

                            //long error = tp * 60 - dataNum;
                            //tbError.Text = error.ToString();
                            //string str = Encoding.ASCII.GetString(buffMsgRec);
                            if (isSerialDataDispaly)//checked will be dispalyed
                            {
                                string str = Encoding.ASCII.GetString(buffMsgRec, 0, netWork_length);
                                textbox_receive_dis.Value_textDisplay += str;
                            }

                            break;
                        default:
                            break;
                    }//switch

                }
                catch (SocketException se)
                {
                    MessageBox.Show(se.Message, "Error1 SocketException;");
                    return;
                }
                catch (ThreadAbortException te)
                {
                    MessageBox.Show(te.Message, "Error2 ThreadAbortException;");
                    return;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Error3;");
                    return;
                }

                byte[] byteReadtemp = new byte[netWork_length];
                System.Buffer.BlockCopy(buffMsgRec, 0, byteReadtemp, 0, netWork_length);//将接收到的数据转移到新的数组中
                netWork_receivedBytes.AddRange(byteReadtemp);

                //plot
                //网络接收的数据与USB接收的数据是反的，发送0x258,接收0x5802
                lock (_viewModel.Q_data.SyncRoot)
                {
                    for (int i = 0; i < netWork_length; i++)
                    {
                        _viewModel.Q_data.Enqueue(byteReadtemp[i]);//队列在多线程时偶尔会出现“源数组长度不足，请检查Srcindex和长度以及数组的下限”错误，这里没有处理
                    }
                }
                //{
                //    int receiveNum = sample_points * bytes_per_point * CH_num;//bytes per received packge
                //    if (receivedBytes.Count() > receiveNum)
                //    {
                //        save_enable = false;
                //        //bytes_ready = false;
                //        byte[] byteRead = new byte[receiveNum];
                //        for (int i = 0; i < receiveNum; i++)
                //        {
                //            byteRead[i] = receivedBytes[i];
                //        }
                //        receivedBytes.RemoveRange(0, receiveNum);
                //        //receivedBytes.Clear();
                //        x1.Clear(); y1.Clear();
                //        x2.Clear(); y2.Clear();
                //        x3.Clear(); y3.Clear();
                //        x4.Clear(); y4.Clear();


                //        short[] shortbuf = new short[sample_points * CH_num];
                //        for (int i = 0; i < sample_points * CH_num; i++)
                //        {
                //            shortbuf[i] = BitConverter.ToInt16(byteRead, bytes_per_point * i);
                //        }

                //        for (int i = 0; i < sample_points; i++) //samples = points/4
                //        {
                //            { x1.Add(i); y1.Add(shortbuf[CH_num * i + 0]); }
                //            if (CH_num > 1) { x2.Add(i); y2.Add(shortbuf[CH_num * i + 1]); }
                //            if (CH_num > 2) { x3.Add(i); y3.Add(shortbuf[CH_num * i + 2]); }
                //            if (CH_num > 3) { x4.Add(i); y4.Add(shortbuf[CH_num * i + 3]); }
                //        }

                //        save_enable = true;

                //        zGraph1.f_Refresh();
                //    }
                //}

            }
        }
        void TCPWatchConnecting()//监听客户端的线程回调 在TCP Server线程中调用
        {
            while (true)
            {
                if (netWork_connectionNum == 0)//如果没有连接的设备
                {
                    try
                    {
                        // 开始监听客户端连接请求，Accept方法会阻断当前的线程
                        Socket sockConnection = socket_TCP_Server.Accept(); // 一旦监听到一个客户端的请求，就返回一个与该客户端通信的 套接字；
                        netWork_connectionNum++;
                        // 将与客户端连接的 套接字 对象添加到集合中
                        dictSock.Add(netWork_connectionNum - 1, sockConnection);
                        EndPoint remoteEndPoint = sockConnection.RemoteEndPoint;
                        //TextBox_SerialReceive.Text += remoteEndPoint + "";//是否显示到文本框中
                        threadServerRec = new Thread(NetWork_RecMsg);//开辟一个线程
                        threadServerRec.IsBackground = true;//该线程是否是一个后台线程
                        threadServerRec.Start(sockConnection);
                        //dictThread.Add(sokConnection.RemoteEndPoint.ToString(), thr);  //  将新建的线程 添加 到线程的集合中去。
                    }
                    catch (SocketException se)
                    {
                        MessageBox.Show("TCPWatchConnecting Socket Error;", se.Message);
                        //return;
                        break;
                    }
                    catch
                    {
                        MessageBox.Show("TCPWatchConnecting Error");
                        //return;
                        break;
                    }
                }
                Thread.Sleep(100);
            }
        }
        #endregion

        #region Network 消息
        //网络协议的下拉框的回调，选择协议
        private void ComboxBox_Wifi_protocal_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //string type = ComboxBox_Wifi_protocal.Text;//怎么显示的是上一次选择的值,用序号可以
            // Console.WriteLine(type);
            //Console.WriteLine(ComboxBox_Wifi_protocal.SelectedIndex);
            int type = ComboxBox_Wifi_protocal.SelectedIndex;
            try
            {
                switch (type)
                {
                    case (int)NetworkProtocalType.TCP_Client:
                        //        //显示提示文字
                        TextBox_wifi_endpoint.Text = netWork_endpointp_client_Normal;
                        wifi_protocalType = (int)NetworkProtocalType.TCP_Client;
                        Button_Wifi_connect.Content = "连接";
                        break;
                    case (int)NetworkProtocalType.TCP_Server:
                        //        //显示提示文字
                        wifi_protocalType = (int)NetworkProtocalType.TCP_Server;
                        TextBox_wifi_endpoint.Text = GetInternalIP("WLAN") + ":"+netWork_port_server;//获取内网IP
                        Button_Wifi_connect.Content = "创建";
                        break;
                    case (int)NetworkProtocalType.UDP_Server:
                        wifi_protocalType = (int)NetworkProtocalType.UDP_Server;
                        TextBox_wifi_endpoint.Text = GetInternalIP("WLAN") + ":"+netWork_port_server;//获取内网IP
                        Button_Wifi_connect.Content = "创建";
                        break;
                    case (int)NetworkProtocalType.UDP_Client:
                        TextBox_wifi_endpoint.Text = netWork_endpointp_client_Normal;
                        wifi_protocalType = (int)NetworkProtocalType.UDP_Client;
                        Button_Wifi_connect.Content = "连接";
                        break;
                    default:
                        break;
                }//switch

            }
            catch { return; }
        }


        private void Button_Wifi_connect_Click(object sender, RoutedEventArgs e)
        {
            if (Button_Wifi_connect.Content.Equals("创建") || Button_Wifi_connect.Content.Equals("连接"))
            {
                //Information("正在连接...", true);
                NetworkConnect();
            }
            else//"断开"
            {
                NetworkDisconnect();
                Information("准备就绪", false);
            }
        }

        private void TextBox_wifi_endpoint_KeyDown(object sender, KeyEventArgs e)
        {
            //正则匹配
            string patten = "[0-9.:]|\b"; //“\b”：退格键
            Regex r = new Regex(patten);//Regex是表示正则表达式
            Match m = r.Match(e.Key.ToString());
            //下面的方法会出现问题，无法检测出中文的冒号
            if (e.Key == Key.RightShift || e.Key == Key.LeftShift)//忽略shift键
                return;
            if (e.Key == Key.OemPeriod)//忽略点
                return;
            if (m.Success)
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
                MessageBox.Show("输入格式 192.168.12.1:40000");
            }
        }

        /// <summary>
        /// 近远程端点选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox_EndPoint_Switch_Click(object sender, RoutedEventArgs e)
        {
            if (Label_ep_switch.Content.Equals("近程端点"))
            {
                TextBox_wifi_endpoint.Text = netWork_endpointp_client_Remote;
                Label_ep_switch.Content = "远程端点";
            }
            else if (Label_ep_switch.Content.Equals("远程端点"))
            {
                TextBox_wifi_endpoint.Text = netWork_endpointp_client_Normal;
                Label_ep_switch.Content = "近程端点";
            }
        }
        #endregion
    }
}
