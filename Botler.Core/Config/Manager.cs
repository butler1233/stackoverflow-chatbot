using System.Text;
using System.Text.Json;
using Serilog;
using Serilog.Core;

namespace Botler.Core.Config
{
	public static class Manager
	{
		private static Base? _instance;

		public static string CONFIG_FILENAME = "config.secret.json";

		public static Logger Logger { get; set; } = ConfigureLogger().CreateLogger();

		public static Base Config()
		{
			if (_instance == null)
			{
				try
				{
					var cf = new FileInfo(CONFIG_FILENAME);
					Logger.Information("About to open {0} ({1} bytes)", cf.FullName, cf.Length);
					using var confStream = File.OpenRead(CONFIG_FILENAME);
					var configSpan = new Span<byte>(new byte[confStream.Length]);
					confStream.Position = 0;
					confStream.Read(configSpan);
					var configData = JsonSerializer.Deserialize<Base>(configSpan);
					configData.StackToDiscordMap = new Dictionary<int, string>();
					foreach (var pair in configData.DiscordToStackMap)
					{
						configData.StackToDiscordMap.Add(pair.Value, pair.Key);
					}
					_instance = configData;

					Log.Information("Loaded config. my triggers are: {0}", string.Join(", ", _instance.Triggers));
				}
				catch (Exception e)
				{
					Log.Error(e, "Failed to load config");
					_instance = new Base();
				}
			}
			return _instance;
		}

		public static void SaveConfig()
		{
			var json = JsonSerializer.Serialize(_instance);
			using var confStream = File.OpenWrite(CONFIG_FILENAME);
			confStream.Position = 0; //Just to make sure
			var bytes = Encoding.UTF8.GetBytes(json);
			confStream.Write(bytes, 0, bytes.Length);
			confStream.SetLength(bytes.Length);
			//Tada, it should be saved
		}

		public static LoggerConfiguration ConfigureLogger()
		{
			Serilog.Debugging.SelfLog.Enable(Console.Error);
			return new LoggerConfiguration()
					.WriteTo.Console()
					.Enrich.WithThreadId()
					.Enrich.WithAssemblyName()
			;
		}

	}
}
