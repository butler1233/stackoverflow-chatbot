using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharpExchange.Chat.Events;
using StackoverflowChatbot.Config;

namespace StackoverflowChatbot
{
	public class EventData
	{
		internal static EventData FromJson(JToken json) => json.ToObject<EventData>()!;

		private static string RemoveTriggerFrom(string content) => content.Substring(GetTriggerFrom(content).Length).Trim();

		private static string GetTriggerFrom(string content) => Manager.Config().Triggers
			.First(trigger => content.StartsWith(trigger, StringComparison.InvariantCultureIgnoreCase));

		internal bool SentByController() => Manager.Config().Controllers.Contains(UserId);

		/// <summary>
		/// The command without the trigger in the beginning.
		/// </summary>
		internal string Command => RemoveTriggerFrom(Content);

		/// <summary>
		/// Name of the command without trigger or parameters.
		/// </summary>
		internal string CommandName => Command.Contains(' ')
			? Command.Substring(0, Command.IndexOf(' '))
			: Command;

		/// <summary>
		/// Parameters to the command without trigger or command name.
		/// </summary>
		internal string? CommandParameters => Command.Contains(' ')
			? Command.Substring(Command.IndexOf(' ') + 1)
			: null;

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
			Type = type;
			TimeStamp = timeStamp;
			Content = content;
			Id = id;
			UserId = userId;
			Username = username;
			RoomId = roomId;
			RoomName = roomName;
			MessageId = messageId;
		}
	}
}
