using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Botler.Core.Config;
using Botler.Database;
using Discord;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using StackoverflowChatbot.CommandProcessors;
using StackoverflowChatbot.Services;
using StackoverflowChatbot.Services.Repositories;

namespace StackoverflowChatbot
{
	public class Program
	{
		public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

		public static IHostBuilder CreateHostBuilder(string[] args)
		{
			string username = "";
			string password = "";

			if (args.Length > 0 && args[0] == "skip")
			{
				//skip everything
			}
			else
			{
				if (args.Length == 0)
				{
					Console.Write("Enter username: ");
					var u = Console.ReadLine();
					Console.Write("Enter password: ");
					var p = Console.ReadLine();
					args = new[] { u, p };
				}
				username = args[0];
				password = args[1];

				var log = Manager.Logger;

				log.Information("Using username: {}", username);

				//Update config file as 3rd arg if it's available.
				if (args.Length >= 3)
				{
					Manager.CONFIG_FILENAME = args[2];
					log.Information("Using config: {0}", Manager.CONFIG_FILENAME);
				}


				// TODO make the Config a service also
				var config = Manager.Config();

				if (string.IsNullOrEmpty(config.SeqUrl))
				{
					log.Warning("Seq config url is empty! logs will not be sent anywhere.");
				}
				else
				{
					Manager.Logger = Manager.ConfigureLogger()
						.WriteTo.Seq(config.SeqUrl, apiKey: config.SeqApiKey)
						.CreateLogger();
					log = Manager.Logger;
					Manager.Logger.Information("Reinitialized logging with Seq");
				}

				log.Information("Checking Database...");

				log.Information("Checking if database needs updating");
				var context = new SqliteContext();
				var pending = context.Database.GetPendingMigrations();
				if (pending.Count() > 0)
				{
					log.Warning("Migrations pending, lets go! Potentially RIP data...");
					context.Database.Migrate();
					log.Information("Database should now be up to date.");
				}
				else
				{
					log.Information("Database appears to be up to date.");
				}

				log.Information("Checking discord connection");
				var discord = Discord.GetDiscord().Result;

				discord.Disconnected += exception =>
				{
					log.Error(exception, $"Discord disconnected!");
					return Task.CompletedTask;
				};

				while (discord.ConnectionState != ConnectionState.Connected)
				{
					log.Information($"Discord: {discord.ConnectionState}");
					Thread.Sleep(333);
				}
				log.Information($"Discord: {discord.ConnectionState}, latency {discord.Latency}");

			}



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
