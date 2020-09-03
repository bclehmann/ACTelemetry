using AdonisUI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AssettoCorsaTelemetryApp
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private static readonly string appdataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		private static readonly string cfgPath = appdataDirectory + "/ACTelemetry/config.json";
		private AppConfigSingleton configWrapper { get; set; }
		App()
		{
			configWrapper = AppConfigSingleton.GetInstance();
			if (System.IO.File.Exists(cfgPath))
			{
				configWrapper.ImportFromCfg(cfgPath);
			}
			else
			{
				configWrapper.config.samplingInterval = 20;
				configWrapper.ExportToCfg(cfgPath);
			}
		}
	}
}
