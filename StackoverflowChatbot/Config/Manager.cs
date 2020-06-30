using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StackoverflowChatbot.Config
{
	internal static class Manager
	{
		private static Base instance = null;

		private const string CONFIG_FILENAME = "config.json";

		public static Base Config()
		{
			if (instance == null)
			{
				using (var confStream = File.OpenRead(CONFIG_FILENAME))
				{
					var configSpan = new Span<byte>(new byte[confStream.Length]);
					confStream.Position = 0;
					confStream.Read(configSpan);
					var configData = JsonSerializer.Deserialize<Base>(configSpan);
					instance = configData;
				}
			}
			return instance;
		}


	}
}
