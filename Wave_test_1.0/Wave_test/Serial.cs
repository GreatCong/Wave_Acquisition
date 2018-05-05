using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using System.Management; //注意要在引用中加入system.Management
using System.Windows.Controls;

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
                string item_s = ComboBox_SerialName.SelectionBoxItem.ToString();
                string[] item_sArray = item_s.Split(':', '-');//'-'是为了分割虚拟串口软件虚拟出来的串口号
                item_s = item_sArray[0];
                //string[] item_sArray = item_s.Split('M',')');
                ////Console.WriteLine(sArray.Length.ToString());
                //item_s = "COM" + item_sArray[item_sArray.Length - 2].ToString();//因为‘)’截取后面还有一个""，所以要减去2
                //Console.WriteLine(item_sArray[item_sArray.Length - 2].ToString());//因为‘)’截取后面还有一个""，所以要减去2
                //foreach (string i in sArray) Console.WriteLine(i.ToString());
                //return;
                //Usart.PortName = ComboBox_SerialName.SelectionBoxItem.ToString();
                Usart.PortName = item_s;
                if (Usart.IsOpen == false)
                {
                    try{
                        Usart.Open();
                        Information(item_s+" 已经打开！", Usart.IsOpen);                
                        //关闭串口选择功能
                         ComboBox_SerialName.IsEnabled = false;

                         Button_SerialOpen.Content = "Close";
                    } catch (System.Exception ex) {
                        MessageBox.Show(ex.Message, "Usart.Open Error");
                    }
                                       
                }

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

        //下拉串口选择框 DropDownOpened
        private void ComboBox_SerialName_DropDownOpened(object sender, EventArgs e)
        {
            ////通过WMI获取COM端口
            string[] names = MulGetHardwareInfo(HardwareEnum.Win32_PnPEntity, "Name");
            //string[] names = SerialPort.GetPortNames();
            ComboBox_SerialName.Items.Clear();
            for (int i = 0; i < names.Length; i++)
            {
                string[] sArray = names[i].Split('(',')');
                names[i] = sArray[1] + ':'+ sArray[0];//把com号放在前面
                ComboBox_SerialName.Items.Add(names[i]);
            }
        }

        //下拉串口选择框 DropDownClosed
        private void ComboBox_SerialName_DropDownClosed(object sender, EventArgs e)
        {
            ToolTip xx = new ToolTip();//设置悬浮框
            xx.IsOpen = true;//open
            //xx.StaysOpen = true;
            ComboBox_SerialName.ToolTip = xx;
            xx.Content = ComboBox_SerialName.SelectionBoxItem.Equals("") ? "NULL" : ComboBox_SerialName.SelectionBoxItem.ToString();
            xx.IsOpen = false;//避免与下次的重复
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

        #region Hardware 枚举串口
        /// <summary>
        /// 枚举win32 api
        /// </summary>
        public enum HardwareEnum
        {
            // 硬件
            Win32_Processor, // CPU 处理器
            Win32_PhysicalMemory, // 物理内存条
            Win32_Keyboard, // 键盘
            Win32_PointingDevice, // 点输入设备，包括鼠标。
            Win32_FloppyDrive, // 软盘驱动器
            Win32_DiskDrive, // 硬盘驱动器
            Win32_CDROMDrive, // 光盘驱动器
            Win32_BaseBoard, // 主板
            Win32_BIOS, // BIOS 芯片
            Win32_ParallelPort, // 并口
            Win32_SerialPort, // 串口
            Win32_SerialPortConfiguration, // 串口配置
            Win32_SoundDevice, // 多媒体设置，一般指声卡。
            Win32_SystemSlot, // 主板插槽 (ISA & PCI & AGP)
            Win32_USBController, // USB 控制器
            Win32_NetworkAdapter, // 网络适配器
            Win32_NetworkAdapterConfiguration, // 网络适配器设置
            Win32_Printer, // 打印机
            Win32_PrinterConfiguration, // 打印机设置
            Win32_PrintJob, // 打印机任务
            Win32_TCPIPPrinterPort, // 打印机端口
            Win32_POTSModem, // MODEM
            Win32_POTSModemToSerialPort, // MODEM 端口
            Win32_DesktopMonitor, // 显示器
            Win32_DisplayConfiguration, // 显卡
            Win32_DisplayControllerConfiguration, // 显卡设置
            Win32_VideoController, // 显卡细节。
            Win32_VideoSettings, // 显卡支持的显示模式。

            // 操作系统
            Win32_TimeZone, // 时区
            Win32_SystemDriver, // 驱动程序
            Win32_DiskPartition, // 磁盘分区
            Win32_LogicalDisk, // 逻辑磁盘
            Win32_LogicalDiskToPartition, // 逻辑磁盘所在分区及始末位置。
            Win32_LogicalMemoryConfiguration, // 逻辑内存配置
            Win32_PageFile, // 系统页文件信息
            Win32_PageFileSetting, // 页文件设置
            Win32_BootConfiguration, // 系统启动配置
            Win32_ComputerSystem, // 计算机信息简要
            Win32_OperatingSystem, // 操作系统信息
            Win32_StartupCommand, // 系统自动启动程序
            Win32_Service, // 系统安装的服务
            Win32_Group, // 系统管理组
            Win32_GroupUser, // 系统组帐号
            Win32_UserAccount, // 用户帐号
            Win32_Process, // 系统进程
            Win32_Thread, // 系统线程
            Win32_Share, // 共享
            Win32_NetworkClient, // 已安装的网络客户端
            Win32_NetworkProtocol, // 已安装的网络协议
            Win32_PnPEntity,//all device
        }
        /// <summary>
        /// WMI取硬件信息
        /// </summary>
        /// <param name="hardType"></param>
        /// <param name="propKey"></param>
        /// <returns></returns>
        public static string[] MulGetHardwareInfo(HardwareEnum hardType, string propKey)
        {
            List<string> strs = new List<string>();
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from " + hardType))
                {
                    var hardInfos = searcher.Get();
                    foreach (var hardInfo in hardInfos)
                    {
                        if (hardInfo.Properties[propKey].Value != null)
                        {
                            if (hardInfo.Properties[propKey].Value.ToString().Contains("COM"))
                            {
                                strs.Add(hardInfo.Properties[propKey].Value.ToString());
                            }
                        }

                    }
                    searcher.Dispose();
                }
                return strs.ToArray();
            }
            catch
            {
                return null;
            }
            finally
            { strs = null; }
        }
        #endregion
    }
}
