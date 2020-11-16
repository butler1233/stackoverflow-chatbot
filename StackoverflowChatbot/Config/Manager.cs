using System;
using System.Collections.Generic;
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
        try
        {
            var cf = new FileInfo(CONFIG_FILENAME);
            Console.WriteLine($"About to open {cf.FullName} ({cf.Length / 1024}kB)");
            using var confStream = File.OpenRead(CONFIG_FILENAME);
            var configSpan = new Span<byte>(new byte[confStream.Length]);
            confStream.Position = 0;
            confStream.Read(configSpan);
            var configData = JsonSerializer.Deserialize<Base>(configSpan);
					configData.StackToDiscordMap = new Dictionary<int, string>();
			foreach (var pair in configData.DiscordToStackMap)
			{
				
			}

            instance = configData;
            Console.WriteLine($"Loaded config. my triggers are: {string.Join(", ", instance.Triggers)}");
        }
        catch (Exception e)
				{
					Console.WriteLine(e.ToString());
				}
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
