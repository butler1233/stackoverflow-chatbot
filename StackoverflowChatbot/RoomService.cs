using System;
using System.Collections.Generic;
using System.Text;
using SharpExchange.Auth;
using SharpExchange.Chat.Events;
using SharpExchange.Net.WebSockets;
using StackoverflowChatbot.EventProcessors;

namespace StackoverflowChatbot
{
	internal class RoomService: IRoomService
	{
		private const string Host = "chat.stackoverflow.com";
		private const string StandardRoom = "https://chat.stackoverflow.com/rooms/1/sandbox";
		private readonly EmailAuthenticationProvider auth;
		private readonly Dictionary<int, RoomWatcher<DefaultWebSocket>> activeRooms;
		public RoomService(string username, string password)
		{
			this.auth = new EmailAuthenticationProvider(username, password);
			this.activeRooms = new Dictionary<int, RoomWatcher<DefaultWebSocket>>();
		}

		public void Login()
		{
			// auth.Login(Host);
			using var _ = new RoomWatcher<DefaultWebSocket>(this.auth, StandardRoom);
		}

		public bool JoinRoom(int roomNumber)
		{
			if (this.activeRooms.ContainsKey(roomNumber))
			{
				return false;
			}

			var newRoomWatcher = new RoomWatcher<DefaultWebSocket>(this.auth, $"https://chat.stackoverflow.com/rooms/{roomNumber}");
			var newMessageHandler = new MessagePosted();
			var editedMessageHandler = new MessageEdited();
			var router = new CommandRouter(this, roomNumber, new SharpExchange.Chat.Actions.ActionScheduler(this.auth, Host, roomNumber));
			newMessageHandler.OnEvent += router.RouteCommand;
			editedMessageHandler.OnEvent += router.RouteCommand;
			newRoomWatcher.EventRouter.AddProcessor(newMessageHandler);
			newRoomWatcher.EventRouter.AddProcessor(editedMessageHandler);
			this.activeRooms.Add(roomNumber, newRoomWatcher);
			return true;
		}

		public void LeaveRoom(int roomNumber)
		{
			if (this.activeRooms.ContainsKey(roomNumber))
			{
				var watcher = this.activeRooms[roomNumber];
				_ = this.activeRooms.Remove(roomNumber);
				watcher.Dispose();
				if(this.activeRooms.Count == 0)
				{
					System.Diagnostics.Process.GetCurrentProcess().Kill();
				}
			}
		}
	}
}
