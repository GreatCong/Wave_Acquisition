using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using System.Threading;
using System.Collections;

using System.ComponentModel;

/*
 * 2017-12-20-by lcj
 */
namespace Wave_test
{
    //plot model
    class PlotViewModel
    {
        #region  variables

        #region public
        public bool isStop = true;
        public Queue Q_data = new Queue();
        public Data_XYZ data_x;
        public Data_XYZ data_y;
        public Data_XYZ data_z;
        //默认全部选择
        public bool is_dataCH1_Display = true;
        public bool is_dataCH2_Display = true;
        public bool is_dataCH3_Display = true;

        public int Int_TabControl_TransportChoose_select = 0;//默认是USB选项卡

        public PlotModel SimplePlotModel { get; set; }
        #endregion

        #region private
        private int dataNum_display = 1024;//默认显示的点数是1024
        private int dataChannel_display = 1;//默认显示的通道是1
        private int[] dataInterval_display = new int[3]{1000,0,-1000};//默认显示的通道间隔，保证线条不重叠   
             
        //Y轴显示的值的系数
        private double  dataPlus_display_Y = 1;//乘系数
        private double dataDiv_display_Y = 1;//除系数

        private int plot_sleepTime = 50;//plot线程休眠时间

        private LinearAxis leftAxis;//定义Y轴
        private LinearAxis bottomAxis;//定义X轴

        #endregion

        #endregion

        #region public functions

        public void setChannel_num(int num)
        {
            dataChannel_display = num;
        }

        public int getChannel_num()
        {
            return dataChannel_display;
        }

        public void setPoint_num(int num)
        {
            dataNum_display = num;
        }

        public int getPoint_num()
        {
            return dataNum_display;
        }

        public void setDisplay_bias(int num,int values)
        {
            dataInterval_display[num] = values;
        }

        public int getDisplay_bias(int num)
        {
            return dataInterval_display[num];
        }

        public void setDisplay_plusY(int values)
        {
            dataPlus_display_Y = values;
        }

        public double getDisplay_plusY()
        {
            return dataPlus_display_Y;
        }

        public void setDisplay_divY(int values)
        {
            dataDiv_display_Y = values;
        }

        public double getDisplay_divY()
        {
            return dataDiv_display_Y;
        }

        public void setDisplay_Ytitle(string text)
        {
            leftAxis.Title = text;
            //刷新视图
            SimplePlotModel.InvalidatePlot(true);
        }

        public void setDisplay_plotTime(int t)
        {
            plot_sleepTime = t;
        }

        #endregion

