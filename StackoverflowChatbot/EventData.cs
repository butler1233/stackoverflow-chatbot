using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharpExchange.Chat.Events;

namespace StackoverflowChatbot
{
	internal class EventData
	{
		internal static EventData FromJson(JToken json) => json.ToObject<EventData>();

		internal static readonly Regex TriggerRegex = new Regex(@"@?S(andy)?, ");
		private static string RemoveTriggerFrom(string content) => TriggerRegex.Replace(content, "").Replace("please ", "");

		/// <summary>
		/// The command without the trigger in the beginning.
		/// </summary>
		internal string Command => RemoveTriggerFrom(this.Content);

		public readonly EventType Type;
		public readonly long TimeStamp;
		public readonly string Content;
		public readonly int Id;
		public readonly int UserId;
		public readonly string Username;
		public readonly int RoomId;
		public readonly string RoomName;
		public readonly int MessageId;

		[JsonConstructor]
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
