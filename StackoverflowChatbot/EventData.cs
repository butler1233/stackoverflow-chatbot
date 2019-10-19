using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using SharpExchange.Chat.Events;

namespace StackoverflowChatbot
{
	internal class EventData
	{
		internal static EventData FromJson(JToken json) => json.Value<EventData>();

		public readonly EventType Type;
		public readonly long TimeStamp;
		public readonly string Content;
		public readonly int Id;
		public readonly int UserId;
		public readonly string Username;
		public readonly int RoomId;
		public readonly string RoomName;
		public readonly int MessageId;

		private EventData(EventType type, long timeStamp, string content, int id, int userId, string username, int roomId, string roomName, int messageId)
		{
			this.Type = type;
			this.TimeStamp = timeStamp;
			this.Content = content;
			this.Id = id;
			this.UserId = userId;
			this.Username = username;
			this.RoomId = roomId;
			this.RoomName = roomName;
			this.MessageId = messageId;
		}
	}
}
