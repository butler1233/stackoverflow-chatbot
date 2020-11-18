using System.Collections.Generic;
using SharpExchange.Auth;
using SharpExchange.Chat.Actions;
using SharpExchange.Chat.Events;
using SharpExchange.Net.WebSockets;
using StackoverflowChatbot.Actions;

namespace StackoverflowChatbot
{
	internal class RoomService: IRoomService
	{
		internal const string Host = "chat.stackoverflow.com";
		private readonly EmailAuthenticationProvider auth;
		private readonly Dictionary<int, RoomWatcher<DefaultWebSocket>> activeRooms;
		public RoomService(string username, string password)
		{
			this.auth = new EmailAuthenticationProvider(username, password);
			this.activeRooms = new Dictionary<int, RoomWatcher<DefaultWebSocket>>();
		}

		public bool Login() => this.auth.Login("stackoverflow.com");

		public bool JoinRoom(int roomNumber)
		{
			if (this.activeRooms.ContainsKey(roomNumber))
			{
				return false;
			}

			var newRoomWatcher = this.NewRoomWatcherFor(roomNumber);
			Discord.StackRoomWatchers.Add(roomNumber, newRoomWatcher);
			this.activeRooms.Add(roomNumber, newRoomWatcher);
			//var discord = Discord.GetDiscord();
			
			return true;
		}

		private RoomWatcher<DefaultWebSocket> NewRoomWatcherFor(int roomNumber)
		{
			var newRoomWatcher =
				new RoomWatcher<DefaultWebSocket>(this.auth, $"https://chat.stackoverflow.com/rooms/{roomNumber}");
			var messageHandler = new ChatEventHandler();
			var scheduler = new SharpExchange.Chat.Actions.ActionScheduler(this.auth, Host, roomNumber);
			var router = new CommandRouter(this, roomNumber,
				scheduler);
			messageHandler.OnEvent += router.RouteCommand;
			newRoomWatcher.EventRouter.AddProcessor(messageHandler);

			scheduler.CreateMessageAsync("Hello friends!");

			return newRoomWatcher;
		}

		public void LeaveRoom(int roomNumber)
		{
			if (!this.activeRooms.ContainsKey(roomNumber)) return;

			var watcher = this.activeRooms[roomNumber];
			_ = this.activeRooms.Remove(roomNumber);
			watcher.Dispose();
			if (this.activeRooms.Count == 0)
			{
				System.Diagnostics.Process.GetCurrentProcess().Kill();
			}
		}
	}
}
