using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Wave_test
{
    public partial class MainWindow : Window
    {
        #region global
        private string appendContent = "";//默认不追加
        #endregion

        #region Menu Fills handle
        /// <summary>
        /// menu 退出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveConfigMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveConfig();
            // 状态栏显示保存成功
            Information("配置信息保存成功。");
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void loadConfigMenuItem_Click(object sender, RoutedEventArgs e)
        {
            LoadConfig();
            // 状态栏显示加载成功
            Information("配置信息加载成功。");
        }
        #endregion

        #region Menu Tools handle
        //打开系统计算器
        private void openCalMenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo();
            System.Diagnostics.Process proc = new System.Diagnostics.Process();

            //设置外部程序名(记事本用 notepad.exe 计算器用 calc.exe) 
            info.FileName = "calc.exe";

            //设置外部程序的启动参数

            info.Arguments = "";

            //设置外部程序工作目录为c:\windows

            info.WorkingDirectory = "c:/windows/";

            try
            {
                // 
                //启动外部程序 
                // 
                proc = System.Diagnostics.Process.Start(info);
            }
            catch
            {
                MessageBox.Show("系统找不到指定的程序文件", "错误提示！");
                return;
            }
        }

        //打开Putty软件
        private void openPuttyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo();
            System.Diagnostics.Process proc = new System.Diagnostics.Process();

            //设置外部程序名(记事本用 notepad.exe 计算器用 calc.exe) 
            info.FileName = "putty.exe";

            //设置外部程序的启动参数

            info.Arguments = "";

            //设置外部程序工作目录为c:\windows

            info.WorkingDirectory = @"software\";

            try
            {
                // 
                //启动外部程序 
                // 
                proc = System.Diagnostics.Process.Start(info);
            }
            catch
            {
                MessageBox.Show("系统找不到指定的程序文件", "错误提示！");
                return;
            }
        }
        #endregion

        #region Menu Setting handle
        private void appendRadioButton_Click(object sender, RoutedEventArgs e)//目前没加入发送函数中，有些问题
        {
            RadioButton rb = sender as RadioButton;
            if (rb != null)
            {
                switch (rb.Tag.ToString())
                {
                    case "none":
                        appendContent = "";
                        break;
                    case "return":
                        appendContent = "\r";
                        break;
                    case "newline":
                        appendContent = "\n";
                        break;
                    case "retnewline":
                        appendContent = "\r\n";
                        break;
                    default:
                        break;
                }
                Information("发送追加：" + rb.Content.ToString());
            }
        }
        #endregion
    }
}
