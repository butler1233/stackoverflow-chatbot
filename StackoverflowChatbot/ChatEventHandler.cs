using System;
using Newtonsoft.Json.Linq;
using SharpExchange.Chat.Events;

namespace StackoverflowChatbot
{
	internal class ChatEventHandler: ChatEventDataProcessor
	{
		public event Action<EventData> OnEvent;

		public override EventType[] Events { get; } = { EventType.MessagePosted, EventType.MessageEdited };

		public override void ProcessEventData(EventType eventType, JToken data)
		{
			if (!IsValid(data)) return;

			this.OnEvent?.Invoke(EventData.FromJson(data));
		}

		private static bool IsValid(JToken data)
		{
			return data.Value<string>("content") != null &&
				(
				data.Value<int>("user_id").Equals(4364057)
				|| data.Value<string>("content").StartsWith("Sandy, ")
				|| data.Value<string>("content").StartsWith("@Sandy, ")
				|| data.Value<string>("content").StartsWith("S, ")
				|| data.Value<string>("content").StartsWith("@S, ")
				);
		}

		public override string ToString() => base.ToString();
	}
}
