using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
				(Program.Settings.Triggers.Any(x => data.Value<string>("content").StartsWith(x)));
		}

		public override string ToString() => base.ToString();
	}
}
