using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackoverflowChatbot.Config;

namespace StackoverflowChatbot
{
	public class Program
	{
		public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

		public static IHostBuilder CreateHostBuilder(string[] args)
		{
			System.Console.WriteLine("Args[0]: " + args[0]);

			//Update config file as 3rd arg if it's available.
			if (args.Length >= 3)
			{
				Manager.CONFIG_FILENAME = args[2];
			}


			return Host.CreateDefaultBuilder(args)
			.ConfigureServices(
				(hostContext, services) =>
				_ = services.AddHostedService<Worker>()
				.AddSingleton<IRoomService>(new RoomService(args[0], args[1]))
			);
		}
	}
}
