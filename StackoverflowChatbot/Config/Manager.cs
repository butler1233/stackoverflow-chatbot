using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace StackoverflowChatbot.Config
{
	internal static class Manager
	{
		private static Base instance = null!;

		internal static string CONFIG_FILENAME = "config.json";

		public static Base Config()
		{
			if (instance == null)
			{
				using var confStream = File.OpenRead(CONFIG_FILENAME);
				var configSpan = new Span<byte>(new byte[confStream.Length]);
				confStream.Position = 0;
				confStream.Read(configSpan);
				var configData = JsonSerializer.Deserialize<Base>(configSpan);
				instance = configData;
			}
			return instance;
		}

		public static void SaveConfig()
		{
			var json = JsonSerializer.Serialize(instance);
			using var confStream = File.OpenWrite(CONFIG_FILENAME);
			confStream.Position = 0; //Just to make sure
			var bytes = Encoding.UTF8.GetBytes(json);
			confStream.Write(bytes, 0, bytes.Length);
			confStream.SetLength(bytes.Length);
			//Tada, it should be saved
		}

	}
}
