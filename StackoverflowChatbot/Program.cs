using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackoverflowChatbot.Settings;

namespace StackoverflowChatbot
{
	public class Program
	{

		public static SettingBase Settings;
		

		public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

		public static IHostBuilder CreateHostBuilder(string[] args)
		{
			System.Console.WriteLine("Args[0]: " + args[0]);

			return Host.CreateDefaultBuilder(args)
				.ConfigureServices(
				(hostContext, services) =>
				_ = services.AddHostedService<Worker>()
				.AddSingleton<IRoomService>(new RoomService(args[0], args[1]))
			);
		}
	}
}
