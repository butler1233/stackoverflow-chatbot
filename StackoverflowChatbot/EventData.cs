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
		private static readonly string[] emptyPhrases = { "please " };

		internal static EventData FromJson(JToken json) => json.ToObject<EventData>();

		private static string GetCommandWithoutTrigger(string content)
		{
			var withoutTrigger = content.Substring(GetTriggerFrom(content).Length).Trim();
			if (StartsWithEmptyPhrase(withoutTrigger, out var emptyPhrase))
			{
				withoutTrigger = withoutTrigger.Substring(emptyPhrase.Length).Trim();
			}

			return withoutTrigger;
		}

		private static bool StartsWithEmptyPhrase(string input, out string emptyPhrase)
		{
			emptyPhrase = emptyPhrases.FirstOrDefault(input.StartsWith);
			return emptyPhrase != null;
		}

		private static string GetTriggerFrom(string content) => Manager.Config().Triggers
			.First(trigger => content.StartsWith(trigger, StringComparison.InvariantCultureIgnoreCase));

		internal bool SentByController() => Manager.Config().Controllers.Contains(this.UserId);

		/// <summary>
		/// The command without the trigger in the beginning.
		/// </summary>
		internal string Command => GetCommandWithoutTrigger(this.Content);

		/// <summary>
		/// Name of the command without trigger or parameters.
		/// </summary>
		internal string CommandName => this.Command.Substring(0, this.Command.IndexOf(' '));

		internal string CommandParameters => this.Command.Substring(this.Command.IndexOf(' '));

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
