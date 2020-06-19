using ScottPlot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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

namespace AssettoCorsaTelemetryApp
{
	public struct TracePlot
	{
		public ScottPlot.WpfPlot plotFrame;
		public ScottPlot.PlottableSignal sigplot;
		public ScottPlot.PlottableSignal sigplotLastLap;
		public string name;
		public double[] yLims;
	}
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, IDisposable
	{
		const int sleepTime = 20;
		const int renderSleepTime = 500;
		const int plotSpanTime = 30_000;
		const int bufferSize = 500_000;
		double[] dataGas = new double[bufferSize];
		double[] dataGasLast = new double[bufferSize / 50];
		double[] dataBrake = new double[bufferSize];
		double[] dataBrakeLast = new double[bufferSize / 50];
		double[] dataGear = new double[bufferSize];
		double[] dataGearLast = new double[bufferSize / 50];
		double[] dataSteer = new double[bufferSize];
		double[] dataSteerLast = new double[bufferSize / 50];
		int index = 0;
		int[] dataLap = new int[bufferSize];
		List<int> beginLapIndices = new List<int>();
		int lastCompletedLap = 0;

		DispatcherTimer updateTimer;
		DispatcherTimer renderTimer;
		TracePlot[] plots;

		PhysicsData physData;
		GraphicsData graphicsData;

		bool started = false;

		public MainWindow()
		{
			InitializeComponent();

			plots = new TracePlot[4];
			plots[0] = new TracePlot()
			{
				plotFrame = plotFrameGas,
				sigplot = plotFrameGas.plt.PlotSignal(dataGas, color: System.Drawing.Color.FromArgb(255, 25, 150, 0), markerSize: 0),
				sigplotLastLap = plotFrameGas.plt.PlotSignal(dataGasLast, color: System.Drawing.Color.FromArgb(100, 25, 150, 0), markerSize: 0),
				name = "Gas",
				yLims = new double[] { -0.2, 1.2 }
			};
			plots[1] = new TracePlot()
			{
				plotFrame = plotFrameBrake,
				sigplot = plotFrameBrake.plt.PlotSignal(dataBrake, color: System.Drawing.Color.FromArgb(255, 200, 0, 0), markerSize: 0),
				sigplotLastLap = plotFrameBrake.plt.PlotSignal(dataBrakeLast, color: System.Drawing.Color.FromArgb(100, 200, 0, 0), markerSize: 0),
				name = "Brake",
				yLims = new double[] { -0.2, 1.2 }
			};
			plots[2] = new TracePlot()
			{
				plotFrame = plotFrameGear,
				sigplot = plotFrameGear.plt.PlotSignal(dataGear, color: System.Drawing.Color.Black, markerSize: 0),
				sigplotLastLap = plotFrameGear.plt.PlotSignal(dataGearLast, color: System.Drawing.Color.FromArgb(100, 0, 0, 0), markerSize: 0),
				name = "Gear",
				yLims = new double[] { -1.2, 10.2 } //This'll be fun if someone has a car with 17 gears
			};
			plots[3] = new TracePlot()
			{
				plotFrame = plotFrameSteer,
				sigplot = plotFrameSteer.plt.PlotSignal(dataSteer, color: System.Drawing.Color.FromArgb(255, 100, 0, 200), markerSize: 0),
				sigplotLastLap = plotFrameSteer.plt.PlotSignal(dataSteerLast, color: System.Drawing.Color.FromArgb(100, 100, 0, 200), markerSize: 0),
				name = "Steering Angle",
				yLims = new double[] { -1.2, 1.2 }
			};

			foreach (TracePlot curr in plots)
			{
				curr.plotFrame.plt.Title(curr.name);
				curr.plotFrame.plt.Ticks(displayTickLabelsX: false);
				curr.plotFrame.plt.Axis(0, 0, curr.yLims[0], curr.yLims[1]);
				curr.sigplotLastLap.visible = false;
			}

			string[] gearYTicks = Enumerable.Range(-1, 12).Select(i => "" + i).ToArray();
			gearYTicks[0] = "R";
			gearYTicks[1] = "N";
			plotFrameGear.plt.YTicks(Enumerable.Range(-1, 12).Select(i => (double)i).ToArray(), gearYTicks);

			physData = new PhysicsData();
			graphicsData = new GraphicsData();
			SamplingRate_TextBlock.Text = $"Logging Frequency: {1 / (sleepTime / 1000f):f3} Hz";
		}

		private void StartLogging_Click(object sender, RoutedEventArgs e)
		{
			started = true;
			index = 0;
			for (int i = 0; i < bufferSize; i++)
			{
				dataGas[i] = 0;
				dataBrake[i] = 0;
				dataGear[i] = 0;
				dataSteer[i] = 0;
				dataLap[i] = 0;
			}

			foreach(var curr in plots)
			{
				curr.sigplotLastLap.visible = false;
			}

			lastCompletedLap = 0;
			beginLapIndices.Clear();

			updateTimer = new DispatcherTimer();
			updateTimer.Interval = TimeSpan.FromMilliseconds(sleepTime);
			updateTimer.Tick += Update;
			updateTimer.Start();

			renderTimer = new DispatcherTimer();
			renderTimer.Interval = TimeSpan.FromMilliseconds(renderSleepTime);
			renderTimer.Tick += RenderSignal;
			renderTimer.Start();
		}

		private void RenderSignal(object sender, EventArgs e)
		{
			bool incrementedLap = lastCompletedLap - 1 < beginLapIndices.Count && beginLapIndices.Count() > 0;
			int beginIndex = beginLapIndices.Count > 0 ? beginLapIndices.Last() : 0;
			if (incrementedLap)
			{
				lastCompletedLap++;
			}
			foreach (TracePlot curr in plots)
			{
				curr.sigplot.maxRenderIndex = index;

				if (incrementedLap)
				{
					int count = beginLapIndices.Count();
					int skip = 0;
					if (count > 1)
					{
						skip = beginLapIndices[count - 2];
						curr.sigplotLastLap.visible = true;
						curr.sigplotLastLap.xOffset = beginIndex;
						curr.sigplotLastLap.ys = curr.sigplot.ys.Skip(skip).ToArray();
					}
				}

				curr.plotFrame.plt.Axis(beginIndex, index - 1, curr.yLims[0], curr.yLims[1]);

				curr.plotFrame.Render();
			}
		}

		private void StopLogging_Click(object sender, RoutedEventArgs e)
		{
			if (started)
			{
				updateTimer.Stop();
				renderTimer.Stop();
				started = false;
			}
		}

		private void Update(object sender, EventArgs e)
		{
			if (index >= bufferSize)
			{
				index = 0;
			}
			PhysicsMemoryMap physics = physData.GetData();
			GraphicsMemoryMap graphics = graphicsData.GetData();

			dataGas[index] = physics.gas;
			dataBrake[index] = physics.brake;
			dataGear[index] = physics.gear - 1; // 0 is reverse, 1 is neutral, 2 is first, etc. I think that's stupid
			dataSteer[index] = physics.steerAngle;
			dataLap[index] = graphics.completedLaps;

			if (index > 0)
			{
				if (dataLap[index] > dataLap[index - 1])
				{
					beginLapIndices.Add(index);
					foreach (TracePlot curr in plots)
					{
						curr.plotFrame.plt.PlotVLine(index, System.Drawing.Color.Gray, lineStyle: LineStyle.Dash);
					}
				}
			}

			index++;
			Thread.Sleep(sleepTime);
		}

		private void PageLink_Click(object sender, RoutedEventArgs e)
		{
			using (Process proc = new Process())
			{
				proc.StartInfo.FileName = "https://github.com/Benny121221/AssettoCorsaTelemetryApp";
				proc.StartInfo.UseShellExecute = true;
				proc.Start();
			}
		}

		public void Dispose()
		{
			physData.Dispose();
			graphicsData.Dispose();
		}

		private void Window_Close(object sender, CancelEventArgs e)
		{
			Dispose();
		}
	}
}
