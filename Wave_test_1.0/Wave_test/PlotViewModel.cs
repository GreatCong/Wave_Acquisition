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

        public PlotModel SimplePlotModel { get; set; }

        public PlotViewModel()
        {
            SimplePlotModel = new PlotModel();

            //线条
            var lineSerial = new LineSeries() { Title = "X轴" };
            //lineSerial.Points.Add(new DataPoint(0, 0));
            //lineSerial.Points.Add(new DataPoint(10, 10));
            SimplePlotModel.Series.Add(lineSerial);

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


            var rd = new Random();

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    if (!isStop)
                    {
                        Console.WriteLine("Q_data.Count={0}", Q_data.Count);
                        if (Q_data.Count > 2048)
                        {
                            lineSerial.Points.Clear();
                            for (int i = 0; i < 1024; i++)
                            {
                                //var x = rd.NextDouble() * 1000 % 10;
                                //var y = rd.NextDouble() * 50 % 9;
                                //var x = i * 0.1;
                                //var y = 3 * rd.NextDouble() * Math.Sin(i) + 5;
                                byte temp1 = (byte)Q_data.Dequeue();
                                byte temp2 = (byte)Q_data.Dequeue();
                                //int ushort_temp = (ushort)(buf_temp[2 * ii] << 8) | buf_temp[2 * ii + 1];//位操作最好加上强制转换，否则数据不对
                                int ushort_temp = (ushort)(temp1 << 8) | temp2;//位操作最好加上强制转换，否则数据不对
                                var x = i;
                                var y = ushort_temp;
                                lineSerial.Points.Add(new DataPoint(x, y));
                            }
                            //刷新视图
                            SimplePlotModel.InvalidatePlot(true);
                        }
                    }
                    Thread.Sleep(50);

                }
            });
        }
    }
}
