using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackoverflowChatbot.Config;
using StackoverflowChatbot.Services;

namespace StackoverflowChatbot
{
	public class Worker: BackgroundService
	{
		private readonly ILogger<Worker> _logger;
		private readonly IRoomService _chatService; //Statics weeeeeeeeeeeeeee // no more


		public Worker(ILogger<Worker> logger, IRoomService chatService, IConfiguration config)
		{
			_logger = logger;
			_chatService = chatService;
			AppDomain.CurrentDomain.SetData("AdminId", config.GetValue<int>("AdminId"));
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
			var loggedIn = Login();
			_logger.LogInformation($"Logged in: {loggedIn}");
			var joinedSandbox = JoinRoom(1);
			_logger.LogInformation($"Joined Sandbox: {joinedSandbox}");

			var autoJoinRooms = Manager.Config().AutoJoinRoomIds;

			foreach (var room in autoJoinRooms)
			{
				_logger.LogInformation($"Auto joining room {room}");
				JoinRoom(room);
			}

			return base.StartAsync(cancellationToken);
		}

		private bool JoinRoom(int roomNumber) => _chatService.JoinRoom(roomNumber);
		private bool Login() => _chatService.Login();
	}
}
