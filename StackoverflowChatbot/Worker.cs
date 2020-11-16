using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace StackoverflowChatbot
{
	public class Worker: BackgroundService
	{
		private readonly ILogger<Worker> logger;
		internal static IRoomService chatService; //Statics weeeeeeeeeeeeeee
		internal readonly IConfiguration Configuration;

		public Worker(ILogger<Worker> logger, IRoomService chatService, IConfiguration config)
		{
			this.logger = logger;
			Worker.chatService = chatService;
			this.Configuration = config;
			AppDomain.CurrentDomain.SetData("AdminId", this.Configuration.GetValue<int>("AdminId"));
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				//this.logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
				await Task.Delay(1000, stoppingToken);
			}
		}

		public override Task StartAsync(CancellationToken cancellationToken)
		{
			var loggedIn = this.Login();
			this.logger.LogInformation($"Logged in: {loggedIn}");
			var joinedSandbox = this.JoinRoom(1);
			this.logger.LogInformation($"Joined Sandbox: {joinedSandbox}");
			return base.StartAsync(cancellationToken);
		}

		private bool JoinRoom(int roomNumber) => Worker.chatService.JoinRoom(roomNumber);
		private bool Login() => Worker.chatService.Login();
		public override void Dispose() => base.Dispose();
	}
}
