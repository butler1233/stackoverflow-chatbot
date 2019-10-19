using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using SharpExchange.Chat.Events;

namespace StackoverflowChatbot
{
	internal class ChatEventHandler: ChatEventDataProcessor
	{
		public override EventType Event { get; } = EventType.MessagePosted | EventType.MessageEdited;

		public override void ProcessEventData(JToken data)
		{
			if (!IsValid(data)) return;

			var eventData = EventData.FromJson(data);
			// Send to command processor. / Maybe some preliminary master-processor first?
		}

		private static bool IsValid(JToken data)
		{
			return data.Value<int>("user_id").Equals(4364057)
				|| data.Value<string>("content").StartsWith("Sandy, ")
				|| data.Value<string>("content").StartsWith("@Sandy, ")
				|| data.Value<string>("content").StartsWith("S, ")
				|| data.Value<string>("content").StartsWith("@S, ");
		}
	}
}