        public PlotViewModel()
        {
            SimplePlotModel = new PlotModel();//定义新的model

            #region 定义线条
            var lineSerial1 = new LineSeries() { Title = "X轴" };
            var lineSerial2 = new LineSeries() { Title = "Y轴" };
            var lineSerial3 = new LineSeries() { Title = "Z轴" };
            //lineSerial1.Color = OxyColors.Red;
            //lineSerial.Points.Add(new DataPoint(0, 0));
            //lineSerial.Points.Add(new DataPoint(10, 10));
            SimplePlotModel.Series.Add(lineSerial1);
            SimplePlotModel.Series.Add(lineSerial2);
            SimplePlotModel.Series.Add(lineSerial3);
            #endregion

            #region 定义坐标轴
            //定义y轴
            leftAxis = new LinearAxis()
            {
                Position = AxisPosition.Left,
                //Minimum = 0,
                //Maximum = 10,
                Title = "mV",//显示标题内容
                //TitlePosition = 1,//显示标题位置  默认是0.5(居中)
                //TitleColor = OxyColor.Parse("#d3d3d3"),//显示标题位置 
                //IsZoomEnabled = false,//坐标轴缩放关闭
                //IsPanEnabled = false,//图表缩放功能关闭
                MajorGridlineStyle = LineStyle.Solid,//主刻度设置格网
                MajorGridlineColor = OxyColor.Parse("#7379a0"),
                MinorGridlineStyle = LineStyle.Dot,//子刻度设置格网样式
                MinorGridlineColor = OxyColor.Parse("#666b8d")
            };
            //定义x轴
            bottomAxis = new LinearAxis()
            {
                Position = AxisPosition.Bottom,
                //Minimum = 0,
                //Maximum = 10,
                Title = "N",//显示标题内容
                //TitlePosition = 1,//显示标题位置
                //TitleColor = OxyColor.Parse("#d3d3d3"),//显示标题位置
                //IsZoomEnabled = false,//坐标轴缩放关闭
                //IsPanEnabled = false,//图表缩放功能关闭
                MajorGridlineStyle = LineStyle.Solid,//主刻度设置格网
                MajorGridlineColor = OxyColor.Parse("#7379a0"),
                MinorGridlineStyle = LineStyle.Dot,//子刻度设置格网样式
                MinorGridlineColor = OxyColor.Parse("#666b8d")
            };

            SimplePlotModel.Axes.Add(leftAxis);
            SimplePlotModel.Axes.Add(bottomAxis);
            #endregion

            SimplePlotModel.LegendPlacement = LegendPlacement.Outside;//设置legend在外面

            #region 刷新处理
            //var rd = new Random();
            Task.Factory.StartNew(() =>
            {
                //int ushort_temp1 = 0;
                //int ushort_temp2 = 0;
                //int ushort_temp3 = 0;
                int[] ushort_temp = new int[3] { 0, 0, 0 };
                double[] data_sum = new double[3];
                double[] data_average = new double[3];
                int[] last_data_temp = new int[3];
                int[] max_data_temp = new int[3]; 

                while (true)
                {
                    if (!isStop)
                    {
                        Console.WriteLine("Q_data.Count={0}", Q_data.Count);
                        //channel显示
                        lineSerial1.IsVisible = is_dataCH1_Display;
                        lineSerial2.IsVisible = is_dataCH2_Display;
                        lineSerial3.IsVisible = is_dataCH3_Display;

                        int dataWill_display = dataNum_display * 2 * dataChannel_display;
                        if (Q_data.Count > dataWill_display)
                        {
                            try
                            {
                                lineSerial1.Points.Clear();
                                lineSerial2.Points.Clear();
                                lineSerial3.Points.Clear();

                                
                                    //var x = rd.NextDouble() * 1000 % 10;
                                    //var y = rd.NextDouble() * 50 % 9;
                                    //var x = i * 0.1;
                                    //var y = 3 * rd.NextDouble() * Math.Sin(i) + 5;
                                    //byte temp1 = (byte)Q_data.Dequeue();
                                    //byte temp2 = (byte)Q_data.Dequeue();
                                    //byte temp3 = (byte)Q_data.Dequeue();
                                    //byte temp4 = (byte)Q_data.Dequeue();
                                    //byte temp5 = (byte)Q_data.Dequeue();
                                    //byte temp6 = (byte)Q_data.Dequeue();
                                    ////int ushort_temp = (ushort)(buf_temp[2 * ii] << 8) | buf_temp[2 * ii + 1];//位操作最好加上强制转换，否则数据不对
                                    //int ushort_temp1 = (ushort)(temp1 << 8) | temp2;//位操作最好加上强制转换，否则数据不对
                                    //int ushort_temp2 = (ushort)(temp3 << 8) | temp4;//位操作最好加上强制转换，否则数据不对

                                    for (int i = 0; i < 3; i++)//init data
                                    {
                                        data_sum[i] = 0;
                                        data_average[i] = 0;
                                        last_data_temp[i] = 0;
                                        max_data_temp[i] = 0;
                                    }
                                    #region Handles by Chnnels
                                    //局部变量定义的太多,运行过程中会报错
                                    switch (dataChannel_display)
                                    { 
                                        case 1:
                                            lock (Q_data.SyncRoot)
                                            {
                                                for (int i = 0; i < dataNum_display; i++)
                                                {
                                                    if (Int_TabControl_TransportChoose_select == 0)
                                                    {
                                                    //ushort_temp[0] = (ushort)((byte)Q_data.Dequeue() << 8) | (byte)Q_data.Dequeue();//位操作最好加上强制转换，否则数据不对
                                                    //移位操作会优先考虑转换为无符号数据
                                                    ushort_temp[0] = (short)(((byte)Q_data.Dequeue() << 8) | (byte)Q_data.Dequeue());//位操作最好加上强制转换，否则数据不对
                                                    }
                                                    else
                                                    {
                                                        //network与USB的解析方向相反
                                                        //ushort_temp[0] = (byte)Q_data.Dequeue() | (ushort)((byte)Q_data.Dequeue() << 8);//位操作最好加上强制转换，否则数据不对 
                                                        ushort_temp[0] = (short)((byte)Q_data.Dequeue() | ((byte)Q_data.Dequeue() << 8));//位操作最好加上强制转换，否则数据不对
                                                }

                                                //lineSerial1.Points.Add(new DataPoint(i, ushort_temp[0] + dataInterval_display[0]));//X通道
                                                lineSerial1.Points.Add(new DataPoint(i, (ushort_temp[0] + dataInterval_display[0]) * dataPlus_display_Y/ dataDiv_display_Y));//X通道

                                                    //handles
                                                    data_sum[0] += ushort_temp[0];
                                                    if (ushort_temp[0] > max_data_temp[0])//寻找最大值
                                                    {
                                                        max_data_temp[0] = ushort_temp[0];
                                                    }
                                                    last_data_temp[0] = ushort_temp[0];
                                                }
                                                data_average[0] = max_data_temp[0] - data_sum[0] / dataNum_display;
                                                //更改单位为mV
                                                data_average[0] = data_average[0] * 10000 / 32768;
                                                data_x.Data_value = "x:" + data_average[0].ToString();
                                                data_y.Data_value = "y:NULL";
                                                data_z.Data_value = "z:NULL";
                                            }
                                            break;
                                        case 2:
                                        lock (Q_data.SyncRoot)
                                        {
                                            for (int i = 0; i < dataNum_display; i++)
                                            {
                                                if (Int_TabControl_TransportChoose_select == 0)
                                                {
                                                    ushort_temp[0] = (short)(((byte)Q_data.Dequeue() << 8) | (byte)Q_data.Dequeue());//位操作最好加上强制转换，否则数据不对
                                                    ushort_temp[1] = (short)(((byte)Q_data.Dequeue() << 8) | (byte)Q_data.Dequeue());//位操作最好加上强制转换，否则数据不对
                                                                                                                                     //ushort_temp[0] = (ushort)((byte)Q_data.Dequeue() << 8) | (byte)Q_data.Dequeue();//位操作最好加上强制转换，否则数据不对
                                                                                                                                     //ushort_temp[1] = (ushort)((byte)Q_data.Dequeue() << 8) | (byte)Q_data.Dequeue();//位操作最好加上强制转换，否则数据不对
                                                }
                                                else
                                                {
                                                    //network与USB的解析方向相反
                                                    ushort_temp[0] = (short)((byte)Q_data.Dequeue() | ((byte)Q_data.Dequeue() << 8));//位操作最好加上强制转换，否则数据不对
                                                    ushort_temp[1] = (short)((byte)Q_data.Dequeue() | ((byte)Q_data.Dequeue() << 8));//位操作最好加上强制转换，否则数据不对
                                                                                                                                     //ushort_temp[0] = (byte)Q_data.Dequeue() | (ushort)((byte)Q_data.Dequeue() << 8);//位操作最好加上强制转换，否则数据不对
                                                                                                                                     //ushort_temp[1] = (byte)Q_data.Dequeue() | (ushort)((byte)Q_data.Dequeue() << 8);//位操作最好加上强制转换，否则数据不对
                                                }

                                                //lineSerial1.Points.Add(new DataPoint(i, ushort_temp[0] + dataInterval_display[0]));//X通道
                                                //lineSerial2.Points.Add(new DataPoint(i, ushort_temp[1] + dataInterval_display[1]));//Y通道
                                                lineSerial1.Points.Add(new DataPoint(i, (ushort_temp[0] + dataInterval_display[0]) * dataPlus_display_Y / dataDiv_display_Y));//X通道
                                                lineSerial2.Points.Add(new DataPoint(i, (ushort_temp[1] + dataInterval_display[1]) * dataPlus_display_Y / dataDiv_display_Y));//Y通道

                                                //handles
                                                data_sum[0] += ushort_temp[0];
                                                data_sum[1] += ushort_temp[1];
                                                if (ushort_temp[0] > max_data_temp[0])//寻找最大值
                                                {
                                                    max_data_temp[0] = ushort_temp[0];
                                                }
                                                if (ushort_temp[1] > max_data_temp[1])//寻找最大值
                                                {
                                                    max_data_temp[1] = ushort_temp[1];
                                                }
                                                last_data_temp[0] = ushort_temp[0];
                                                last_data_temp[1] = ushort_temp[1];
                                            }
                                            data_average[0] = max_data_temp[0] - data_sum[0] / dataNum_display;
                                            data_average[1] = max_data_temp[1] - data_sum[1] / dataNum_display;
                                            //更改单位为mV
                                            data_average[0] = data_average[0] * 10000 / 32768;
                                            data_average[1] = data_average[1] * 10000 / 32768;

                                            data_x.Data_value = "x:" + data_average[0].ToString();
                                            data_y.Data_value = "y:" + data_average[1].ToString();
                                            data_z.Data_value = "z:NULL";
                                        }
                                            break;
                                        case 3:
                                        lock (Q_data.SyncRoot)
                                        {
                                            for (int i = 0; i < dataNum_display; i++)
                                            {
                                                if (Int_TabControl_TransportChoose_select == 0)
                                                {
                                                    ushort_temp[0] = (short)(((byte)Q_data.Dequeue() << 8) | (byte)Q_data.Dequeue());//位操作最好加上强制转换，否则数据不对
                                                    ushort_temp[1] = (short)(((byte)Q_data.Dequeue() << 8) | (byte)Q_data.Dequeue());//位操作最好加上强制转换，否则数据不对
                                                    ushort_temp[2] = (short)(((byte)Q_data.Dequeue() << 8) | (byte)Q_data.Dequeue());//位操作最好加上强制转换，否则数据不对
                                                                                                                                     //ushort_temp[0] = (ushort)((byte)Q_data.Dequeue() << 8) | (byte)Q_data.Dequeue();//位操作最好加上强制转换，否则数据不对
                                                                                                                                     //ushort_temp[1] = (ushort)((byte)Q_data.Dequeue() << 8) | (byte)Q_data.Dequeue();//位操作最好加上强制转换，否则数据不对
                                                                                                                                     //ushort_temp[2] = (ushort)((byte)Q_data.Dequeue() << 8) | (byte)Q_data.Dequeue();//位操作最好加上强制转换，否则数据不对
                                                }
                                                else
                                                {
                                                    //network与USB的解析方向相反
                                                    ushort_temp[0] = (short)((byte)Q_data.Dequeue() | ((byte)Q_data.Dequeue() << 8));//位操作最好加上强制转换，否则数据不对
                                                    ushort_temp[1] = (short)((byte)Q_data.Dequeue() | ((byte)Q_data.Dequeue() << 8));//位操作最好加上强制转换，否则数据不对
                                                    ushort_temp[2] = (short)((byte)Q_data.Dequeue() | ((byte)Q_data.Dequeue() << 8));//位操作最好加上强制转换，否则数据不对
                                                                                                                                     //ushort_temp[0] = (byte)Q_data.Dequeue() | (ushort)((byte)Q_data.Dequeue() << 8);//位操作最好加上强制转换，否则数据不对
                                                                                                                                     //ushort_temp[1] = (byte)Q_data.Dequeue() | (ushort)((byte)Q_data.Dequeue() << 8);//位操作最好加上强制转换，否则数据不对
                                                                                                                                     //ushort_temp[2] = (byte)Q_data.Dequeue() | (ushort)((byte)Q_data.Dequeue() << 8);//位操作最好加上强制转换，否则数据不对
                                                }
                                                //var x = i;
                                                //var y1 = ushort_temp1 + 1000;
                                                //var y2 = ushort_temp2;
                                                //var y3 = ushort_temp3 - 1000;
                                                //lineSerial1.Points.Add(new DataPoint(i, ushort_temp[0] + dataInterval_display[0]));//X通道
                                                //lineSerial2.Points.Add(new DataPoint(i, ushort_temp[1] + dataInterval_display[1]));//Y通道
                                                //lineSerial3.Points.Add(new DataPoint(i, ushort_temp[2] + dataInterval_display[2]));//Z通道
                                                lineSerial1.Points.Add(new DataPoint(i, (ushort_temp[0] + dataInterval_display[0]) * dataPlus_display_Y / dataDiv_display_Y));//X通道
                                                lineSerial2.Points.Add(new DataPoint(i, (ushort_temp[1] + dataInterval_display[1]) * dataPlus_display_Y / dataDiv_display_Y));//Y通道
                                                lineSerial3.Points.Add(new DataPoint(i, (ushort_temp[2] + dataInterval_display[2]) * dataPlus_display_Y / dataDiv_display_Y));//Z通道

                                                //handles
                                                data_sum[0] += ushort_temp[0];
                                                data_sum[1] += ushort_temp[1];
                                                data_sum[2] += ushort_temp[2];
                                                if (ushort_temp[0] > max_data_temp[0])//寻找最大值
                                                {
                                                    max_data_temp[0] = ushort_temp[0];
                                                }
                                                if (ushort_temp[1] > max_data_temp[1])//寻找最大值
                                                {
                                                    max_data_temp[1] = ushort_temp[1];
                                                }
                                                if (ushort_temp[2] > max_data_temp[2])//寻找最大值
                                                {
                                                    max_data_temp[2] = ushort_temp[2];
                                                }
                                                last_data_temp[0] = ushort_temp[0];
                                                last_data_temp[1] = ushort_temp[1];
                                                last_data_temp[2] = ushort_temp[2];
                                            }
                                            data_average[0] = max_data_temp[0] - data_sum[0] / dataNum_display;
                                            data_average[1] = max_data_temp[1] - data_sum[1] / dataNum_display;
                                            data_average[2] = max_data_temp[2] - data_sum[2] / dataNum_display;

                                            data_x.Data_value = "x:" + data_average[0].ToString();
                                            data_y.Data_value = "y:" + data_average[1].ToString();
                                            data_z.Data_value = "z:" + data_average[2].ToString();
                                        }
                                            break;
                                        default:
                                              Console.WriteLine("1 channel default!");
                                              for (int i = 0; i < dataNum_display; i++)
                                              {
                                                  if (Int_TabControl_TransportChoose_select == 0)
                                                  {
                                                   ushort_temp[0] = (short)((byte)Q_data.Dequeue() | ((byte)Q_data.Dequeue() << 8));//位操作最好加上强制转换，否则数据不对
                                                //ushort_temp[0] = (ushort)((byte)Q_data.Dequeue() << 8) | (byte)Q_data.Dequeue();//位操作最好加上强制转换，否则数据不对
                                            }
                                                  else
                                                  {
                                                //network与USB的解析方向相反
                                                //ushort_temp[0] = (byte)Q_data.Dequeue() | (ushort)((byte)Q_data.Dequeue() << 8);//位操作最好加上强制转换，否则数据不对 
                                                ushort_temp[0] = (short)((byte)Q_data.Dequeue() | ((byte)Q_data.Dequeue() << 8));//位操作最好加上强制转换，否则数据不对
                                            }
                                                  lineSerial1.Points.Add(new DataPoint(i, ushort_temp[0] + dataInterval_display[1]));//X通道
                                              }
                                            break;

                                    }
                                    #endregion
                                    //刷新视图
                                SimplePlotModel.InvalidatePlot(true);

                                //data_xyz.Data_value = "x:"+ushort_temp1.ToString();

                            }
                            catch (Exception e)
                            {
                                System.Windows.MessageBox.Show("Data handle error"+e.ToString());
                            }
                        }
                    }
                    Thread.Sleep(plot_sleepTime);//50ms刷新一次

                }
            });
            #endregion
        }
    }

    #region Bind data class
    //与textBox控件绑定
    public class Data_XYZ : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string data_value;
        public string Data_value
        {
            get { return data_value; }
            set
            {
                data_value = value;

                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Data_value"));
                }
            }
        }
    }

    public class Data_textDisplay : INotifyPropertyChanged //用于其他的textBox数据绑定
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string value_textDisplay;
        public string Value_textDisplay
        {
            get { return value_textDisplay; }
            set
            {
                value_textDisplay = value;

                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Value_textDisplay"));//这里的string的值要与类名一致？？
                }
            }
        }
    }
    #endregion
}
