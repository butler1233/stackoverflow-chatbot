using System;
using Newtonsoft.Json.Linq;
using SharpExchange.Chat.Events;
using StackoverflowChatbot.Extensions;

namespace StackoverflowChatbot
{
	internal class ChatEventHandler: ChatEventDataProcessor
	{
		public event Action<EventData> OnEvent;

		public override EventType[] Events { get; } = { EventType.MessagePosted, EventType.MessageEdited };

		public override void ProcessEventData(EventType eventType, JToken data)
		{
			var chatEvent = EventData.FromJson(data);

			if (!chatEvent.ContainsTrigger()) return;

			this.OnEvent?.Invoke(EventData.FromJson(data));
		}

	}
}
