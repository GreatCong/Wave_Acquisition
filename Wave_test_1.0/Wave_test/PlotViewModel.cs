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


namespace Wave_test
{
    class PlotViewModel
    {
        public bool isStop = true;
        public Queue Q_data = new Queue();
        private int dataNum_display = 1024;//默认显示的点数是1024
        private int dataChannel_display = 1;//默认显示的通道是1
        private int[] dataInterval_display = new int[3]{1000,0,-1000};//默认显示的通道间隔，保证线条不重叠

        public PlotModel SimplePlotModel { get; set; }

        public PlotViewModel()
        {
            SimplePlotModel = new PlotModel();

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
            LinearAxis leftAxis = new LinearAxis()
            {
                Position = AxisPosition.Left,
                //Minimum = 0,
                //Maximum = 10,
                Title = "Y",//显示标题内容
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
            LinearAxis bottomAxis = new LinearAxis()
            {
                Position = AxisPosition.Bottom,
                //Minimum = 0,
                //Maximum = 10,
                Title = "X",//显示标题内容
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
                int ushort_temp1 = 0;
                int ushort_temp2 = 0;
                int ushort_temp3 = 0;

                while (true)
                {
                    if (!isStop)
                    {
                        Console.WriteLine("Q_data.Count={0}", Q_data.Count);
                        int dataWill_display = dataNum_display * 2 * dataChannel_display;
                        if (Q_data.Count > dataWill_display)
                        {
                            try
                            {
                                lineSerial1.Points.Clear();
                                lineSerial2.Points.Clear();
                                lineSerial3.Points.Clear();

                                for (int i = 0; i < dataNum_display; i++)
                                {
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

                                    //局部变量定义的太多,运行过程中会报错
                                    switch (dataChannel_display)
                                    { 
                                        case 1:
                                              ushort_temp1 = (ushort)((byte)Q_data.Dequeue() << 8) | (byte)Q_data.Dequeue();//位操作最好加上强制转换，否则数据不对
                                              lineSerial1.Points.Add(new DataPoint(i, ushort_temp1));//X通道
                                            break;
                                        case 2:
                                              ushort_temp1 = (ushort)((byte)Q_data.Dequeue() << 8) | (byte)Q_data.Dequeue();//位操作最好加上强制转换，否则数据不对
                                              ushort_temp2 = (ushort)((byte)Q_data.Dequeue() << 8) | (byte)Q_data.Dequeue();//位操作最好加上强制转换，否则数据不对
                                              lineSerial1.Points.Add(new DataPoint(i, ushort_temp1 + dataInterval_display[0]));//X通道
                                              lineSerial2.Points.Add(new DataPoint(i, ushort_temp2 + dataInterval_display[1]));//Y通道
                                            break;
                                        case 3:
                                              ushort_temp1 = (ushort)((byte)Q_data.Dequeue() << 8) | (byte)Q_data.Dequeue();//位操作最好加上强制转换，否则数据不对
                                              ushort_temp2 = (ushort)((byte)Q_data.Dequeue() << 8) | (byte)Q_data.Dequeue();//位操作最好加上强制转换，否则数据不对
                                              ushort_temp3 = (ushort)((byte)Q_data.Dequeue() << 8) | (byte)Q_data.Dequeue();//位操作最好加上强制转换，否则数据不对
                                              //var x = i;
                                              //var y1 = ushort_temp1 + 1000;
                                              //var y2 = ushort_temp2;
                                              //var y3 = ushort_temp3 - 1000;
                                              lineSerial1.Points.Add(new DataPoint(i, ushort_temp1 + dataInterval_display[0]));//X通道
                                              lineSerial2.Points.Add(new DataPoint(i, ushort_temp2 + dataInterval_display[1]));//Y通道
                                              lineSerial3.Points.Add(new DataPoint(i, ushort_temp3 + dataInterval_display[2]));//Z通道
                                            break;
                                        default:
                                              Console.WriteLine("1 channel default!");
                                              ushort_temp1 = (ushort)((byte)Q_data.Dequeue() << 8) | (byte)Q_data.Dequeue();//位操作最好加上强制转换，否则数据不对
                                              lineSerial1.Points.Add(new DataPoint(i, ushort_temp1 + dataInterval_display[1]));//X通道
                                            break;

                                    }
                                }
                                //刷新视图
                                SimplePlotModel.InvalidatePlot(true);
                            }
                            catch (Exception e)
                            {
                                System.Windows.MessageBox.Show("Data handle error"+e.ToString());
                            }
                        }
                    }
                    Thread.Sleep(50);//50ms刷新一次

                }
            });
            #endregion
        }
    }
}
