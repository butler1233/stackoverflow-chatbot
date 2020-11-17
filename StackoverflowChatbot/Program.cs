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
			if (args == null || args.Length == 0)
			{
				Console.Write("Enter username: ");
				var u = Console.ReadLine();
				Console.Write("Enter password: ");
				var p = Console.ReadLine();
				args = new [] { u, p };
			}
			var username = args[0];
			var password = args[1];
			System.Console.WriteLine("Args[0]: " + username);

			//Update config file as 3rd arg if it's available.
			if (args.Length >= 3)
			{
				Manager.CONFIG_FILENAME = args[2];
			}


			return Host.CreateDefaultBuilder(args)
			.ConfigureServices(
				(hostContext, services) =>
				_ = services.AddHostedService<Worker>()
				.AddSingleton<IRoomService>(new RoomService(username, password))
			);
		}
	}
}
