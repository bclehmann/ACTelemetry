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
		private AppConfigSingleton configWrapper { get; set; }
		App()
		{
			configWrapper = AppConfigSingleton.GetInstance();
			configWrapper.ImportFromCfg();
		}
	}
}
