using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace StackoverflowChatbot
{
	public class Worker: BackgroundService
	{
		private readonly ILogger<Worker> logger;
		private readonly IRoomService chatService;

		public Worker(ILogger<Worker> logger, IRoomService chatService)
		{
			this.logger = logger;
			this.chatService = chatService;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				this.logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
				await Task.Delay(1000, stoppingToken);
			}
		}

		public override Task StartAsync(CancellationToken cancellationToken)
		{
			//this.Login();
			this.JoinRoom(1);
			return base.StartAsync(cancellationToken);
		}

		private void JoinRoom(int roomNumber) => this.chatService.JoinRoom(roomNumber);
		private void Login() => this.chatService.Login();
		public override void Dispose() => base.Dispose();
	}
}
