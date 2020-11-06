using System;
using System.IO;
using System.Text.Json;

namespace AssettoCorsaTelemetryApp
{
	public class AppConfigSingleton
	{
		public static readonly string appdataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		public static readonly string cfgPath = appdataDirectory + "/ACTelemetry/config.json";

		private static AppConfigSingleton instance;
		private static readonly object _lock = new object();

		public AppConfig config;


		private AppConfigSingleton() { }
		public static AppConfigSingleton GetInstance()
		{
			lock (_lock)
			{
				if (instance == null)
				{
					instance = new AppConfigSingleton();
				}
			}

			return instance;
		}

		public void ExportToCfg()
		{
			string json = JsonSerializer.Serialize(config);
			File.WriteAllText(cfgPath, json);
		}

		public void ImportFromCfg()
		{
			if (System.IO.File.Exists(AppConfigSingleton.cfgPath))
			{
				config = JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(cfgPath));
			}
			else
			{
				Directory.CreateDirectory(appdataDirectory + "/ACTelemetry");
				File.Create(cfgPath).Close();
				config = new AppConfig();
				config.samplingInterval = 20;
				ExportToCfg();
			}
		}
	}

	public struct AppConfig
	{
		public int samplingInterval { get; set; }
	}
}