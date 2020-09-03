using System;
using System.IO;
using System.Text.Json;

namespace AssettoCorsaTelemetryApp
{
	public class AppConfigSingleton
	{
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

		public void ExportToCfg(string path)
		{
			string json = JsonSerializer.Serialize(config);
			File.WriteAllText(path, json);
		}

		public void ImportFromCfg(string path)
		{
			config = JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(path));
		}
	}

	public struct AppConfig
	{
		public int samplingInterval { get; set; }
	}
}