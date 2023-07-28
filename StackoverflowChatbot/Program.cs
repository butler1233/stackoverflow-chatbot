using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackoverflowChatbot.CommandProcessors;
using StackoverflowChatbot.Config;
using StackoverflowChatbot.Database;
using StackoverflowChatbot.Services;
using StackoverflowChatbot.Services.Repositories;

namespace StackoverflowChatbot
{
	public class Program
	{
		public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

		public static IHostBuilder CreateHostBuilder(string[] args)
		{
			if (args.Length == 0)
			{
				Console.Write("Enter username: ");
				var u = Console.ReadLine();
				Console.Write("Enter password: ");
				var p = Console.ReadLine();
				args = new [] { u, p };
			}
			var username = args[0];
			var password = args[1];
			Console.WriteLine("Args[0]: " + username);

			//Update config file as 3rd arg if it's available.
			if (args.Length >= 3)
			{
				Manager.CONFIG_FILENAME = args[2];
			}

			// TODO make the Config a service also
			var config = Manager.Config();

			Console.WriteLine("Checking if database needs updating");
			var context = new SqliteContext();
			var pending = context.Database.GetPendingMigrations();
			if (pending.Count() > 0)
			{
				Console.WriteLine("Migrations pending, lets go!");
				context.Database.Migrate();
				Console.WriteLine("Database should now be up to date.");
			}

			Console.WriteLine("Checking discord connection");
			var discord = Discord.GetDiscord().Result;

			discord.Disconnected += exception =>
			{
				Console.WriteLine($"Discord rip error: {exception.ToString()}");
				return Task.CompletedTask;
			};
			
			while (discord.ConnectionState != ConnectionState.Connected)
			{
				Console.WriteLine($"Discord: {discord.ConnectionState}");
				Thread.Sleep(333);
			}
			Console.WriteLine($"Discord: {discord.ConnectionState}, latency {discord.Latency}");

			
			return Host.CreateDefaultBuilder(args)
			.ConfigureServices(
				(hostContext, services) =>
					_ = services.AddHostedService<Worker>()
						.AddSingleton<IIdentityProvider>(new IdentityProvider(username, password))
						//.AddSingleton<IRepositoryService>(new FirebaseRepositoryService(config.FirebaseProjectId))
						.AddSingleton<IRepositoryService>(new MonkeyCacheRepositoryService("so-chatbot"))
						.AddSingleton<ICommandStore, CommandStore>()
						.AddSingleton<IRoomService, RoomService>()
						.AddSingleton<ICommandFactory, CommandFactory>()
						.AddSingleton<IHttpService, HttpService>()
						// leave it last row
						.AddSingleton<IServiceCollection, ServiceCollection>(ser => (ServiceCollection)services)
			);
		}
	}
}
