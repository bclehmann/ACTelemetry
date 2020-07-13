using ScottPlot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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

	public struct WheelDoubles
	{
		public double[] FL, FR, RL, RR;

		public void InitializeToZero()
		{
			for(int i=0; i< FL.Length; i++)
			{
				FL[i] = 0;
				FR[i] = 0;
				RL[i] = 0;
				RR[i] = 0;
			}
		}
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

		System.Drawing.Color fl_colour = System.Drawing.Color.Cyan;
		System.Drawing.Color fr_colour = System.Drawing.Color.DarkCyan;
		System.Drawing.Color rl_colour = System.Drawing.Color.HotPink;
		System.Drawing.Color rr_colour = System.Drawing.Color.DeepPink;

		double[] dataGas = new double[bufferSize];
		double[] dataGasLast = new double[bufferSize / 50];
		double[] dataBrake = new double[bufferSize];
		double[] dataBrakeLast = new double[bufferSize / 50];
		double[] dataGear = new double[bufferSize];
		double[] dataGearLast = new double[bufferSize / 50];
		double[] dataSteer = new double[bufferSize];
		double[] dataSteerLast = new double[bufferSize / 50];
		double[] dataRPM = new double[bufferSize];

		WheelDoubles dataSlip = new WheelDoubles()
		{
			FL = new double[bufferSize],
			FR = new double[bufferSize],
			RL = new double[bufferSize],
			RR = new double[bufferSize]
		};

		WheelDoubles dataTyreTempI= new WheelDoubles()
		{
			FL = new double[bufferSize],
			FR = new double[bufferSize],
			RL = new double[bufferSize],
			RR = new double[bufferSize]
		};

		WheelDoubles dataTyreTempM = new WheelDoubles()
		{
			FL = new double[bufferSize],
			FR = new double[bufferSize],
			RL = new double[bufferSize],
			RR = new double[bufferSize]
		};

		WheelDoubles dataTyreTempO = new WheelDoubles()
		{
			FL = new double[bufferSize],
			FR = new double[bufferSize],
			RL = new double[bufferSize],
			RR = new double[bufferSize]
		};

		WheelDoubles dataPressures = new WheelDoubles()
		{
			FL = new double[bufferSize],
			FR = new double[bufferSize],
			RL = new double[bufferSize],
			RR = new double[bufferSize]
		};

		double[] dataRideHeightF = new double[bufferSize];
		double[] dataRideHeightR = new double[bufferSize];

		int index = 0;
		int[] dataLap = new int[bufferSize];
		List<int> beginLapIndices = new List<int>();
		int lastCompletedLap = 0;

		DispatcherTimer updateTimer;
		DispatcherTimer renderTimer;
		TracePlot[] plots;

		PhysicsData physData;
		GraphicsData graphicsData;
		StaticsData staticsData;

		StaticsMemoryMap staticsMemoryMap;

		List<ScottPlot.Plottable> sessionLimited = new List<Plottable>();

		bool started = false;


		public MainWindow()
		{
			InitializeComponent();
			UpdateVisibility();

			plots = new TracePlot[27];
			plots[0] = new TracePlot()
			{
				plotFrame = plotFrameGas,
				sigplot = plotFrameGas.plt.PlotSignal(dataGas, color: System.Drawing.Color.FromArgb(255, 25, 150, 0), markerSize: 0),
				sigplotLastLap = plotFrameGas.plt.PlotSignal(dataGasLast, color: System.Drawing.Color.FromArgb(100, 25, 150, 0), markerSize: 0),
				name = "Gas",
				yLims = new double[] { -0.2, 1.2 },
			};
			plots[1] = new TracePlot()
			{
				plotFrame = plotFrameBrake,
				sigplot = plotFrameBrake.plt.PlotSignal(dataBrake, color: System.Drawing.Color.FromArgb(255, 200, 0, 0), markerSize: 0),
				sigplotLastLap = plotFrameBrake.plt.PlotSignal(dataBrakeLast, color: System.Drawing.Color.FromArgb(100, 200, 0, 0), markerSize: 0),
				name = "Brake",
				yLims = new double[] { -0.2, 1.2 },
			};
			plots[2] = new TracePlot()
			{
				plotFrame = plotFrameGear,
				sigplot = plotFrameGear.plt.PlotSignal(dataGear, color: System.Drawing.Color.Black, markerSize: 0),
				sigplotLastLap = plotFrameGear.plt.PlotSignal(dataGearLast, color: System.Drawing.Color.FromArgb(100, 0, 0, 0), markerSize: 0),
				name = "Gear",
				yLims = new double[] { -1.2, 10.2 }, //This'll be fun if someone has a car with 17 gears
			};
			plots[3] = new TracePlot()
			{
				plotFrame = plotFrameSteer,
				sigplot = plotFrameSteer.plt.PlotSignal(dataSteer, color: System.Drawing.Color.FromArgb(255, 100, 0, 200), markerSize: 0),
				sigplotLastLap = plotFrameSteer.plt.PlotSignal(dataSteerLast, color: System.Drawing.Color.FromArgb(100, 100, 0, 200), markerSize: 0),
				name = "Steering Angle",
				yLims = new double[] { -1.2, 1.2 },
			};
			plots[4] = new TracePlot()
			{
				plotFrame = plotFrameSlip,
				sigplot = plotFrameSlip.plt.PlotSignal(dataSlip.FL, label: "FL", color: fl_colour, markerSize: 0),
				name = "Tyre Slip",
				yLims = new double[] { -0.2, 5 },
			};
			plots[5] = new TracePlot()
			{
				plotFrame = plotFrameSlip,
				sigplot = plotFrameSlip.plt.PlotSignal(dataSlip.FR, label: "FR", color: fr_colour, markerSize: 0),
				name = "Tyre Slip",
				yLims = new double[] { -0.2, 5 },
			};
			plots[6] = new TracePlot()
			{
				plotFrame = plotFrameSlip,
				sigplot = plotFrameSlip.plt.PlotSignal(dataSlip.RL, label: "RL", color: rl_colour, markerSize: 0),
				name = "Tyre Slip",
				yLims = new double[] { -0.2, 5 },
			};
			plots[7] = new TracePlot()
			{
				plotFrame = plotFrameSlip,
				sigplot = plotFrameSlip.plt.PlotSignal(dataSlip.RR, label: "RR", color: rr_colour, markerSize: 0),
				name = "Tyre Slip",
				yLims = new double[] { -0.2, 5 },
			};
			plots[8] = new TracePlot()
			{
				plotFrame = plotFrameRPM,
				sigplot = plotFrameRPM.plt.PlotSignal(dataRPM, color: System.Drawing.Color.Gold, markerSize: 0),
				name = "RPM",
				yLims = new double[] { -0.2, 30_000},
			};
			plots[9] = new TracePlot()
			{
				plotFrame = plotFrameTyreTemp,
				sigplot = plotFrameTyreTemp.plt.PlotSignal(dataTyreTempI.FL, label: "FL I", color: fl_colour, markerSize: 0, lineStyle : LineStyle.Dot),
				name = "Tyre Temperature (Celsius)",
				yLims = new double[] { 20, 180 },
			};
			plots[10] = new TracePlot()
			{
				plotFrame = plotFrameTyreTemp,
				sigplot = plotFrameTyreTemp.plt.PlotSignal(dataTyreTempI.FR, label: "FR I", color: fr_colour, markerSize: 0, lineStyle: LineStyle.Dot),
				name = "Tyre Temperature (Celsius)",
				yLims = new double[] { 20, 180 },
			};
			plots[11] = new TracePlot()
			{
				plotFrame = plotFrameTyreTemp,
				sigplot = plotFrameTyreTemp.plt.PlotSignal(dataTyreTempI.RL, label: "RL I", color: rl_colour, markerSize: 0, lineStyle: LineStyle.Dot),
				name = "Tyre Temperature (Celsius)",
				yLims = new double[] { 20, 180 },
			};
			plots[12] = new TracePlot()
			{
				plotFrame = plotFrameTyreTemp,
				sigplot = plotFrameTyreTemp.plt.PlotSignal(dataTyreTempI.RR, label: "RR I", color: rr_colour, markerSize: 0, lineStyle: LineStyle.Dot),
				name = "Tyre Temperature (Celsius)",
				yLims = new double[] { 20, 180 },
			};
			plots[13] = new TracePlot()
			{
				plotFrame = plotFrameTyreTemp,
				sigplot = plotFrameTyreTemp.plt.PlotSignal(dataTyreTempM.FL, label: "FL M", color: fl_colour, markerSize: 0, lineStyle: LineStyle.Dash),
				name = "Tyre Temperature (Celsius)",
				yLims = new double[] { 20, 180 },
			};
			plots[14] = new TracePlot()
			{
				plotFrame = plotFrameTyreTemp,
				sigplot = plotFrameTyreTemp.plt.PlotSignal(dataTyreTempM.FR, label: "FR M", color: fr_colour, markerSize: 0, lineStyle: LineStyle.Dash),
				name = "Tyre Temperature (Celsius)",
				yLims = new double[] { 20, 180 },
			};
			plots[15] = new TracePlot()
			{
				plotFrame = plotFrameTyreTemp,
				sigplot = plotFrameTyreTemp.plt.PlotSignal(dataTyreTempM.RL, label: "RL M", color: rl_colour, markerSize: 0, lineStyle: LineStyle.Dash),
				name = "Tyre Temperature (Celsius)",
				yLims = new double[] { 20, 180 },
			};
			plots[16] = new TracePlot()
			{
				plotFrame = plotFrameTyreTemp,
				sigplot = plotFrameTyreTemp.plt.PlotSignal(dataTyreTempM.RR, label: "RR M", color: rr_colour, markerSize: 0, lineStyle: LineStyle.Dash),
				name = "Tyre Temperature (Celsius)",
				yLims = new double[] { 20, 180 },
			};
			plots[17] = new TracePlot()
			{
				plotFrame = plotFrameTyreTemp,
				sigplot = plotFrameTyreTemp.plt.PlotSignal(dataTyreTempO.FL, label: "FL O", color: fl_colour, markerSize: 0, lineStyle: LineStyle.Solid),
				name = "Tyre Temperature (Celsius)",
				yLims = new double[] { 20, 180 },
			};
			plots[18] = new TracePlot()
			{
				plotFrame = plotFrameTyreTemp,
				sigplot = plotFrameTyreTemp.plt.PlotSignal(dataTyreTempO.FR, label: "FR O", color: fr_colour, markerSize: 0, lineStyle: LineStyle.Solid),
				name = "Tyre Temperature (Celsius)",
				yLims = new double[] { 20, 180 },
			};
			plots[19] = new TracePlot()
			{
				plotFrame = plotFrameTyreTemp,
				sigplot = plotFrameTyreTemp.plt.PlotSignal(dataTyreTempO.RL, label: "RL O", color: rl_colour, markerSize: 0, lineStyle: LineStyle.Solid),
				name = "Tyre Temperature (Celsius)",
				yLims = new double[] { 20, 180 },
			};
			plots[20] = new TracePlot()
			{
				plotFrame = plotFrameTyreTemp,
				sigplot = plotFrameTyreTemp.plt.PlotSignal(dataTyreTempO.RR, label: "RR O", color: rr_colour, markerSize: 0, lineStyle: LineStyle.Solid),
				name = "Tyre Temperature (Celsius)",
				yLims = new double[] { 20, 180 },
			};
			plots[21] = new TracePlot()
			{
				plotFrame = plotFrameTyrePressure,
				sigplot = plotFrameTyrePressure.plt.PlotSignal(dataPressures.FL, label: "FL", color: fl_colour, markerSize: 0, lineStyle: LineStyle.Solid),
				name = "Tyre Pressure (PSI)",
				yLims = new double[] { 0, 60 },
			};
			plots[22] = new TracePlot()
			{
				plotFrame = plotFrameTyrePressure,
				sigplot = plotFrameTyrePressure.plt.PlotSignal(dataPressures.FR, label: "FR", color: fr_colour, markerSize: 0, lineStyle: LineStyle.Solid),
				name = "Tyre Pressure (PSI)",
				yLims = new double[] { 0, 60 },
			};
			plots[23] = new TracePlot()
			{
				plotFrame = plotFrameTyrePressure,
				sigplot = plotFrameTyrePressure.plt.PlotSignal(dataPressures.RL, label: "RL", color: rl_colour, markerSize: 0, lineStyle: LineStyle.Solid),
				name = "Tyre Pressure (PSI)",
				yLims = new double[] { 0, 60 },
			};
			plots[24] = new TracePlot()
			{
				plotFrame = plotFrameTyrePressure,
				sigplot = plotFrameTyrePressure.plt.PlotSignal(dataPressures.RR, label: "RR", color: rr_colour, markerSize: 0, lineStyle: LineStyle.Solid),
				name = "Tyre Pressure (PSI)",
				yLims = new double[] { 0, 60 },
			};
			plots[25] = new TracePlot()
			{
				plotFrame = plotFrameRideHeight,
				sigplot = plotFrameRideHeight.plt.PlotSignal(dataRideHeightF, label: "F", color: fl_colour, markerSize: 0, lineStyle: LineStyle.Solid),
				name = "Ride Height (Physics-Engine Units)",
				yLims = new double[] { 0, 0.3 },
			};
			plots[26] = new TracePlot()
			{
				plotFrame = plotFrameRideHeight,
				sigplot = plotFrameRideHeight.plt.PlotSignal(dataRideHeightR, label: "R", color: rl_colour, markerSize: 0, lineStyle: LineStyle.Solid),
				name = "Ride Height (Physics-Engine Units)",
				yLims = new double[] { 0, 0.3 },
			};

			foreach (TracePlot curr in plots)
			{
				curr.plotFrame.plt.Title(curr.name);
				curr.plotFrame.plt.Ticks(displayTickLabelsX: false);
				curr.plotFrame.plt.Axis(0, 0, curr.yLims[0], curr.yLims[1]);
				if (curr.sigplotLastLap != null)
				{
					curr.sigplotLastLap.visible = false;
				}
			}

			string[] gearYTicks = Enumerable.Range(-1, 12).Select(i => "" + i).ToArray();
			gearYTicks[0] = "R";
			gearYTicks[1] = "N";
			plotFrameGear.plt.YTicks(Enumerable.Range(-1, 12).Select(i => (double)i).ToArray(), gearYTicks);

			plotFrameSlip.plt.Legend(location: legendLocation.upperRight);
			plotFrameTyreTemp.plt.Legend(location: legendLocation.upperRight);
			plotFrameTyrePressure.plt.Legend(location: legendLocation.lowerRight);
			plotFrameRideHeight.plt.Legend(location: legendLocation.upperRight);

			plotFrameTyreTemp.plt.PlotAnnotation("Interior: Dotted\nMiddle: Dashed\nOuter: Solid");

			plotFrameSteer.plt.PlotHLine(0, System.Drawing.Color.Gray, lineStyle: LineStyle.Dash);


			SamplingRate_TextBlock.Text = $"Logging Frequency: {1 / (sleepTime / 1000f):f3} Hz";
		}

		private void StartLogging_Click(object sender, RoutedEventArgs e)
		{
			physData = new PhysicsData();
			graphicsData = new GraphicsData();
			staticsData = new StaticsData();
			staticsMemoryMap = staticsData.GetData();

			foreach(var curr in plots)
			{
				curr.plotFrame.plt.Clear(p => sessionLimited.Contains(p));

				if (curr.plotFrame == plotFrameRPM)
				{
					curr.yLims[1] = staticsMemoryMap.maxRpm + 2000;
				}
			}

			sessionLimited.Clear();
			sessionLimited.Add(plotFrameRPM.plt.PlotHLine((double)staticsMemoryMap.maxRpm, System.Drawing.Color.Red, lineStyle: LineStyle.Dot));

			if (started)
			{
				renderTimer.Stop();
				updateTimer.Stop();
			}

			started = true;
			index = 0;
			for (int i = 0; i < bufferSize; i++)
			{
				dataGas[i] = 0;
				dataBrake[i] = 0;
				dataGear[i] = 0;
				dataSteer[i] = 0;
				dataLap[i] = 0;
				dataRPM[i] = 0;
			}

			dataSlip.InitializeToZero();
			dataTyreTempI.InitializeToZero();
			dataTyreTempM.InitializeToZero();
			dataTyreTempO.InitializeToZero();

			foreach (var curr in plots)
			{
				if (curr.sigplotLastLap != null)
				{
					curr.sigplotLastLap.visible = false;
				}
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

				if (incrementedLap && curr.sigplotLastLap != null)
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
				physData.Dispose();
				graphicsData.Dispose();

				updateTimer.Stop();
				renderTimer.Stop();
				started = false;
			}
		}

		private unsafe void Update(object sender, EventArgs e)
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
			dataSlip.FL[index] = physics.wheelSlip[0];
			dataSlip.FR[index] = physics.wheelSlip[1];
			dataSlip.RL[index] = physics.wheelSlip[2];
			dataSlip.RR[index] = physics.wheelSlip[3];
			dataRPM[index] = physics.rpms;

			dataTyreTempI.FL[index] = physics.tyreTempI[0];
			dataTyreTempI.FR[index] = physics.tyreTempI[1];
			dataTyreTempI.RL[index] = physics.tyreTempI[2];
			dataTyreTempI.RR[index] = physics.tyreTempI[3];

			dataTyreTempM.FL[index] = physics.tyreTempM[0];
			dataTyreTempM.FR[index] = physics.tyreTempM[1];
			dataTyreTempM.RL[index] = physics.tyreTempM[2];
			dataTyreTempM.RR[index] = physics.tyreTempM[3];

			dataTyreTempO.FL[index] = physics.tyreTempO[0];
			dataTyreTempO.FR[index] = physics.tyreTempO[1];
			dataTyreTempO.RL[index] = physics.tyreTempO[2];
			dataTyreTempO.RR[index] = physics.tyreTempO[3];

			dataPressures.FL[index] = physics.wheelsPressure[0];
			dataPressures.FR[index] = physics.wheelsPressure[1];
			dataPressures.RL[index] = physics.wheelsPressure[2];
			dataPressures.RR[index] = physics.wheelsPressure[3];

			dataRideHeightF[index] = physics.rideHeight[0];
			dataRideHeightR[index] = physics.rideHeight[1];

			if (index > 0)
			{
				if (dataLap[index] > dataLap[index - 1])
				{
					beginLapIndices.Add(index);
					foreach (TracePlot curr in plots)
					{
						sessionLimited.Add(curr.plotFrame.plt.PlotVLine(index, System.Drawing.Color.Gray, lineStyle: LineStyle.Dash));
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

		private void CSVExport_Click(object sender, RoutedEventArgs e)
		{
			Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
			dlg.FileName = $"{DateTime.UtcNow.ToString(@"yyyy-MM-dd HH\hmm\mss")}Z ACTelemetry";
			dlg.DefaultExt = ".csv";

			bool? result = dlg.ShowDialog();

			if (result == true)
			{ //Nullable
				StringBuilder output = new StringBuilder();
				output.Append("gas,brake,gear,steer,slip_fl,slip_fr,slip_rl,slip_rr,temp_fl_i,temp_fr_i,temp_rl_i,temp_rr_i,templ_fl_m,temp_fr_m,temp_rl_m,temp_rr_m,temp_fl_o,temp_fr_o,temp_rl_o,femp_rr_o,pressure_fl,pressure_fr,pressure_rl,pressure_rr,rideHeight_f,rideHeight_r\n");
				for (int i = 0; i < index; i++)
				{
					output.Append($"{dataGas[i]},{dataBrake[i]},{dataGear[i]},{dataSteer[i]},{dataSlip.FL[i]},{dataSlip.FR[i]},{dataSlip.RL[i]},{dataSlip.RR[i]}" +
						$",{dataTyreTempI.FL[i]},{dataTyreTempI.FR[i]},{dataTyreTempI.RL[i]},{dataTyreTempI.RR[i]},{dataTyreTempM.FL[i]},{dataTyreTempM.FR[i]},{dataTyreTempM.RL[i]},{dataTyreTempM.RR[i]}" +
						$",{dataTyreTempO.FL[i]},{dataTyreTempO.FR[i]},{dataTyreTempO.RL[i]},{dataTyreTempO.RR[i]},{dataPressures.FL[i]},{dataPressures.FR[i]},{dataPressures.RL[i]},{dataPressures.RR[i]}" +
						$",{dataRideHeightF[i]},{dataRideHeightR[i]}\n");
				}

				using (StreamWriter writer = new StreamWriter(dlg.FileName))
				{
					writer.Write(output);
				}
			}
		}

		public void Dispose()
		{
			physData.Dispose();
			graphicsData.Dispose();
		}

		private void Window_Close(object sender, CancelEventArgs e)
		{
			if (started)
				Dispose();
		}

		private void UpdateVisibility()
		{
			Resources["basicsVisibility"] = basics.IsChecked ?? false ? Visibility.Visible : Visibility.Collapsed;
			Resources["slipVisibility"] = slip.IsChecked ?? false ? Visibility.Visible : Visibility.Collapsed;
			Resources["temperatureVisibility"] = temperatures.IsChecked ?? false ? Visibility.Visible : Visibility.Collapsed;
			Resources["suspensionVisibility"] = suspension.IsChecked ?? false ? Visibility.Visible : Visibility.Collapsed;

			Resources["basicsHeight"] = basics.IsChecked ?? false ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
			Resources["slipHeight"] = slip.IsChecked ?? false ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
			Resources["temperatureHeight"] = temperatures.IsChecked ?? false ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
			Resources["suspensionHeight"] = suspension.IsChecked ?? false ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
		}

		private void checkbox_Click(object sender, RoutedEventArgs e)
		{
			UpdateVisibility();
		}
	}
}
