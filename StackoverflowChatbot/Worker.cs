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
		private readonly IChatService chatService;

		public Worker(ILogger<Worker> logger) => this.logger = logger;

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
			Login();
			JoinRoom(1);
			return base.StartAsync(cancellationToken);
		}

		public override void Dispose() => base.Dispose();
	}
}
