using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace BRSReadout
{
    public partial class Form2 : Form
    {
        public static object graphLock = Form1.graphLock;
        public static Queue<Form1.graphData> graphQueue = Form1.graphQueue;
        public static AutoResetEvent graphSignal = Form1.graphSignal;
        public bool run=true;
        Thread dataLoopThread;
        public double dataPointNum;
        public double angleMax;
        public double angleMin=Math.Pow(10,100);
        double numberPoints = double.Parse(ConfigurationManager.AppSettings.Get("numberOfGraphPoints"));
        public static int camWidth = int.Parse(ConfigurationManager.AppSettings.Get("cameraWidth"));
        public static double bitDepth = double.Parse(ConfigurationManager.AppSettings.Get("cameraBitDepth"));
        static int graphingFrameNumber = int.Parse(ConfigurationManager.AppSettings.Get("graphingFrameNumber"));
        public int graphingFrameCount=0;

        public Form1.graphData inData = new Form1.graphData();

        public void stopLoop()
        {
            run = false;
        }
        public void dataLoop()
        {
            while (true)
            {
                graphSignal.WaitOne();
                do
                {
                    inData.frame = null;
                    lock (graphLock)
                    {
                        if (graphQueue.Count > 10)
                        {
                            inData = graphQueue.Dequeue();
                        }
                        if (inData.frame != null)
                        {
                            if (graphingFrameCount >= graphingFrameNumber)
                            {
                                updatePlot(inData.frame, inData.angle);
                                graphingFrameCount = 0;
                            }
                            else
                            {
                                graphingFrameCount++;
                            }
                        }
                    }
                }
                while (inData.frame != null);
            }
        }
        public Form2()
        {
            InitializeComponent();
            ushort[] rawFrame = Form1.frame;
            double[,] data = Form1.newdata;
            imagePlot.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Image",
                    Values = new ChartValues<int> (),
                    Fill=System.Windows.Media.Brushes.Transparent,
                    Stroke = System.Windows.Media.Brushes.Red,
                    LineSmoothness = 0,
                    PointGeometry = null,
                    PointGeometrySize = 0
                }
            };
            anglePlot.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Angle",
                    Values = new ChartValues<double>(),
                    Fill=System.Windows.Media.Brushes.Transparent,
                    LineSmoothness = 0,
                    PointGeometry = null,
                    PointGeometrySize = 0
                }
            };

            imagePlot.AxisX.Add(
            new Axis
            {
                Title = "Pixel Number",
                FontSize = 16,
                Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0)),
                MinValue = 0,
                MaxValue = camWidth,
                Separator = new Separator
                {
                    StrokeThickness = 0.5,
                    Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0)),
                    Step = Math.Pow(2, bitDepth) / 8.0
                }
            });
            imagePlot.AxisX.Add(
            new Axis
            {
                Labels =new string[camWidth],
                MinValue = 0,
                MaxValue = camWidth,
                Separator = new Separator
                {
                    StrokeThickness = 0.5,
                    Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(220, 220, 220)),
                    Step = Math.Pow(2, bitDepth) / 64.0
                }
            });

            imagePlot.AxisY.Add(
            new Axis
            {
                Title = "Intensity",
                FontSize = 16,
                Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0)),
                MinValue = 0,
                MaxValue = Math.Pow(2, bitDepth),
                Separator = new Separator
                {
                    StrokeThickness=0.5,
                    Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0)),
                    Step=  camWidth / 8.0
                }
            });
            imagePlot.AxisY.Add(
            new Axis
            {
                Labels = new string[camWidth],
                MinValue = 0,
                MaxValue = camWidth,
                Separator = new Separator
                {
                    StrokeThickness = 0.5,
                    Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(220, 220, 220)),
                    Step = camWidth / 64.0
                }
            });

            anglePlot.AxisX.Add(
            new Axis { 
                Title="Time (data points)",
                FontSize = 16,
                MinValue = 0,
                MaxValue = 10,
                Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0)),
                Separator = new Separator
                {
                    StrokeThickness=0.5,
                    Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0)),
                    Step=100.0
                }
            });
            anglePlot.AxisX.Add(
            new Axis
            {
                Labels = new string[1000],
                MinValue = 0,
                MaxValue = 10,
                Separator = new Separator
                {
                    StrokeThickness = 0.5,
                    Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(220, 220, 220)),
                    Step = 10.0
                }
            });

            anglePlot.AxisY.Add(
            new Axis
            {
                Title = "Angle (pixel number)",
                FontSize = 16,
                Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0)),
                Separator = new Separator
                {
                    StrokeThickness=0.5,
                    Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0))
                }
            });
            anglePlot.AxisY.Add(
            new Axis
            {
                Labels = new string[1000],
                MinValue = 0,
                MaxValue = 10,
                Separator = new Separator
                {
                    StrokeThickness = 0.5,
                    Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(220, 220, 220))
                }
            });


            imagePlot.DisableAnimations = true;
            imagePlot.Hoverable = false;
            imagePlot.DataTooltip = null;
            imagePlot.Zoom = ZoomingOptions.X;

            anglePlot.DisableAnimations = true;
            anglePlot.Hoverable = false;
            anglePlot.DataTooltip = null;
            anglePlot.Zoom = ZoomingOptions.X;

            imagePlot.Anchor = AnchorStyles.Left;
            anglePlot.Anchor = AnchorStyles.Right;
            imagePlot.Anchor = AnchorStyles.Top;
            anglePlot.Anchor = AnchorStyles.Top;


            this.Size = new System.Drawing.Size(1600, 800);
            imagePlot.Size = new Size((ClientRectangle.Width / 2) - 40, ClientRectangle.Height - 40);
            anglePlot.Size = new Size((ClientRectangle.Width / 2) - 40, ClientRectangle.Height - 40);
            imagePlot.Location = new Point(20, 20);
            anglePlot.Location = new Point((ClientRectangle.Width / 2) - 20, 20);

            if (dataLoopThread == null)
            {
                dataLoopThread = new Thread(dataLoop);
                dataLoopThread.Start();
            }
            else
            {
                dataLoopThread.Start();
            }
        }
        public static IEnumerable<T> ToEnumerable<T>(Array target)
        {
            foreach (var item in target)
            {
                yield return (T)item;
            }
        }
        public void updatePlot(ushort[] rawFrame, double data)
        {
            if (anglePlot.Series[0].Values.Count > numberPoints)
            {
                anglePlot.Series[0].Values.RemoveAt(0);
                anglePlot.Series[0].Values.Add(data);
            }
            else
            {
                anglePlot.Series[0].Values.Add(data);
                dataPointNum++;
            }
            imagePlot.Series[0].Values.Clear();
            var inFrame = Array.ConvertAll(rawFrame, item => (int)item);
            imagePlot.Series[0].Values.AddRange(ToEnumerable<object>(inFrame));

            if (data > angleMax)
            {
                angleMax = data;
            }
            if (data < angleMin)
            {
                angleMin = data;
            }
        }
        public void updateAxis()
        {
            if (dataPointNum >= 1)
            {
                anglePlot.AxisX[0].MaxValue = dataPointNum;
                anglePlot.AxisX[0].MinValue = 0;
                anglePlot.AxisX[1].MaxValue = dataPointNum;
                anglePlot.AxisX[1].MinValue = 0;
                anglePlot.AxisX[0].Separator.Step = dataPointNum / 10;
                anglePlot.AxisX[1].Separator.Step = dataPointNum / 100.0;

                double plotMax = Math.Round(angleMax)+1;
                double plotMin = Math.Round(angleMin)-1;
                anglePlot.AxisY[0].MaxValue = plotMax;
                anglePlot.AxisY[0].MinValue = plotMin;
                anglePlot.AxisY[1].MaxValue = plotMax;
                anglePlot.AxisY[1].MinValue = plotMin;
                anglePlot.AxisY[0].Separator.Step = Math.Round((plotMax-plotMin) / 10.0,1);
                anglePlot.AxisY[1].Separator.Step = (plotMax-plotMin) / 100.0;
            }
        }
        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            stopLoop();
            this.Hide();
            e.Cancel=true;
        }

    }
}
