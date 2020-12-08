using System.Collections.Generic;
using System.Diagnostics;
using SharpExchange.Auth;
using SharpExchange.Chat.Actions;
using SharpExchange.Chat.Events;
using SharpExchange.Net.WebSockets;
using StackoverflowChatbot.CommandProcessors;
using StackoverflowChatbot.Services;

namespace StackoverflowChatbot
{
	internal class RoomService: IRoomService
	{
		internal const string Host = "chat.stackoverflow.com";
		private readonly EmailAuthenticationProvider _auth;
		private readonly Dictionary<int, RoomWatcher<DefaultWebSocket>> _activeRooms;
		private readonly ICommandStore _commandService;
		private readonly IHttpService _httpService;
		private readonly ICommandFactory _commandFactory;

		public RoomService(IIdentityProvider identityProvider, ICommandStore commandService, IHttpService httpService, ICommandFactory commandFactory)
		{
			_auth = new EmailAuthenticationProvider(identityProvider.Username, identityProvider.Password);
			_commandService = commandService;
			_httpService = httpService;
			_activeRooms = new Dictionary<int, RoomWatcher<DefaultWebSocket>>();
			_commandFactory = commandFactory;
		}

		public bool Login() => _auth.Login("stackoverflow.com");

		public bool JoinRoom(int roomNumber)
		{
			if (_activeRooms.ContainsKey(roomNumber))
			{
				return false;
			}

			var newRoomWatcher = NewRoomWatcherFor(roomNumber);
			Discord.StackRoomWatchers.Add(roomNumber, newRoomWatcher);
			_activeRooms.Add(roomNumber, newRoomWatcher);
			//var discord = Discord.GetDiscord();
			
			return true;
		}

		private RoomWatcher<DefaultWebSocket> NewRoomWatcherFor(int roomNumber)
		{
			var newRoomWatcher =
				new RoomWatcher<DefaultWebSocket>(_auth, $"https://chat.stackoverflow.com/rooms/{roomNumber}");
			var messageHandler = new ChatEventHandler();
			var scheduler = new ActionScheduler(_auth, Host, roomNumber);
			var router = new CommandRouter(_commandService, _httpService, _commandFactory, scheduler);
			messageHandler.OnEvent += router.RouteCommand;
			newRoomWatcher.EventRouter.AddProcessor(messageHandler);

			scheduler.CreateMessageAsync("Hello friends!");

			return newRoomWatcher;
		}

		public void LeaveRoom(int roomNumber)
		{
			if (!_activeRooms.ContainsKey(roomNumber)) return;

			var watcher = _activeRooms[roomNumber];
			_ = _activeRooms.Remove(roomNumber);
			watcher.Dispose();
			if (_activeRooms.Count == 0)
			{
				Process.GetCurrentProcess().Kill();
			}
		}
	}
}
