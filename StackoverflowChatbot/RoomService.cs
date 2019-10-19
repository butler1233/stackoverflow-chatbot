using System;
using System.Collections.Generic;
using System.Text;
using SharpExchange.Auth;
using SharpExchange.Chat.Events;
using SharpExchange.Net.WebSockets;

namespace StackoverflowChatbot
{
	internal class RoomService: IRoomService
	{
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
			// auth.Login("stackoverflow.com");
		}

		public void JoinRoom(int roomNumber)
		{
			if (!this.activeRooms.ContainsKey(roomNumber))
			{
				var newRoomWatcher = new RoomWatcher<DefaultWebSocket>(this.auth, $"https://chat.stackoverflow.com/rooms/{roomNumber}");
				newRoomWatcher.EventRouter.AddProcessor(new ChatEventHandler());
				this.activeRooms.Add(roomNumber, newRoomWatcher);
			}
		}
	}
}
