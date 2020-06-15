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

		[JsonProperty("event_type")]
		public readonly EventType Type;
		[JsonProperty("time_stamp")]
		public readonly long TimeStamp;
		[JsonProperty("content")]
		public readonly string Content;
		[JsonProperty("id")]
		public readonly int Id;
		[JsonProperty("user_id")]
		public readonly int UserId;
		[JsonProperty("user_name")]
		public readonly string Username;
		[JsonProperty("room_id")]
		public readonly int RoomId;
		[JsonProperty("room_name")]
		public readonly string RoomName;
		[JsonProperty("message_id")]
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
