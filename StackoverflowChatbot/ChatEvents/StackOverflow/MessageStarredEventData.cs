using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharpExchange.Chat.Events;

namespace StackoverflowChatbot.ChatEvents.StackOverflow
{
	public class MessageStarredEventData
	{
		internal static MessageStarredEventData FromJson(JToken json) => json.ToObject<MessageStarredEventData>()!;

		[JsonProperty("event_type")]
		public readonly EventType Type;
		[JsonProperty("time_stamp")]
		public readonly long TimeStamp;
		[JsonProperty("content")]
		public readonly string Content;
		[JsonProperty("id")]
		public readonly int Id;
		[JsonProperty("room_id")]
		public readonly int RoomId;
		[JsonProperty("room_name")]
		public readonly string RoomName;
		[JsonProperty("message_id")]
		public readonly int MessageId;
		[JsonProperty("message_stars")]
		public readonly int MessageStars;
	}
}
